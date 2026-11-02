param([string]$RimWorldDir = 'C:\Games\RimWorld')

$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent

$managed = Join-Path $RimWorldDir 'RimWorldWin64_Data\Managed'

Get-ChildItem $managed -Filter *.dll | ForEach-Object {

    try { [Reflection.Assembly]::LoadFrom($_.FullName) | Out-Null } catch { }

}

$game = [Reflection.Assembly]::LoadFrom("$managed\Assembly-CSharp.dll")

$index = @{}

$parents = @{}

Get-ChildItem (Join-Path $RimWorldDir 'Data\Core\Defs') -Recurse -Filter *.xml | ForEach-Object {

    [xml]$doc = Get-Content -LiteralPath $_.FullName

    foreach ($node in $doc.DocumentElement.ChildNodes) {

        if ($node.NodeType -ne 'Element') { continue }

        if ($node.defName) { $index["$($node.LocalName):$($node.defName)"] = $node }

        if ($node.HasAttribute('Name')) { $parents[$node.GetAttribute('Name')] = $node }

    }

}

# Register mod abstract parents before validating children, independent of file order.
foreach ($file in Get-ChildItem (Join-Path $root 'Defs') -Filter *.xml) {
    [xml]$document = Get-Content -LiteralPath $file.FullName
    foreach ($node in $document.Defs.ChildNodes) {
        if ($node.NodeType -eq 'Element' -and $node.HasAttribute('Name')) {
            $name = $node.GetAttribute('Name')
            if ($parents.ContainsKey($name)) { throw "Duplicate XML parent: $name" }
            $parents[$name] = $node
        }
    }
}

[xml]$defs = Get-Content (Join-Path $root 'Defs\LightningHeart.xml')

[xml]$about = Get-Content (Join-Path $root 'About\About.xml')

foreach ($defFile in Get-ChildItem (Join-Path $root 'Defs') -Filter *.xml) {

[xml]$defDoc = Get-Content -LiteralPath $defFile.FullName

foreach ($node in $defDoc.Defs.ChildNodes) {

    if ($node.NodeType -ne 'Element') { continue }

    if ($node.HasAttribute('ParentName') -and !$parents.ContainsKey($node.ParentName)) { throw "Missing XML parent: $($node.ParentName)" }

    $key = if ($node.defName) { "$($node.LocalName):$($node.defName)" } else { "Abstract:$($node.GetAttribute('Name'))" }

    if ($index.ContainsKey($key)) { throw "Duplicate Def: $key" }

    $index[$key] = $node

    $type = $game.GetType("Verse.$($node.LocalName)"); if (!$type) { $type = $game.GetType("RimWorld.$($node.LocalName)") }

    foreach ($field in $node.ChildNodes) {

        if ($field.NodeType -eq 'Element' -and !$type.GetField($field.Name, [Reflection.BindingFlags]'Public,NonPublic,Instance')) {

            throw "Unknown field $($type.FullName).$($field.Name)"

        }

    }

}

}

$stage = $defs.SelectSingleNode('Defs/HediffDef/stages/li')

$stageType = $game.GetType('Verse.HediffStage')

foreach ($field in $stage.ChildNodes) {

    if ($field.NodeType -ne 'Element') { continue }

    if (!$stageType.GetField($field.Name)) { throw "Missing HediffStage field: $($field.Name)" }

}

foreach ($stat in $defs.SelectNodes('//statFactors/* | //statOffsets/* | //statBases/*')) {

    if (!$index.ContainsKey("StatDef:$($stat.Name)")) { throw "Missing Core StatDef: $($stat.Name)" }

}

foreach ($ref in @('BodyPartDef:Torso', 'ThingDef:Human', 'ThingCategoryDef:Medicine', 'ThingCategoryDef:BodyPartsArchotech')) {

    if (!$index.ContainsKey($ref)) { throw "Missing Core reference: $ref" }

}

if ($about.ModMetaData.modDependencies.li.packageId -ne 'brrainz.harmony') { throw 'Harmony dependency absent' }

if (@($about.ModMetaData.loadAfter.li) -notcontains 'brrainz.harmony') { throw 'Harmony loadAfter absent' }

$install = $defs.SelectSingleNode('Defs/RecipeDef[defName="EH_InstallLightningHeart"]')

$remove = $defs.SelectSingleNode('Defs/RecipeDef[defName="EH_RemoveLightningHeart"]')

$hediff = $defs.SelectSingleNode('Defs/HediffDef')

if ($install.addsHediff -ne $hediff.defName -or $remove.removesHediff -ne $hediff.defName) { throw 'Surgery hediff mismatch' }

if ($hediff.spawnThingOnRemoved) { throw 'Ascension must not return an implant item' }

$item = $defs.SelectSingleNode('Defs/ThingDef')

if (!(Test-Path (Join-Path $root "Textures/$($item.graphicData.texPath).png"))) { throw 'Missing texture' }

if (!(Test-Path (Join-Path $root 'Assemblies/ElementalHearts.dll'))) { throw 'Missing compiled assembly' }

$own = [Reflection.Assembly]::LoadFrom((Join-Path $root 'Assemblies/ElementalHearts.dll'))

foreach ($compNode in $defs.SelectNodes('Defs/HediffDef/comps/li')) {

    $compType = $own.GetType($compNode.GetAttribute('Class'))

    if (!$compType) { throw "Missing component class" }

    foreach ($field in $compNode.ChildNodes) {

        if ($field.NodeType -eq 'Element' -and !$compType.GetField($field.LocalName)) { throw "Unknown tuning field: $($field.LocalName)" }

    }

}

if (!$index.ContainsKey('DamageDef:EH_Lightning') -or !$index.ContainsKey('JobDef:EH_HeavenlyThunder')) { throw 'Missing Phase 2 definitions' }

if (!(Test-Path (Join-Path $root 'Textures/Effects/ElementalHearts/Lightning/AbilityAtlas.png'))) { throw 'Missing ability atlas' }

if (!$index.ContainsKey('HediffDef:EH_Charged')) { throw 'Missing charge definition' }

Write-Output "PASS: all Def XML files parse; Def parents/direct fields exist in local Core/assembly."

Write-Output 'PASS: all selected stage fields, stats, body/category references, serum/recipe links and texture exist.'

Write-Output 'PASS: Harmony dependency/load order and compiled assembly present.'

Write-Output 'PASS: ability tuning fields, custom lightning DamageDef, ultimate JobDef and effect atlas exist.'

Write-Output 'LIMIT: this script performs static validation. See Docs/Phase4.3.md for separate runtime test results.'












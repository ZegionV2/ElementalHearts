param([string]$RimWorldDir='C:\Games\RimWorld')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$managed=Join-Path $RimWorldDir 'RimWorldWin64_Data\Managed'
Get-ChildItem $managed -Filter '*.dll' | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
$game=[Reflection.Assembly]::LoadFrom("$managed\Assembly-CSharp.dll")
$own=[Reflection.Assembly]::LoadFrom("$root\Assemblies\ElementalHearts.dll")
foreach($file in @('AdamantAscendant.xml','AdamantGear.xml','AdamantSounds.xml')){
 [xml]$xml=Get-Content "$root\Defs\$file"
 foreach($d in $xml.Defs.ChildNodes){if($d.NodeType -ne 'Element'){continue};$type=$game.GetType('Verse.'+$d.Name);if(!$type){$type=$game.GetType('RimWorld.'+$d.Name)};if(!$type){throw "Unknown native definition $($d.Name)"};foreach($n in $d.ChildNodes){if($n.NodeType -eq 'Element' -and !$type.GetField($n.Name)){throw "Invalid native field $($d.Name)/$($n.Name)"}};foreach($comp in $d.SelectNodes('comps/li[@Class]')){if(!$own.GetType($comp.Class) -and !$game.GetType('Verse.'+$comp.Class) -and !$game.GetType('RimWorld.'+$comp.Class)){throw "Unknown native component $($comp.Class)"}}}
}
[xml]$defs=Get-Content "$root\Defs\AdamantAscendant.xml"
$heart=$defs.SelectSingleNode("Defs/HediffDef[defName='EH_AdamantAscendant']")
if($heart.ParentName -ne 'EH_ElementalHeartHediffBase' -or $heart.stages.li.statFactors.MoveSpeed -ne '5' -or $heart.stages.li.statFactors.IncomingDamageFactor -ne '.08'){throw 'Incorrect evolved tier'}
if(!$own.GetType('ElementalHearts.AdamantEvolution') -or !$own.GetType('ElementalHearts.HediffComp_AdamantMastery')){throw 'Missing native evolution implementation'}
[xml]$gear=Get-Content "$root\Defs\AdamantGear.xml"
if($gear.Defs.ThingDef.Count -ne 2 -or $gear.SelectNodes("Defs/ThingDef[contains(defName,'Helmet')]").Count -ne 0){throw 'Evolved kit must have no helmet'}
foreach($d in $gear.Defs.ThingDef){if($d.useHitPoints -ne 'false' -or $d.recipeMaker.IsNull -ne 'True'){throw 'Regalia and Worldbreaker must be indestructible projections'}}
Add-Type -AssemblyName System.Drawing
$textures=@(Get-ChildItem "$root\Textures" -Recurse -Filter '*.png' | Where-Object {$_.FullName -match '[\\/]Adamant[\\/]|[\\/]AdamantRegalia[\\/]|[\\/]AdamantSword\.png$'})
if($textures.Count -ne 83){throw "Expected 83 Adamant textures; found $($textures.Count)"}
foreach($p in $textures){$b=[Drawing.Bitmap]::new($p.FullName);try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0){throw "Bad Adamant RGBA export: $p"}}finally{$b.Dispose()}}
$frames="$root\Textures\Effects\ElementalHearts\Adamant\Frames"
foreach($name in @('AdamantSeverance','MountainKingsAdvance','SovereignQuake','WorldspineEruption','BulwarkFormation','BulwarkFade','Halo')){$expected=if($name -eq 'Halo'){6}else{8};$f=@(Get-ChildItem $frames -Filter ($name+'_*.png'));if($f.Count -ne $expected -or @($f|Get-FileHash|Select-Object -ExpandProperty Hash -Unique).Count -ne $expected){throw "Invalid frame sequence $name"}}
Write-Output 'PASS: Adamant native definitions/components, evolved stat tier, helmetless indestructible kit, 83 transparent native textures and 54 distinct sequence phases.'

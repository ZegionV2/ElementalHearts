$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Add-Type -AssemblyName System.Drawing
Get-ChildItem 'C:/Games/RimWorld/RimWorldWin64_Data/Managed' -Filter '*.dll' | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
$game=[Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll')
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
[xml]$gear=Get-Content -LiteralPath "$root/Defs/WaterGear.xml"
[xml]$water=Get-Content -LiteralPath "$root/Defs/WaterHeart.xml"
[xml]$normal=Get-Content -LiteralPath "$root/Defs/LightningHeart.xml"
$heart=$water.SelectSingleNode("Defs/HediffDef[defName='EH_WaterHeart']")
$reference=$normal.SelectSingleNode("Defs/HediffDef[defName='EH_LightningHeart']/stages")
if($heart.stages.OuterXml -ne $reference.OuterXml){throw 'Water ordinary stat stage differs from the selected normal Ascendant package'}
$defense=$heart.SelectNodes("comps/li[@Class='ElementalHearts.HediffCompProperties_WaterDefense']")
if($defense.Count -ne 1 -or $defense[0].ChildNodes.Count -ne 0){throw 'Water defense component/tuning altered'}
$recovery=$heart.SelectSingleNode("comps/li[@Class='ElementalHearts.HediffCompProperties_LightningRecovery']")
if(!$recovery -or $recovery.sealAfterTicks -ne '180' -or $recovery.healPerSecond -ne '2'){throw 'Normal recovery values missing'}
if($water.SelectSingleNode("Defs/ThoughtDef[defName='EH_WaterResolve']/stages/li/baseMoodEffect").InnerText -ne '30'){throw 'Water ordinary mood changed'}
$count=0
foreach($suffix in @('Uniform','Helmet','Sword')){
 $def=$gear.SelectSingleNode("Defs/ThingDef[defName='EH_Water$suffix']")
 if(!$def -or $def.useHitPoints -ne 'false' -or !$def.SelectSingleNode("modExtensions/li[@Class='ElementalHearts.IndestructibleElementalGear']") -or !$def.SelectSingleNode("comps/li[@Class='ElementalHearts.CompProperties_IndestructibleGear']")){throw "Water $suffix is not indestructible"}
 if($def.generateCommonality -and $def.generateCommonality -ne '0'){throw 'Unexpected generated Water gear'}
 $paths=@("$root/Textures/$($def.graphicData.texPath).png")
 if($suffix -eq 'Uniform'){
  foreach($body in @('Male','Female','Thin','Fat','Hulk','Child','Baby')){foreach($dir in @('south','east','north')){$paths+="$root/Textures/$($def.apparel.wornGraphicPath)_${body}_$dir.png"}}
  foreach($part in @('Torso','Shoulders','Arms','Hands','Waist','Legs','Feet')){if($def.apparel.bodyPartGroups.li -notcontains $part){throw "Missing body coverage: $part"}}
 }elseif($suffix -eq 'Helmet'){
  foreach($dir in @('south','east','north')){$paths+="$root/Textures/$($def.apparel.wornGraphicPath)_$dir.png"}
  if($def.apparel.layers.li -ne 'Overhead' -or $def.apparel.bodyPartGroups.li -ne 'FullHead' -or $def.apparel.renderSkipFlags.li -contains 'Head'){throw 'Water helmet hides its own rendering branch'}
  foreach($flag in @('Hair','Beard')){if($def.apparel.renderSkipFlags.li -notcontains $flag){throw "Helmet exposes $flag"}}
 }
 foreach($p in $paths){$b=[Drawing.Bitmap]::new($p);try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0 -or $b.GetPixel(255,255).A -ne 0){throw "Invalid Water sprite: $p"}}finally{$b.Dispose()};$count++}
 foreach($node in $def.apparel.ChildNodes){if($node.NodeType -eq 'Element' -and !$game.GetType('RimWorld.ApparelProperties').GetField($node.Name)){throw "Invalid apparel field: $($node.Name)"}}
}
if(!$own.GetType('ElementalHearts.GameComponent_WaterKitMigration') -or !$own.GetType('ElementalHearts.Patches.WaterInjuryEfficiency') -or !$own.GetType('ElementalHearts.Patches.WaterNoHitSlowdown')){throw 'Water migration/resilience missing'}
Write-Output "PASS: $count transparent 256px Water sprites, full-body/head coverage definitions, hair concealment, indestructible gear, ordinary stat stage, recovery, +30 mood and migration/resilience bindings."
Write-Output 'LIMIT: actual pawn fit, Melee Animation visuals, native save loading and live recovery/transformation remain in-game checks.'

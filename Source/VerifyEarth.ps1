param([string]$RimWorldDir='C:\Games\RimWorld')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$managed=Join-Path $RimWorldDir 'RimWorldWin64_Data\Managed'
Get-ChildItem $managed -Filter '*.dll' | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
[void][Reflection.Assembly]::LoadFrom((Join-Path $RimWorldDir 'Mods\2009463077\1.6\Assemblies\0Harmony.dll'))
$game=[Reflection.Assembly]::LoadFrom("$managed\Assembly-CSharp.dll")
$own=[Reflection.Assembly]::LoadFrom("$root\Assemblies\ElementalHearts.dll")
[xml]$defs=Get-Content "$root\Defs\EarthHeart.xml"
[xml]$gear=Get-Content "$root\Defs\EarthGear.xml"
foreach($document in @($defs,$gear)){
 foreach($node in $document.Defs.ChildNodes){
  if($node.NodeType -ne 'Element'){continue}
  $type=$game.GetType('Verse.'+$node.Name)
  if(!$type){$type=$game.GetType('RimWorld.'+$node.Name)}
  if(!$type){throw "Unknown native definition type $($node.Name)"}
  foreach($field in $node.ChildNodes){if($field.NodeType -eq 'Element' -and !$type.GetField($field.Name)){throw "Unknown native $($node.Name) field: $($field.Name)"}}
  foreach($comp in $node.SelectNodes('comps/li[@Class]')){
   $compType=$own.GetType($comp.Class)
   if(!$compType){$compType=$game.GetType('Verse.'+$comp.Class)}
   if(!$compType){$compType=$game.GetType('RimWorld.'+$comp.Class)}
   if(!$compType){throw "Unknown component $($comp.Class)"}
   foreach($field in $comp.ChildNodes){if($field.NodeType -eq 'Element' -and !$compType.GetField($field.Name)){throw "Unknown tuning field $($comp.Class)/$($field.Name)"}}
  }
 }
}
$heart=$defs.SelectSingleNode("Defs/HediffDef[defName='EH_EarthHeart']")
$training=$defs.SelectSingleNode("Defs/HediffDef[defName='EH_EarthTraining']")
if($heart.ParentName -ne 'EH_ElementalHeartHediffBase' -or $training.tags -or $training.stages -or $training.comps.li.Count -ne 2){throw 'Lineage/disciple tier isolation failed'}
if($training.comps.li[0].cooldownFactor -ne '1.5'){throw 'Disciple cooldown scaling failed'}
if($defs.SelectSingleNode("Defs/ThoughtDef[defName='EH_EarthResolve']/stages/li/baseMoodEffect").InnerText -ne '30'){throw 'Normal Earth mood tier failed'}
$job=$defs.SelectSingleNode("Defs/JobDef[defName='EH_EarthTechnique']")
if(!$own.GetType($job.driverClass) -or $job.playerInterruptible -ne 'true' -or $job.suspendable -ne 'false'){throw 'Native cast interruption policy failed'}
if($gear.Defs.ThingDef.Count -ne 3 -or $gear.SelectNodes("Defs/ThingDef[defName='EH_EarthHelmet']").Count -ne 1){throw 'Earth requires one stoneplate, enclosed helmet and sword'}
Add-Type -AssemblyName System.Drawing
$earthTextures=@(Get-ChildItem "$root\Textures" -Recurse -Filter '*.png' | Where-Object {$_.FullName -match '[\\/]Earth[\\/]|[\\/]Earth(Uniform|Helmet)[\\/]|[\\/]Earth(Sword|Serum)\.png$'})
if($earthTextures.Count -ne 92){throw "Expected 92 Earth textures; got $($earthTextures.Count)"}
foreach($file in $earthTextures){$b=[Drawing.Bitmap]::new($file.FullName);try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.PixelFormat.ToString() -notmatch 'Argb'){throw "Bad Earth RGBA export $file"}}finally{$b.Dispose()}}
foreach($sequence in @('FaultlineCleave','StonebreakerAdvance','BastionStance','Mountainfall','Idle','Combat','Activation')){
 $frames=@(Get-ChildItem "$root\Textures\Effects\ElementalHearts\Earth\Frames" -Filter ($sequence+'_*.png'))
 if($frames.Count -ne 6 -or @($frames|Get-FileHash|Select-Object -ExpandProperty Hash -Unique).Count -ne 6){throw "Missing or duplicate animation phases: $sequence"}
}
foreach($sequence in @('SpireEruption','SeismicQuake')){
 $frames=@(Get-ChildItem "$root\Textures\Effects\ElementalHearts\Earth\Frames" -Filter ($sequence+'_*.png'))
 if($frames.Count -ne 8 -or @($frames|Get-FileHash|Select-Object -ExpandProperty Hash -Unique).Count -ne 8){throw "Missing or duplicate seismic animation phases: $sequence"}
}
$helmet=$gear.SelectSingleNode("Defs/ThingDef[defName='EH_EarthHelmet']")
if($helmet.apparel.bodyPartGroups.li -ne 'FullHead' -or $helmet.apparel.layers.li -ne 'Overhead' -or $helmet.useHitPoints -ne 'false' -or @($helmet.apparel.renderSkipFlags.li).Count -ne 2){throw 'Helmet coverage, concealment or indestructibility failed'}
foreach($direction in @('south','east','north')){if(!(Test-Path "$root\Textures\Things\Pawn\Humanlike\Apparel\EarthHelmet\EarthHelmet_$direction.png")){throw "Missing helmet facing $direction"}}
foreach($body in @('Male','Female','Thin','Fat','Hulk','Child','Baby')){foreach($direction in @('south','east','north')){if(!(Test-Path (Join-Path "$root\Textures\Things\Pawn\Humanlike\Apparel\EarthUniform" ('EarthUniform_'+$body+'_'+$direction+'.png')))){throw "Missing Earth body/direction: $body $direction"}}}
[xml]$sounds=Get-Content "$root\Defs\EarthSounds.xml"
$corePaths=@{}
Get-ChildItem (Join-Path $RimWorldDir 'Data\Core\Defs\SoundDefs') -Filter '*.xml' | ForEach-Object {[xml]$d=Get-Content $_.FullName;foreach($n in $d.SelectNodes('//clipPath|//clipFolderPath')){$corePaths[$n.InnerText]=$true}}
foreach($sound in $sounds.Defs.SoundDef){foreach($grain in $sound.SelectNodes('.//clipPath|.//clipFolderPath')){
 $p=$grain.InnerText;if($corePaths.ContainsKey($p)){continue}
 if($grain.Name -eq 'clipFolderPath'){$folder=Join-Path "$root\Sounds" $p;if(!(Test-Path -LiteralPath $folder) -or @(Get-ChildItem -LiteralPath $folder -Filter '*.wav').Count -ne 2){throw "Missing Earth sound variants $p"}}
 elseif(!(Test-Path "$root\Sounds\$p.wav") -and !(Test-Path "$root\Sounds\$p.ogg")){throw "Missing Earth sound $p"}
}}
Write-Output 'PASS: native Earth definition/component/recipe/job bindings, disciple scaling, indestructible three-piece kit, 92 RGBA textures, 21 body exports, 3 helmet facings/native west, 58 distinct effect frames and shared sound samples.'


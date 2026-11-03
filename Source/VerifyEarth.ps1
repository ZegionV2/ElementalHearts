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
if($gear.Defs.ThingDef.Count -ne 2 -or $gear.SelectNodes("Defs/ThingDef[contains(defName,'Helmet')]").Count -ne 0){throw 'Earth should have exactly stoneplate and sword, no helmet or evolved halo'}
Add-Type -AssemblyName System.Drawing
$earthTextures=@(Get-ChildItem "$root\Textures" -Recurse -Filter '*.png' | Where-Object {$_.FullName -match '[\\/]Earth[\\/]|[\\/]EarthUniform[\\/]|[\\/]Earth(Sword|Serum)\.png$'})
if($earthTextures.Count -ne 70){throw "Expected 70 Earth textures; got $($earthTextures.Count)"}
foreach($file in $earthTextures){$b=[Drawing.Bitmap]::new($file.FullName);try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.PixelFormat.ToString() -notmatch 'Argb'){throw "Bad Earth RGBA export $file"}}finally{$b.Dispose()}}
foreach($sequence in @('FaultlineCleave','StonebreakerAdvance','BastionStance','Mountainfall','Idle','Combat','Activation')){
 $frames=@(Get-ChildItem "$root\Textures\Effects\ElementalHearts\Earth\Frames" -Filter ($sequence+'_*.png'))
 if($frames.Count -ne 6 -or @($frames|Get-FileHash|Select-Object -ExpandProperty Hash -Unique).Count -ne 6){throw "Missing or duplicate animation phases: $sequence"}
}
foreach($body in @('Male','Female','Thin','Fat','Hulk','Child','Baby')){foreach($direction in @('south','east','north')){if(!(Test-Path (Join-Path "$root\Textures\Things\Pawn\Humanlike\Apparel\EarthUniform" ('EarthUniform_'+$body+'_'+$direction+'.png')))){throw "Missing Earth body/direction: $body $direction"}}}
[xml]$sounds=Get-Content "$root\Defs\EarthSounds.xml"
foreach($sound in $sounds.Defs.SoundDef){foreach($grain in $sound.SelectNodes('.//clipPath')){if(!(Test-Path "$root\Sounds\$($grain.InnerText).wav") -and !(Test-Path "$root\Sounds\$($grain.InnerText).ogg")){throw "Missing shared sound $($grain.InnerText)"}}}
Write-Output 'PASS: native Earth definition/field/component/recipe/job bindings, ordinary tier, disciple scaling, two-piece helmetless kit, 70 RGBA textures, all 21 body/direction exports, 42 distinct animation frames and existing shared sound samples.'


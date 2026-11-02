$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Get-ChildItem C:/Games/RimWorld/RimWorldWin64_Data/Managed -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
$game=[Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll')
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
[xml]$defs=Get-Content "$root/Defs/CelestialAscendant.xml"
foreach($node in $defs.Defs.ChildNodes){if($node.NodeType -ne 'Element'){continue};$type=$game.GetType('Verse.'+$node.Name);if(!$type){$type=$game.GetType('RimWorld.'+$node.Name)};if(!$type){$type=$game.GetType('RimWorld.Planet.'+$node.Name)};if(!$type){throw "Missing native definition type $($node.Name)"};foreach($field in $node.ChildNodes){if($field.NodeType -eq 'Element' -and !$type.GetField($field.Name)){throw "Unknown $($node.Name) field $($field.Name)"}}}
foreach($job in $defs.Defs.JobDef){if(!$own.GetType($job.driverClass) -or $job.playerInterruptible -ne 'true' -or $job.suspendable -ne 'false'){throw 'Celestial job class/cleanup policy missing'}}
$heart=$defs.SelectSingleNode("Defs/HediffDef[defName='EH_CelestialAscendant']")
foreach($comp in $heart.comps.li){if(!$own.GetType($comp.Class)){throw "Missing Celestial component $($comp.Class)"}}
if($heart.stages.li.statFactors.MoveSpeed -ne '6.5' -or $heart.stages.li.statFactors.IncomingDamageFactor -ne '0.04' -or $heart.stages.li.statFactors.CarryingCapacity -ne '15'){throw 'Celestial stat tier regression'}
if($defs.SelectSingleNode("Defs/ThoughtDef[defName='EH_CelestialResolve']/stages/li/baseMoodEffect").InnerText -ne '75'){throw 'Celestial mood regression'}
$radiance=$defs.SelectSingleNode("Defs/HediffDef[defName='EH_CelestialRadiance']");if(!$radiance -or $radiance.stages){throw 'Radiance grants unwanted ambient/stat effects'}
$source=Get-Content "$root/Source/CelestialVfx.cs" -Raw
foreach($s in @('EffectLimit=128','EchoLimit=32','AdornmentLimit=128','MaxDuration=300','CurrentViewRect','Fogged(map)','FinalizeInit','CelestialArmorFit.Echo')){if(!$source.Contains($s)){throw "Missing visual bound/cleanup: $s"}}
foreach($s in @('TakeDamage(','GenSpawn.','SetTerrain(','weatherManager','roofGrid','StunFor(')){if($source.Contains($s)){throw "Presentation changes gameplay: $s"}}
$textureRoot=[IO.Path]::GetFullPath("$root/Textures")
$files=@(Get-ChildItem $textureRoot -Recurse -Filter '*.png' | Where-Object {$_.FullName.Substring($textureRoot.Length) -match 'Celestial'});if($files.Count -ne 174){throw 'Expected 174 Celestial textures'}
if($defs.SelectNodes("Defs/HediffDef[contains(defName,'Training')]").Count -ne 0 -or $defs.SelectNodes("Defs/JobDef[contains(defName,'Evolve')]").Count -ne 0){throw 'Celestial should arrive fully evolved'}
Write-Output 'PASS: Celestial native definition fields, job bindings, standalone lineage, stronger stats/mood, resource-free bounded visual provider and all 174 textures.'

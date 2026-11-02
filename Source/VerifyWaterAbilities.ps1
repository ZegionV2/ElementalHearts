$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Add-Type -AssemblyName System.Drawing
[xml]$water=Get-Content -LiteralPath "$root/Defs/WaterHeart.xml"
[xml]$abilities=Get-Content -LiteralPath "$root/Defs/WaterAbilities.xml"
$heart=$water.SelectSingleNode("Defs/HediffDef[defName='EH_WaterHeart']")
foreach($class in @('WaterDefense','WaterAbilities','LightningRecovery')){if($heart.SelectNodes("comps/li[@Class='ElementalHearts.HediffCompProperties_$class']").Count -ne 1){throw "Missing or duplicate Water component: $class"}}
$pressure=$abilities.SelectSingleNode("Defs/HediffDef[defName='EH_WaterPressure']")
if($pressure.maxSeverity -ne '3' -or $pressure.stages.li.Count -ne 3){throw 'Pressure stage budget invalid'}
if(($pressure.stages.li | ForEach-Object {$_.statFactors.MoveSpeed}) -join ',' -ne '0.95,0.9,0.85'){throw 'Pressure slow exceeds intended tier'}
$damage=$abilities.SelectSingleNode("Defs/DamageDef[defName='EH_WaterDamage']")
if($damage.ParentName -ne 'BluntBase' -or $damage.workerClass -ne 'DamageWorker_AddInjury' -or $damage.armorCategory -ne 'Blunt' -or $damage.buildingDamageFactor -ne '0'){throw 'Unexpected Water damage policy'}
$job=$abilities.SelectSingleNode("Defs/JobDef[defName='EH_WaterTechnique']")
if($job.playerInterruptible -ne 'true' -or $job.suspendable -ne 'false' -or $job.neverFleeFromEnemies -ne 'true'){throw 'Water technique interruption policy invalid'}
$count=0
foreach($sequence in @('Torrent','Flow','Whirlpool','Judgement','Sigil')){
 $paths=@(0..7 | ForEach-Object {"$root/Textures/Effects/ElementalHearts/Water/Frames/${sequence}_$('{0:00}' -f $_).png"})
 if((@($paths | ForEach-Object {Get-FileHash -LiteralPath $_} | Select-Object -ExpandProperty Hash | Sort-Object -Unique)).Count -ne 8){throw "Duplicate frames in $sequence"}
 foreach($p in $paths){$b=[Drawing.Bitmap]::new($p);try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0 -or $b.GetPixel(255,255).A -ne 0){throw "Invalid effect: $p"}}finally{$b.Dispose()};$count++}
}
foreach($name in @('TorrentCleave','FlowingStep','WhirlpoolDance','OceansJudgement')){$b=[Drawing.Bitmap]::new("$root/Textures/UI/Commands/ElementalHearts/Water/$name.png");try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0){throw "Invalid icon: $name"}}finally{$b.Dispose()};$count++}
$powers=Get-Content -LiteralPath "$root/Source/WaterAbilities.cs" -Raw
if($powers.Contains('defaultLabel="Auto Combat')){throw 'Water active skills add a duplicate Auto Combat control'}
$defense=Get-Content -LiteralPath "$root/Source/WaterDefense.cs" -Raw
foreach($key in @('waterAuto','waterNonLethal','waterReadyAt','waterCounterTarget','waterCounterAt','waterElapsed','waterStruck')){if(!$defense.Contains('"'+$key+'"')){throw "Lost legacy Water save key: $key"}}
Write-Output "PASS: $count transparent 256px Water ability assets, eight unique frames in each sequence, four icons, bounded Pressure tiers, single-injury damage, interruption JobDef, shared control and legacy save keys."
Write-Output 'LIMIT: live hit timing, animation placement, native load and actual automated combat need an in-game check.'

$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
[xml]$defs=Get-Content "$root/Defs/DiscipleAwakening.xml"
$grace=$defs.SelectSingleNode("Defs/HediffDef[defName='EH_AwakeningGrace']")
if($grace.comps.li.disappearsAfterTicks -ne '180' -or $grace.stages.li.statFactors.IncomingDamageFactor -ne '0.1'){throw 'Awakening grace lifetime/protection incorrect'}
$damage=$defs.SelectSingleNode("Defs/DamageDef[defName='EH_AwakeningShockwave']")
if($damage.workerClass -ne 'DamageWorker_AddInjury' -or $damage.buildingDamageFactor -ne '0'){throw 'Shockwave damage policy incorrect'}
$source=Get-Content "$root/Source/DiscipleAwakening.cs" -Raw
foreach($part in @('hit.disciple.awakeningRolled=true','finally{','current=hit.previous','missingBefore.Contains','HediffDefOf.BloodLoss','IsPermanent()')){if(!$source.Contains($part)){throw "Awakening invariant missing: $part"}}
$vfx=Get-Content "$root/Source/AwakeningBurst.cs" -Raw
foreach($part in @('spent<4','targets.Count>=16','ids.Add','pending.RemoveAt(0)','EH_awakeningPulses')){if(!$vfx.Contains($part)){throw "Shockwave work/ownership bound missing: $part"}}
Write-Output 'PASS: three-second 90% protection, one-injury-packet shockwave, bounded per-tick/victim work, saved burst queue, one-time roll and scoped repair safeguards.'

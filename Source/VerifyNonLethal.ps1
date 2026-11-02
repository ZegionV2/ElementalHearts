param([string]$RimWorldDir='C:/Games/RimWorld')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$managed="$RimWorldDir/RimWorldWin64_Data/Managed"
Get-ChildItem $managed -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
[void][Reflection.Assembly]::LoadFrom("$RimWorldDir/Mods/2009463077/1.6/Assemblies/0Harmony.dll")
$game=[Reflection.Assembly]::LoadFrom("$managed/Assembly-CSharp.dll")
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
$modes=$own.GetType('ElementalHearts.ElementalAutoModes');$mode=$own.GetType('ElementalHearts.ElementalAutoMode')
foreach($pair in @(@('On','Off'),@('Off','NonLethal'),@('NonLethal','On'))){
 $actual=$modes.GetMethod('Next').Invoke($null,@([Enum]::Parse($mode,$pair[0])))
 if($actual.ToString() -ne $pair[1]){throw 'Cycle order failed'}
}
foreach($case in @(@($false,$false,'Off'),@($true,$false,'On'),@($true,$true,'NonLethal'),@($false,$true,'Off'))){
 if($modes.GetMethod('FromFlags').Invoke($null,[object[]]$case[0..1]).ToString() -ne $case[2]){throw 'Saved flags migration failed'}
}
$policy=$own.GetType('ElementalHearts.ElementalNonLethal')
if($policy.GetField('KnockdownChance').GetRawConstantValue() -ne .75){throw 'Knockdown tuning changed'}
foreach($force in @(-100,0,10,100,10000)){
 $duration=$policy.GetMethod('StunDuration').Invoke($null,@([single]$force))
 if($duration -lt 45 -or $duration -gt 90){throw 'Stun duration outside safe bounds'}
}
$verb=$game.GetType('RimWorld.Verb_MeleeAttackDamage')
if(!$verb){$verb=$game.GetType('Verse.Verb_MeleeAttackDamage')}
$method=$verb.GetMethod('ApplyMeleeDamageToTarget',[Reflection.BindingFlags]'Instance,Public,NonPublic')
if($method.GetParameters()[0].ParameterType.FullName -ne 'Verse.LocalTargetInfo' -or $method.ReturnType.FullName -ne 'Verse.DamageWorker+DamageResult'){throw 'Melee patch signature mismatch'}
[xml]$xml=Get-Content "$root/Defs/ElementalSuppression.xml"
$def=$xml.Defs.HediffDef
if($def.comps.li.disappearsAfterTicks -ne '1800'){throw 'Suppression expiry mismatch'}
foreach($cap in $def.stages.li.capMods.li){if([float]$cap.setMax -le 0 -or [double]$cap.setMax -gt 0.201){throw 'Unsafe suppression capacity'}}
foreach($element in @('Lightning','Flame')){
 $source=Get-Content "$PSScriptRoot/${element}Abilities.cs" -Raw
 if(!$source.Contains('"EH_nonLethal",false') -or !$source.Contains('action=CycleAutoMode')){throw 'Mode persistence or button missing'}
 $strike=$source.Substring($source.IndexOf('public static bool Strike('))
 if($strike.IndexOf('ElementalNonLethal.TryApply') -gt $strike.IndexOf('victim.TakeDamage')){throw 'Damage bypasses restraint'}
}
Write-Output 'PASS: three-state cycle, legacy flags, knockdown tuning, bounded stun, installed melee API, temporary safe capacity caps, saved mode and elemental damage interception.'
Write-Output 'LIMIT: live-game knockdown and save/reload still need in-game verification.'



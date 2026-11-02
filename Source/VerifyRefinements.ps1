param([string]$RimWorldDir='C:/Games/RimWorld')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$managed=Join-Path $RimWorldDir 'RimWorldWin64_Data/Managed'
Get-ChildItem $managed -Filter *.dll|ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
[void][Reflection.Assembly]::LoadFrom("$RimWorldDir/Mods/2009463077/1.6/Assemblies/0Harmony.dll")
$game=[Reflection.Assembly]::LoadFrom("$managed/Assembly-CSharp.dll")
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
$policy=$own.GetType('ElementalHearts.ElementalCombatPolicy')
$cases=@(
 @($true,$false,$false,$false,$true), # deliberately selected colonist / neutral
 @($true,$false,$true,$false,$true),  # deliberately selected prisoner
 @($false,$false,$false,$true,$true), # enemy in normal combat
 @($false,$false,$false,$false,$false), # unintended friendly remains protected
 @($false,$false,$true,$true,$false), # prisoner remains protected
 @($false,$true,$false,$false,$true), # explicit global friendly-fire opt-in
 @($false,$true,$true,$false,$true)
)
foreach($case in $cases){$actual=$policy.GetMethod('TargetAllowed').Invoke($null,[object[]]$case[0..3]);if($actual -ne $case[4]){throw 'Target policy regression'}}
foreach($global in @($false,$true)){foreach($toggle in @($false,$true)){
 if(!$policy.GetMethod('AutomationEnabled').Invoke($null,[object[]]@($false,$global,$toggle))){throw 'NPC automation can be disabled'}
 $actual=$policy.GetMethod('AutomationEnabled').Invoke($null,[object[]]@($true,$global,$toggle))
 if($actual -ne ($global -and $toggle)){throw 'Player toggle regression'}
}}
# Scope cleanup/nesting must not turn friendly fire on for unrelated pawns or later attacks.
$pawnType=$game.GetType('Verse.Pawn')
$caster=[Runtime.Serialization.FormatterServices]::GetUninitializedObject($pawnType)
$first=[Runtime.Serialization.FormatterServices]::GetUninitializedObject($pawnType)
$second=[Runtime.Serialization.FormatterServices]::GetUninitializedObject($pawnType)
$scopeType=$own.GetType('ElementalHearts.ElementalManualTarget');$explicit=$scopeType.GetMethod('Explicit')
$scope=[Activator]::CreateInstance($scopeType,[object[]]@($caster,$first))
try {
 if(!$explicit.Invoke($null,[object[]]@($caster,$first)) -or $explicit.Invoke($null,[object[]]@($caster,$second))){throw 'Manual target scope leaked'}
 $inner=[Activator]::CreateInstance($scopeType,[object[]]@($caster,$second))
 try{if(!$explicit.Invoke($null,[object[]]@($caster,$second))){throw 'Nested scope failed'}}finally{$inner.Dispose()}
 if(!$explicit.Invoke($null,[object[]]@($caster,$first))){throw 'Previous manual target not restored'}
} finally {$scope.Dispose()}
if($explicit.Invoke($null,[object[]]@($caster,$first))){throw 'Manual target survives disposal'}
$stylePolicy=$own.GetType('ElementalHearts.ElementalStylePolicy').GetMethod('Reserved')
foreach($name in @('EH_LightningHair','EH_LightningHairShort','EH_FlameHair')){
 $hair=[Activator]::CreateInstance($game.GetType('RimWorld.HairDef'));$hair.defName=$name
 if(!$stylePolicy.Invoke($null,[object[]]@($hair))){throw "Hair not reserved: $name"}
}
$regular=[Activator]::CreateInstance($game.GetType('RimWorld.HairDef'));$regular.defName='Mohawk'
if($stylePolicy.Invoke($null,[object[]]@($regular))){throw 'Unrelated hair affected'}
# Check Harmony's target methods and positional arguments against this local game build.
$chooser=$game.GetType('RimWorld.PawnStyleItemChooser')
foreach($entry in @(@('WantsToUseStyle',1),@('TotalStyleItemLikelihood',0))){
 $method=$chooser.GetMethod($entry[0],[Reflection.BindingFlags]'Public,NonPublic,Static')
 if(!$method -or $method.GetParameters()[$entry[1]].ParameterType.FullName -ne 'RimWorld.StyleItemDef'){throw "Hair patch target changed: $($entry[0])"}
}
$referenceStats=$null;$referenceCoverage=$null
foreach($e in @('Lightning','Flame')){
 [xml]$g=Get-Content "$root/Defs/${e}Gear.xml";$u=$g.SelectSingleNode("Defs/ThingDef[defName='EH_${e}Uniform']")
 if($u.statBases.MaxHitPoints -ne '1200' -or $u.generateCommonality -ne '0' -or $u.SelectNodes('apparel/tags/li').Count -ne 0){throw 'Uniform durability/generation mismatch'}
 if($referenceStats -and $u.statBases.OuterXml -ne $referenceStats){throw 'Uniform stats differ'}
 $referenceStats=$u.statBases.OuterXml
 if($referenceCoverage -and $u.apparel.bodyPartGroups.OuterXml -ne $referenceCoverage){throw 'Uniform coverage differs'}
 $referenceCoverage=$u.apparel.bodyPartGroups.OuterXml
 foreach($h in $g.SelectNodes('Defs/HairDef')){if($h.SelectNodes('styleTags/li').Count -ne 0){throw 'Random hair tags remain'}}
 foreach($d in @('south','east','north')){
  if(!(Test-Path "$root/Textures/Things/Pawn/Humanlike/Hair/${e}Hair_$d.png")){throw 'Missing hair texture'}
 }
}
foreach($icon in @('ElementalHair','AutoCombat')){if(!(Test-Path "$root/Textures/UI/Commands/ElementalHearts/$icon.png")){throw "Missing shared icon: $icon"}}
Write-Output 'PASS: manual target policy, protected bystanders, nested scope cleanup, forced NPC automation, player toggles, style exclusions, patch signatures, matching 1200-HP uniforms, and asset paths.'
Write-Output 'LIMIT: no live-game rendering/combat test. Verify AI retreat, delayed ultimates, generation and save/reload in RimWorld.'

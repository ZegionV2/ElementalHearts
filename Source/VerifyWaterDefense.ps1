$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Get-ChildItem C:/Games/RimWorld/RimWorldWin64_Data/Managed -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
[void][Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/Mods/2009463077/1.6/Assemblies/0Harmony.dll')
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
$rules=$own.GetType('ElementalHearts.WaterDefenseRules')
foreach($active in @($false,$true)){foreach($awake in @($false,$true)){foreach($armed in @($false,$true)){foreach($busy in @($false,$true)){foreach($front in @($false,$true)){foreach($now in @(99,100,101)){
 $expected=$active -and $awake -and $armed -and !$busy -and $front -and $now -ge 100
 $actual=$rules.GetMethod('Ready').Invoke($null,[object[]]@($active,$awake,$armed,$busy,$front,$now,100))
 if($actual -ne $expected){throw 'Parry eligibility regression'}
}}}}}}
foreach($case in @(@(20,-20,$true,$true),@(170,-170,$true,$false),@(20,10,$true,$false),@(20,-20,$false,$false),@(0,4,$false,$true))){if($rules.GetMethod('Crossed').Invoke($null,[object[]]@([single]$case[0],[single]$case[1],[bool]$case[2])) -ne $case[3]){throw 'Impact crossing regression'}}
foreach($case in @(@(0.5,$true),@(0.49,$false),@(-1,$false),@(1,$true))){if($rules.GetMethod('Facing').Invoke($null,[object[]]@([single]$case[0])) -ne $case[1]){throw 'Frontal cone regression'}}
[xml]$defs=Get-Content "$root/Defs/WaterHeart.xml"
if($defs.SelectSingleNode("Defs/HediffDef[defName='EH_WaterHeart']").ParentName -ne 'EH_ElementalHeartHediffBase'){throw 'Shared Heart contract missing'}
if(!$own.GetType($defs.Defs.JobDef.driverClass)){throw 'Counter job missing'}
foreach($e in @('Lightning','Flame','Wind')){
 $source=Get-Content "$root/Source/${e}Abilities.cs" -Raw
 if(!$source.Contains('WaterDefense.TryParry(') -or !$source.Contains('ElementalAnimationBridge.Controlled(Pawn)')){throw "Missing interception/animation guard: $e"}
}
# Verify the optional adapter against the actual installed assembly, without requiring it at build time.
Get-ChildItem C:/Games/RimWorld/Mods/2944488802/1.6/Assemblies -Filter *.dll | ForEach-Object {[void][Reflection.Assembly]::LoadFrom($_.FullName)}
$am=[Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/Mods/2944488802/1.6/Assemblies/zAnimationMod.dll')
$renderer=$am.GetType('AM.AnimRenderer');$idle=$am.GetType('AM.Idle.IdleControllerComp')
foreach($m in @('TryGetAnimator','Seek')){if(!$renderer.GetMethod($m)){throw "Missing renderer API: $m"}}
foreach($m in @('OnMeleeDodge','NotifyPawnDidMeleeAttack','ClearAnimation')){if(!$idle.GetMethod($m)){throw "Missing idle API: $m"}}
foreach($p in @('IsDestroyed','Duration')){if(!$renderer.GetProperty($p)){throw "Missing animation property: $p"}}
foreach($f in @('RootTransform','TimeScale')){if(!$renderer.GetField($f)){throw "Missing animation field: $f"}}
$snapshot=$renderer.GetField('snapshots',[Reflection.BindingFlags]'Instance,NonPublic').FieldType.GetElementType()
if(!$snapshot.GetField('WorldMatrixPreserveFlip') -or !$snapshot.GetField('Part').FieldType.GetProperty('Name')){throw 'Weapon snapshot schema changed'}
$bridge=Get-Content "$root/Source/ElementalAnimationBridge.cs" -Raw
if(!$bridge.Contains('timeScale.SetValue(a,0f)') -or !$bridge.Contains('seek.Invoke')){throw 'Counter depends on camera/frame timing'}
Write-Output 'PASS: 96 eligibility/cooldown cases, frontal cone boundaries, blade crossing and wraparound, shared Heart/job definitions, three Heart interceptors, and installed optional animation API/snapshot schema.'
Write-Output 'LIMIT: full combat, paired animations, rendering and save/reload require live-game testing.'

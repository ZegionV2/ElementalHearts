param([string]$RimWorldDir='C:/Games/RimWorld')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$managed=Join-Path $RimWorldDir 'RimWorldWin64_Data/Managed'
Get-ChildItem $managed -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
[void][Reflection.Assembly]::LoadFrom("$RimWorldDir/Mods/2009463077/1.6/Assemblies/0Harmony.dll")
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
if($own.GetReferencedAssemblies().Name -contains 'zAnimationMod'){throw 'Melee Animation became a required assembly dependency'}
$expected=@{EH_LightningHeart=5;EH_FlameHeart=5;EH_WindHeart=5;EH_WaterHeart=5;EH_AzureFlameAscendant=10;EH_ThunderstormAscendant=10;EH_GlacialAscendant=10;EH_CycloneAscendant=10}
$seen=0
Get-ChildItem "$root/Defs" -Filter *.xml | ForEach-Object {
 $xml=[xml](Get-Content $_.FullName -Raw)
 foreach($h in $xml.Defs.HediffDef){if($expected.ContainsKey([string]$h.defName)){$values=@($h.stages.li.statFactors.CarryingCapacity);if($values.Count -ne 1 -or [float]$values[0] -ne $expected[[string]$h.defName]){throw "Wrong carrying factor: $($h.defName)"};$seen++}}
}
if($seen -ne 8){throw 'Missing shipped Ascendant lineage'}
$target=[RimWorld.Planet.CaravanTicksPerMoveUtility].GetMethod('GetTicksPerMove',[type[]]@([Collections.Generic.List[Verse.Pawn]],[float],[float],[bool],[Text.StringBuilder]))
if(!$target){throw 'Caravan preview/world shared overload changed'}
$capacity=[RimWorld.MassUtility].GetMethod('Capacity',[type[]]@([Verse.Pawn],[Text.StringBuilder]))
if(!$capacity){throw 'Native caravan capacity API changed'}
$gizmo=$own.GetType('ElementalHearts.DevEvolutionReadyCommand').GetMethod('GizmoOnGUIInt',[Reflection.BindingFlags]'NonPublic,Instance,DeclaredOnly')
if(!$gizmo -or !$gizmo.IsVirtual){throw 'Cached command drawing guard missing'}
Write-Output 'PASS: eight native carrying factors, shared preview/world caravan overload, capacity API, optional Melee Animation dependency and cached-command drawing guard.'
Write-Output 'Native disease, Melee Animation outcomes, new-grant defaults and real saving/loading are checked by the separate BenefitsQA in-game fixture.'

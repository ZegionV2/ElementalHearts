$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Get-ChildItem C:/Games/RimWorld/RimWorldWin64_Data/Managed -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
$a=[Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll')
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
$p=$own.GetType('ElementalHearts.ElementalEquipmentLock')
foreach($active in @($false,$true)){foreach($internal in @($false,$true)){foreach($dead in @($false,$true)){
 $actual=$p.GetMethod('Policy').Invoke($null,@($active,$internal,$dead));$expected=$active -and !$internal -and !$dead
 if($actual -ne $expected){throw 'Equipment lock lifecycle mismatch'}
}}}
foreach($name in @('Wear','Equip','RemoveApparel','DropEquipment','EH_Transform','AttackMelee','Goto','Wait_Combat','Ingest','EH_TeachElemental')){
 $expected=@('Wear','Equip','RemoveApparel','DropEquipment') -contains $name
 if($p.GetMethod('ChangeJob').Invoke($null,@([string]$name)) -ne $expected){throw "Unexpected job gate: $name"}
}
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
foreach($pair in @(@('RimWorld.Pawn_ApparelTracker','IsLocked'),@('RimWorld.Pawn_ApparelTracker','WouldReplaceLockedApparel'),@('Verse.Pawn_EquipmentTracker','TryDropEquipment'),@('Verse.Pawn_EquipmentTracker','TryTransferEquipmentToContainer'),@('RimWorld.JobGiver_OptimizeApparel','TryGiveJob'),@('Verse.AI.Pawn_JobTracker','StartJob'),@('Verse.AI.Pawn_JobTracker','TryTakeOrderedJob'))){if(!$a.GetType($pair[0]).GetMethod($pair[1],$flags)){throw "Missing lock target $pair"}}
$tracker=$a.GetType('RimWorld.Pawn_ApparelTracker');$apparel=$a.GetType('RimWorld.Apparel');$cell=$a.GetType('Verse.IntVec3')
$drop=$tracker.GetMethod('TryDrop',[type[]]@($apparel,$apparel.MakeByRefType(),$cell,[bool]));if(!$drop){throw 'Drop overload missing'}
if($drop.GetParameters()[1].Name -ne 'resultingAp'){throw 'Apparel output argument mismatch'}
$eq=$a.GetType('Verse.Pawn_EquipmentTracker').GetMethod('TryDropEquipment');if($eq.GetParameters()[1].Name -ne 'resultingEq'){throw 'Equipment output argument mismatch'}
Write-Output 'PASS: 8 lock lifecycle cases, 10 gear/non-gear job cases, native lock/drop/transfer/outfit targets and output parameter bindings.'
Write-Output 'LIMIT: live gear-tab actions, incidents and third-party equipment mods require in-game testing.'

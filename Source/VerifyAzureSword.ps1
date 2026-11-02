$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Get-ChildItem C:/Games/RimWorld/RimWorldWin64_Data/Managed -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
[void][Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/Mods/2009463077/1.6/Assemblies/0Harmony.dll')
Get-ChildItem C:/Games/RimWorld/Mods/2944488802/1.6/Assemblies -Filter *.dll | ForEach-Object {[void][Reflection.Assembly]::LoadFrom($_.FullName)}
$assembly=[Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/Mods/2944488802/1.6/Assemblies/zAnimationMod.dll')
$type=$assembly.GetType('AM.Tweaks.ItemTweakData',$true)
Add-Type -AssemblyName System.Drawing
foreach($e in @('AzureFlame')){
 $path="$root/WeaponTweakData/EH_${e}Sword_elementalhearts.lightningheart.json"
 $json=Get-Content $path -Raw | ConvertFrom-Json
 foreach($field in $json.PSObject.Properties.Name){if(!$type.GetField($field)){throw "Unknown Melee Animation field: $field"}}
 $data=$type.GetMethod('LoadFrom').Invoke($null,@($path))
 if($data.ItemDefName -ne "EH_${e}Sword" -or $data.TextureModID -ne 'elementalhearts.lightningheart' -or [int]$data.MeleeWeaponType -ne 6 -or [int]$data.HandsMode -ne 0){throw 'Identity or animation classification mismatch'}
 if($data.Rotation -ne 90 -or $data.ScaleX -ne $data.ScaleY -or $data.BladeStart -le 0 -or $data.BladeEnd -le $data.BladeStart){throw 'Invalid blade transform'}
 # For a +90 degree Y rotation, an upward texture points right.
 $gripX=$data.OffX+(128-195)*$data.ScaleY/256
 if([Math]::Abs($gripX) -gt 0.00001){throw 'Primary hand misses grip anchor'}
 $secondGripY=195+0.146*256/$data.ScaleY
 $b=[Drawing.Bitmap]::new("$root/Textures/Things/Item/Equipment/WeaponMelee/${e}Sword.png")
 try{foreach($y in @(195,[int]$secondGripY)){if($b.GetPixel(128,$y).A -lt 200){throw 'Hand anchor outside handle artwork'}}}finally{$b.Dispose()}
 [xml]$defs=Get-Content "$root/Defs/AzureFlame.xml"
 if(!$defs.SelectSingleNode("Defs/ThingDef[defName='$($data.ItemDefName)']")){throw 'Weapon definition missing'}
}
if(Test-Path "$root/WeaponTweakData/EH_WaterSword_elementalhearts.lightningheart.json"){
 [xml]$waterGear=Get-Content -LiteralPath "$root/Defs/WaterGear.xml"
 if(!$waterGear.SelectSingleNode("Defs/ThingDef[defName='EH_WaterSword']")){throw 'Active Water profile has no Water sword definition'}
}
Write-Output 'PASS: Azure sword profile deserialize using installed Melee Animation; identifiers, fields, blade orientation, scale, two hand anchors and sharp/stab animation flags validated. Active Water support is checked separately.'
Write-Output 'LIMIT: animated grip and movement still require an in-game visual check.'

param([string]$RimWorldDir='C:/Games/RimWorld')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Get-ChildItem "$RimWorldDir/RimWorldWin64_Data/Managed" -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
[void][Reflection.Assembly]::LoadFrom("$RimWorldDir/Mods/2009463077/1.6/Assemblies/0Harmony.dll")
Get-ChildItem "$RimWorldDir/Mods/2944488802/1.6/Assemblies" -Filter *.dll | ForEach-Object {[void][Reflection.Assembly]::LoadFrom($_.FullName)}
$assembly=[Reflection.Assembly]::LoadFrom("$RimWorldDir/Mods/2944488802/1.6/Assemblies/zAnimationMod.dll")
$type=$assembly.GetType('AM.Tweaks.ItemTweakData',$true)
Add-Type -AssemblyName System.Drawing
$definitions=@{}
foreach($path in Get-ChildItem "$root/Defs" -Filter *.xml){[xml]$defs=Get-Content -LiteralPath $path.FullName;foreach($def in $defs.SelectNodes("Defs/ThingDef[starts-with(defName,'EH_')]")){if($def.defName -like '*Sword'){$definitions[$def.defName]=$def}}}
if($definitions.Count -ne 11){throw 'Expected all eleven elemental sword definitions'}
foreach($element in @('Lightning','Flame','Wind','Water','AzureFlame','Thunderstorm','Glacial','Cyclone','Celestial','Earth','Adamant')){
 $name="EH_${element}Sword";$path="$root/WeaponTweakData/${name}_elementalhearts.lightningheart.json"
 $json=Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
 foreach($field in $json.PSObject.Properties.Name){if(!$type.GetField($field)){throw "Unknown installed Melee Animation field: $field"}}
 $data=$type.GetMethod('LoadFrom').Invoke($null,@($path))
 if($data.ItemDefName -ne $name -or $data.TextureModID -ne 'elementalhearts.lightningheart' -or $data.ItemType -ne 'ThingDef' -or $data.ItemTypeNamespace -ne 'Verse' -or [int]$data.MeleeWeaponType -ne 6 -or [int]$data.HandsMode -ne 0){throw "Invalid animation identity/classification: $name"}
 if(!$definitions.ContainsKey($name) -or $data.Rotation -ne 90 -or $data.ScaleX -ne $data.ScaleY -or $data.ScaleY -le 0 -or $data.BladeStart -le 0 -or $data.BladeEnd -le $data.BladeStart -or $data.FlipX -or $data.FlipY){throw "Invalid sword orientation or blade bounds: $name"}
 $gripY=if($element -eq 'Adamant'){206}elseif($element -eq 'Celestial'){190}elseif($element -eq 'Glacial'){182}elseif($element -eq 'Cyclone'){188}else{195}
 if([Math]::Abs($data.OffX+(128-$gripY)*$data.ScaleY/256) -gt 0.00001 -or $data.OffY -ne 0){throw "Primary grip misses artwork anchor: $name"}
 $secondY=$gripY+0.146*256/$data.ScaleY
 $texture="$root/Textures/$($definitions[$name].graphicData.texPath).png"
 $bitmap=[Drawing.Bitmap]::new($texture)
 try{
  if($bitmap.Width -ne 256 -or $bitmap.Height -ne 256){throw 'Unexpected sword canvas size'}
  foreach($y in @($gripY,[int][Math]::Round($secondY))){if($y -lt 0 -or $y -ge 256 -or $bitmap.GetPixel(128,$y).A -lt 200){throw "Hand anchor falls off handle: $name at $y"}}
  if($element -eq 'Glacial'){
   if($gripY -lt 180 -or $secondY -gt 209){throw 'Glacial hands do not fit its shorter ivory grip'}
   foreach($distance in @($data.BladeStart,$data.BladeEnd)){$y=[int][Math]::Round($gripY-$distance*256/$data.ScaleY);if($y -lt 0 -or $bitmap.GetPixel(128,$y).A -lt 200){throw 'Glacial sweep bound misses opaque blade artwork'}}
  }
  if($element -eq 'Cyclone'){
   if($gripY -lt 168 -or $secondY -gt 223){throw 'Cyclone hands do not fit the ivory grip'}
   foreach($distance in @($data.BladeStart,$data.BladeEnd)){$y=[int][Math]::Round($gripY-$distance*256/$data.ScaleY);$opaque=$false;foreach($x in 100..151){if($bitmap.GetPixel($x,$y).A -ge 200){$opaque=$true;break}};if(!$opaque){throw 'Cyclone sweep bound misses its curved blade artwork'}}
  }
 }finally{$bitmap.Dispose()}
 Write-Output "PASS: $element native profile, long sharp/stab flags, both-hand mode, grip alignment and blade bounds."
}
Write-Output 'PASS: all eleven elemental swords deserialize with the installed Melee Animation loader. Native registry/animation selection is checked separately in-game.'

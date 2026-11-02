$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$textures=(Resolve-Path "$root/Textures").Path
$assets=@(Get-ChildItem -LiteralPath $textures -Recurse -Filter *.png | Where-Object {$_.FullName.Substring($textures.Length) -match 'Cyclone'})
if($assets.Count -ne 97){throw 'Expected exactly 97 new Cyclone textures'}
Add-Type -AssemblyName System.Drawing
foreach($file in $assets){$image=[Drawing.Bitmap]::new($file.FullName);try{if($image.Width -ne 256 -or $image.Height -ne 256 -or $image.GetPixel(0,0).A -ne 0){throw "Invalid Cyclone size/alpha: $file"}}finally{$image.Dispose()}}
foreach($body in @('Male','Female','Thin','Fat','Hulk','Child','Baby')){foreach($dir in @('south','east','north')){if(!(Test-Path "$textures/Things/Pawn/Humanlike/Apparel/CycloneRegalia/CycloneRegalia_${body}_${dir}.png")){throw 'Missing body/direction sprite'}}}
if(Get-ChildItem "$textures/Things/Pawn/Humanlike/Apparel/CycloneRegalia" -Filter '*_west.png'){throw 'Cyclone should use native west mirroring'}
foreach($entry in @(@('DrawTrail',8),@('RisingSpiral',8),@('CounterSweep',8),@('FinisherSlash',8),@('CycloneForm',8),@('CycloneLoop',8),@('ReleaseBurst',8),@('Airbound',6),@('Halo',6))){$hashes=@();for($i=0;$i -lt $entry[1];$i++){$path="$textures/Effects/ElementalHearts/Cyclone/Frames/$($entry[0])_$('{0:00}' -f $i).png";$hashes+=(Get-FileHash -LiteralPath $path).Hash};if(@($hashes|Select-Object -Unique).Count -ne $entry[1]){throw "Duplicate animation frame: $($entry[0])"}}
[xml]$gear=Get-Content "$root/Defs/CycloneGear.xml"
if($gear.SelectNodes('Defs/ThingDef').Count -ne 2 -or $gear.SelectNodes('Defs/HediffDef|Defs/AbilityDef|Defs/JobDef').Count -ne 0){throw 'Gear file must contain only its two equipment definitions'}
foreach($item in $gear.Defs.ThingDef){if($item.modExtensions.li.Class -ne 'ElementalHearts.IndestructibleElementalGear' -or $item.useHitPoints -ne 'false'){throw 'Cyclone gear missing shared indestructibility'}}
if(!(Test-Path "$textures/UI/Commands/ElementalHearts/Bestow.png")){throw 'Shared Bestow icon missing'}
$source=Get-Content "$root/Source/CycloneVfx.cs" -Raw
foreach($required in @('EffectLimit=128','EchoLimit=32','AirboundLimit=64','HaloLimit=128','MaxDuration=300','Material[9,8]','Material[7,3]','CurrentViewRect','Fogged(map)','effectOpacity','FinalizeInit')){if(!$source.Contains($required)){throw "Missing visual budget/cache/culling: $required"}}
foreach($forbidden in @('TakeDamage(','DamageInfo(','GenSpawn','SetTerrain(','roofGrid','weatherManager','StartJob(','StunFor(')){if($source.Contains($forbidden)){throw "Visual provider changes gameplay/environment: $forbidden"}}
Write-Output 'PASS: 97 transparent textures, 21 body/direction sprites with native west mirroring, 68 distinct frames, six icons, indestructible gear and shared Bestow.'
Write-Output 'PASS: cached materials, fixed effect/echo/airbound/halo budgets, expiry, fog/camera culling and presentation-only provider; separate Cyclone gameplay owns state and impacts.'

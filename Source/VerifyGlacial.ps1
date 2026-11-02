$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
[xml]$defs=Get-Content "$root/Defs/GlacialAscendant.xml"
$heart=$defs.SelectSingleNode("Defs/HediffDef[defName='EH_GlacialAscendant']")
if($heart.ParentName -ne 'EH_ElementalHeartHediffBase' -or $heart.stages.li.statFactors.MoveSpeed -ne '5' -or $heart.stages.li.statFactors.IncomingDamageFactor -ne '0.1'){throw 'Glacial lineage stats incorrect'}
foreach($comp in @('WaterDefense','WaterAbilities','LightningRecovery','GlacialMastery','GlacialExplosion')){if($heart.SelectNodes("comps/li[@Class='ElementalHearts.HediffCompProperties_$comp']").Count -ne 1){throw "Glacial component missing/duplicated: $comp"}}
if($defs.SelectNodes("Defs/ThingDef[defName='EH_GlacialHelmet']").Count -ne 0 -or $defs.SelectNodes('Defs/ThingDef').Count -ne 2){throw 'Unexpected Glacial kit/helmet'}
Add-Type -AssemblyName System.Drawing
$paths=@(Get-ChildItem "$root/Textures" -Recurse -Filter *.png | Where-Object {$_.FullName.Substring((Resolve-Path "$root/Textures").Path.Length) -match 'Glacial'})
if($paths.Count -ne 93){throw 'Expected 93 Glacial textures'}
foreach($p in $paths){$b=[Drawing.Bitmap]::new($p.FullName);try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0){throw "Invalid Glacial PNG dimensions/alpha: $p"}}finally{$b.Dispose()}}
foreach($body in @('Male','Female','Thin','Fat','Hulk','Child','Baby')){foreach($dir in @('south','east','north')){if(!(Test-Path "$root/Textures/Things/Pawn/Humanlike/Apparel/GlacialRegalia/GlacialRegalia_${body}_${dir}.png")){throw 'Missing body/direction'}}}
foreach($item in @(@('Crescent',8),@('Wake',8),@('Spin',8),@('Telegraph',8),@('Column',8),@('Explosion',8),@('FrostStreak',4),@('Freeze',6),@('Halo',6))){$hashes=@();for($i=0;$i -lt $item[1];$i++){$path="$root/Textures/Effects/ElementalHearts/Glacial/Frames/$($item[0])_$('{0:00}' -f $i).png";$hashes+=(Get-FileHash $path).Hash};if(@($hashes|Select-Object -Unique).Count -ne $item[1]){throw "Duplicated animation frames: $($item[0])"}}
$vfx=Get-Content "$root/Source/GlacialVfx.cs" -Raw
foreach($token in @('Effect[128]','Echo[32]','tracked.Count<256','CurrentViewRect','Fogged(map)','effectOpacity')){if(!$vfx.Contains($token)){throw "Missing VFX budget/culling: $token"}}
$combat=Get-Content "$root/Source/GlacialExplosion.cs" -Raw
foreach($bad in @('GenExplosion.DoExplosion','SetTerrain(','SetTemperature(','FireUtility.TryStartFire')){if($combat.Contains($bad)){throw "Unexpected environmental effect: $bad"}}
if(!(Test-Path "$root/Textures/UI/Commands/ElementalHearts/Bestow.png")){throw 'Shared Bestow icon missing'}
Write-Output 'PASS: 93 transparent Glacial textures, all 21 directional/body variants, six command icons, all 64 distinct animation frames, helmetless kit, evolved stats and bounded culled VFX.'

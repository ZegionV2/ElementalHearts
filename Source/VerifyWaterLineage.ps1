$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
[xml]$teaching=Get-Content "$root/Defs/ElementalTeaching.xml"
$training=$teaching.SelectSingleNode("Defs/HediffDef[defName='EH_WaterTraining']")
if(!$training -or $training.comps.li.Count -ne 3 -or $training.stages -or $training.tags){throw 'Water disciple gets unwanted ascension powers'}
foreach($class in @('WaterDefense','WaterAbilities','ElementalApprentice')){if($training.SelectNodes("comps/li[@Class='ElementalHearts.HediffCompProperties_$class']").Count -ne 1){throw "Water disciple component missing/duplicated: $class"}}
if($training.SelectSingleNode("comps/li[@Class='ElementalHearts.HediffCompProperties_WaterAbilities']/cooldownFactor").InnerText -ne '1.5'){throw 'Water training cooldown incorrect'}
foreach($job in $teaching.Defs.JobDef){if($job.playerInterruptible -ne 'true' -or $job.suspendable -ne 'false'){throw 'Paired lesson interruption cleanup policy incorrect'}}
Add-Type -AssemblyName System.Drawing
$hashes=@();foreach($i in 0..5){$path="$root/Textures/Effects/ElementalHearts/Water/Frames/Aura_$('{0:00}' -f $i).png";$hashes+=(Get-FileHash $path).Hash;$b=[Drawing.Bitmap]::new($path);try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0){throw 'Aura sprite dimensions/transparency incorrect'}}finally{$b.Dispose()}}
if(($hashes | Sort-Object -Unique).Count -ne 6){throw 'Aura has duplicated frames'}
[xml]$heart=Get-Content "$root/Defs/WaterHeart.xml";[xml]$about=Get-Content "$root/About/About.xml"
if($heart.Defs.HediffDef.description -match 'teaching is planned|teaching is in development' -or $about.ModMetaData.description -match 'Water teaching is in development'){throw 'Shipped Water teaching described as future work'}
if($about.ModMetaData.description -notmatch 'Glacial.*Explosion'){throw 'Playable Glacial evolution not described'}
$projections=0;foreach($path in Get-ChildItem "$root/Defs" -Filter *.xml){[xml]$defs=Get-Content $path.FullName;foreach($thing in $defs.SelectNodes("Defs/ThingDef[modExtensions/li[@Class='ElementalHearts.IndestructibleElementalGear']]")){$projections++;if($thing.statBases.Flammability -ne '0' -or $thing.statBases.DeteriorationRate -ne '0' -or $thing.smeltable -ne 'false'){throw "Invalid indestructible projection fire/deterioration/smelting policy: $($thing.defName)"}}}
if($projections -ne 27){throw 'Unexpected indestructible gear count (includes Earth stoneplate and Faultbreaker)'}
Write-Output 'PASS: Water disciple component bindings, no ascension stage/tags, paired job interruption flags, six distinct transparent aura sprites and shipped Glacial descriptions.'
Write-Output 'PASS: all 27 indestructible projections explicitly disable burning, deterioration and smelting.'

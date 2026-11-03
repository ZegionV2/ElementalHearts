param([string]$RimWorldDir='C:/Games/RimWorld')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$managed="$RimWorldDir/RimWorldWin64_Data/Managed"
Get-ChildItem $managed -Filter *.dll|ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
[void][Reflection.Assembly]::LoadFrom("$RimWorldDir/Mods/2009463077/1.6/Assemblies/0Harmony.dll")
$game=[Reflection.Assembly]::LoadFrom("$managed/Assembly-CSharp.dll")
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
$rhythm=$own.GetType('ElementalHearts.WindRhythm')
foreach($c in @(@(-1,0,0,$true,0),@(0,1,0,$false,1),@(1,2,1,$false,2),@(2,3,2,$false,3),@(3,0,3,$false,3),@(0,0,3,$false,0),@(1,2,3,$true,0))){
 if($rhythm.GetMethod('NextStacks').Invoke($null,[object[]]$c[0..3]) -ne $c[4]){throw 'Flow sequence regression'}
}
foreach($c in @(@(300,0,300),@(300,3,228),@(300,-1,300),@(300,100,228))){if($rhythm.GetMethod('Cooldown').Invoke($null,[object[]]$c[0..1]) -ne $c[2]){throw 'Flow cooldown regression'}}
[xml]$defs=Get-Content "$root/Defs/WindHeart.xml"
$heart=$defs.SelectSingleNode("Defs/HediffDef[defName='EH_WindHeart']")
if($heart.ParentName -ne 'EH_ElementalHeartHediffBase' -or $heart.spawnThingOnRemoved){throw 'Shared ascension contract mismatch'}
foreach($comp in $heart.comps.li){
 $type=$own.GetType($comp.Class);if(!$type){throw "Missing comp: $($comp.Class)"}
 foreach($field in $comp.ChildNodes){if($field.NodeType -eq 'Element' -and !$type.GetField($field.Name)){throw "Unknown tuning field $($field.Name)"}}
}
foreach($def in $defs.SelectNodes('Defs/HediffDef')){
 foreach($cap in $def.SelectNodes('stages/li/capMods/li')){if([double]$cap.offset -lt 0){throw 'Unexpected negative capacity'}}
 foreach($comp in $def.SelectNodes('comps/li')){if($comp.Class -eq 'HediffCompProperties_Disappears'){
  $type=$game.GetType('Verse.HediffCompProperties_Disappears');foreach($field in $comp.ChildNodes){if(!$type.GetField($field.Name)){throw "Unknown expiry field $($field.Name)"}}
 }}
}
$job=$defs.SelectSingleNode("Defs/JobDef[defName='EH_WindTechnique']")
if(!$own.GetType($job.driverClass)){throw 'Job driver missing'}
$policy=$own.GetType('ElementalHearts.ElementalStylePolicy').GetMethod('Reserved')
$hair=[Activator]::CreateInstance($game.GetType('RimWorld.HairDef'));$hair.defName='EH_WindHair'
if(!$policy.Invoke($null,[object[]]@($hair))){throw 'Wind hair can generate randomly'}
[xml]$gear=Get-Content "$root/Defs/WindGear.xml";[xml]$flame=Get-Content "$root/Defs/FlameGear.xml"
$uniform=$gear.SelectSingleNode("Defs/ThingDef[defName='EH_WindUniform']");$reference=$flame.SelectSingleNode("Defs/ThingDef[defName='EH_FlameUniform']")
if($uniform.statBases.OuterXml -ne $reference.statBases.OuterXml -or $uniform.apparel.bodyPartGroups.OuterXml -ne $reference.apparel.bodyPartGroups.OuterXml){throw 'Apparel settings differ'}
if($uniform.generateCommonality -ne '0' -or $uniform.apparel.tags.li){throw 'Uniform can generate randomly'}
$names=@('DustCuttingDraw','RisingGale','TurningSwallow','MountainScouringTempest','Idle','Combat','Activation')
foreach($name in $names){
 $files=@(Get-ChildItem "$root/Textures/Effects/ElementalHearts/Wind/Frames/${name}_*.png")
 $frameCount=if($name -in @('Idle','Combat','Activation')){6}else{8}
 $expectedNames=@(0..($frameCount-1) | ForEach-Object { '{0}_{1:00}.png' -f $name,$_ })
 if($files.Count -ne $frameCount -or (Compare-Object $expectedNames @($files.Name)) -or @($files|Get-FileHash|Select-Object -ExpandProperty Hash -Unique).Count -ne $frameCount){throw "Animation incomplete: $name"}
}
Add-Type -AssemblyName System.Drawing
$textures=@(Get-ChildItem "$root/Textures" -Recurse -Filter '*.png'|Where-Object {$_.FullName -match '[\\/]Wind[\\/]|[\\/]Wind[^\\/]*\.png$|[\\/]WindUniform[\\/]'})
if($textures.Count -ne 90){throw "Expected 90 textures, got $($textures.Count)"}
foreach($file in $textures){$b=[Drawing.Bitmap]::new($file.FullName);try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0){throw "Texture format mismatch: $file"}}finally{$b.Dispose()}}
$nonlethal=Get-Content "$PSScriptRoot/ElementalNonLethal.cs" -Raw
if(!$nonlethal.Contains('HediffComp_WindAbilities.On(source)')){throw 'Wind omitted from non-lethal policy'}
$manual=Get-Content "$PSScriptRoot/ElementalPresentation.cs" -Raw
if(!$manual.Contains('job.def.defName=="EH_WindTechnique"')){throw 'Manual targeting lost during delayed strikes'}
$driver=Get-Content "$PSScriptRoot/WindTechnique.cs" -Raw
foreach($key in @('windRoute','windRouteIndex','windNextStep','windStruck')){if(!$driver.Contains('"'+$key+'"')){throw "Missing saved movement state: $key"}}
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$method=$game.GetType('RimWorld.Verb_MeleeAttackDamage').GetMethod('ApplyMeleeDamageToTarget',$flags)
if($method.GetParameters()[0].ParameterType.FullName -ne 'Verse.LocalTargetInfo'){throw 'Counter patch API mismatch'}
Write-Output 'PASS: Flow chaining, repeats, expiry, cooldown bounds, implant comps, recipes/job types, hair exclusion, matching uniforms, 90 transparent textures, 50 unique frames, non-lethal integration, manual-target persistence, movement save fields, and counter API.'
Write-Output 'LIMIT: no live-game combat/rendering/save-reload test.'


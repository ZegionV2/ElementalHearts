$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Add-Type -AssemblyName System.Drawing
Get-ChildItem C:/Games/RimWorld/RimWorldWin64_Data/Managed -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
$game=[Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll')
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
[xml]$helmets=Get-Content "$root/Defs/ElementalHelmets.xml"
[xml]$visors=Get-Content "$root/Defs/ElementalVisors.xml"
foreach($h in $helmets.Defs.ThingDef){
 if($h.apparel.bodyPartGroups.li -ne 'FullHead' -or $h.apparel.layers.li -ne 'Overhead'){throw 'Helmet coverage mismatch'}
 if($h.apparel.renderSkipFlags.li -contains 'Head'){throw 'Helmet hides its own head apparel render branch'}
 foreach($flag in @('Hair','Beard')){if($h.apparel.renderSkipFlags.li -notcontains $flag){throw "Helmet exposes $flag"}}
 if($h.generateCommonality -ne '0' -or $h.apparel.canBeGeneratedToSatisfyWarmth -ne 'false'){throw 'Random helmet generation enabled'}
 if($h.statBases.MaxHitPoints -ne '1200' -or $h.equippedStatOffsets.MeleeHitChance -ne '5' -or $h.equippedStatOffsets.MeleeDodgeChance -ne '3'){throw 'Helmet bonuses mismatch'}
 $v=$visors.SelectSingleNode("Defs/ThingDef[defName='$($h.defName.Replace('Helmet','Visor'))']")
 if($v.equippedStatOffsets -or [float]$v.statBases.ArmorRating_Sharp -ne 0){throw 'Visor retains combat bonuses'}
 foreach($n in $h.apparel.ChildNodes){if($n.NodeType -eq 'Element' -and !$game.GetType('RimWorld.ApparelProperties').GetField($n.Name)){throw "Invalid helmet field $($n.Name)"}}
 foreach($suffix in @('','_south','_east','_north')){
  $b=[Drawing.Bitmap]::new("$root/Textures/$($h.graphicData.texPath)$suffix.png")
  try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0){throw 'Invalid helmet texture'}}finally{$b.Dispose()}
 }
}
$clips=@{}
Get-ChildItem C:/Games/RimWorld/Data/Core/Defs/SoundDefs -Filter *.xml | ForEach-Object {[xml]$d=Get-Content $_.FullName;foreach($n in $d.SelectNodes('//clipPath|//clipFolderPath')){$clips[$n.InnerText]=$true}}
[xml]$sounds=Get-Content "$root/Defs/ElementalSounds.xml"
foreach($s in $sounds.Defs.SoundDef){
 if($s.maxSimultaneous -ne '3'){throw 'Unbounded simultaneous audio'}
 foreach($n in $s.ChildNodes){if($n.NodeType -eq 'Element' -and !$game.GetType('Verse.SoundDef').GetField($n.Name)){throw "Invalid sound field $($n.Name)"}}
 foreach($n in $s.SelectNodes('.//clipPath|.//clipFolderPath')){if(!$clips.ContainsKey($n.InnerText)){$local=Join-Path "$root/Sounds" $n.InnerText;if(!(Test-Path -LiteralPath $local) -or @(Get-ChildItem -LiteralPath $local -Filter *.wav).Count -lt 1){throw 'Unknown sound asset'}}}
}
if(!$own.GetType('ElementalHearts.HediffComp_ElementalTransform').GetMethod('Transforming')){throw 'Transformation busy gate missing'}
$throttle=$own.GetType('ElementalHearts.ElementalAudio').GetNestedType('Throttle',[Reflection.BindingFlags]'NonPublic')
if(!$throttle.GetConstructor([Type[]]@())){throw 'Audio throttle cannot initialize'}
if(!(Test-Path "$root/Textures/UI/Commands/ElementalHearts/Transform.png")){throw 'Transform icon missing'}
[xml]$job=Get-Content "$root/Defs/ElementalTransform.xml"
$j=$job.Defs.JobDef
if($j.defName -ne 'EH_Transform' -or $j.playerInterruptible -ne 'false' -or $j.casualInterruptible -ne 'false'){throw 'Transformation is interruptible by ordinary orders'}
$driver=$own.GetType($j.driverClass)
if(!$driver -or !$game.GetType('Verse.AI.JobDriver').IsAssignableFrom($driver)){throw 'Invalid transformation job driver'}
foreach($n in $j.ChildNodes){if($n.NodeType -eq 'Element' -and !$game.GetType('Verse.JobDef').GetField($n.Name)){throw "Invalid transformation job field $($n.Name)"}}
$flags=[Reflection.BindingFlags]'NonPublic,Instance'
foreach($method in @('Begin','Cancel')){if(!$own.GetType('ElementalHearts.HediffComp_ElementalTransform').GetMethod($method,$flags)){throw "Missing job lifecycle method $method"}}
Write-Output 'PASS: dedicated transformation job binding, ordinary interruption locks, and begin/cancel lifecycle.'
Write-Output 'PASS: three full-face helmets, visible head render branch with hidden hair/beard, migrated combat bonuses, cosmetic visors, generation exclusions, directional transparent textures, sound schema and Core/custom clips, bounded audio and transform busy API.'
Write-Output 'LIMIT: saved-kit migration, timing, actual pawn fit and audio mix require live-game testing.'

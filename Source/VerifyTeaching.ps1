$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Get-ChildItem C:/Games/RimWorld/RimWorldWin64_Data/Managed -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
$game=[Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll')
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
[xml]$training=Get-Content "$root/Defs/ElementalTeaching.xml"
[xml]$visors=Get-Content "$root/Defs/ElementalVisors.xml"
foreach($job in $training.Defs.JobDef){if(!$own.GetType($job.driverClass)){throw 'Teaching/receiving job missing'}}
foreach($e in @('Lightning','Flame','Wind')){
 [xml]$heart=Get-Content "$root/Defs/${e}Heart.xml"
 $moods=@($heart.SelectNodes('//baseMoodEffect'));if($moods.Count -lt 1 -or $moods[0].InnerText -ne '30'){throw 'Ordinary ascendant mood regression'}
 if($e -in @('Flame','Lightning','Wind')){if($moods.Count -ne 2 -or $moods[1].InnerText -ne '50'){throw 'Evolved mood must replace ordinary mood with +50'}}elseif($moods.Count -ne 1){throw 'Unexpected extra mood stage'}
 $h=$training.SelectSingleNode("Defs/HediffDef[defName='EH_${e}Training']")
 if(!$h -or $h.comps.li.Count -ne 2 -or $h.tags -or $h.stages){throw 'Training grants unwanted implant effects'}
 foreach($comp in $h.comps.li){
  $type=$own.GetType($comp.Class);if(!$type){throw "Missing training component $($comp.Class)"}
  foreach($field in $comp.ChildNodes){if($field.NodeType -eq 'Element' -and !$type.GetField($field.Name)){throw "Invalid training field $($field.Name)"}}
 }
 $v=$visors.SelectSingleNode("Defs/ThingDef[defName='EH_${e}Visor']")
 if($v.statBases.MaxHitPoints -ne '1200' -or $v.apparel.layers.li -ne 'EyeCover' -or $v.apparel.renderSkipFlags.li -ne 'None' -or $v.generateCommonality -ne '0'){throw 'Visor configuration mismatch'}
 foreach($field in $v.apparel.ChildNodes){if($field.NodeType -eq 'Element' -and !$game.GetType('RimWorld.ApparelProperties').GetField($field.Name)){throw "Invalid apparel field $($field.Name)"}}
 foreach($suffix in @('','_south','_east','_north')){if(!(Test-Path "$root/Textures/$($v.graphicData.texPath)$suffix.png")){throw 'Visor texture missing'}}
}
if(!(Test-Path "$root/Textures/UI/Commands/ElementalHearts/Teach.png")){throw 'Teach icon missing'}
Write-Output 'PASS: teaching job and component bindings, cooldown fields, no implant bonuses on pupils, +30 ordinary / +50 evolved mood stages, 1200-HP visors, hair visibility, generation exclusions, and directional asset paths.'
Write-Output 'LIMIT: teaching interruption, saved pupils and visor alignment need live-game verification.'


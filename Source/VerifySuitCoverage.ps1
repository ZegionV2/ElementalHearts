$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Add-Type -AssemblyName System.Drawing
Get-ChildItem C:/Games/RimWorld/RimWorldWin64_Data/Managed -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
$game=[Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll')
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
if($own.GetType('ElementalHearts.FullSuitBodyCoverage')){throw 'Body-hiding patch is still compiled'}
$count=0
foreach($e in @('Lightning','Flame','Wind')){
 [xml]$xml=Get-Content "$root/Defs/${e}Gear.xml"
 $suit=$xml.SelectSingleNode("Defs/ThingDef[defName='EH_${e}Uniform']")
 if($suit.SelectSingleNode("modExtensions/li[@Class='ElementalHearts.ElementalFullBodySuit']")){throw "Obsolete coverage marker on $e"}
 if($suit.apparel.renderSkipFlags.li -contains 'Body'){throw 'Body branch must not be hidden'}
 foreach($body in @('Male','Female','Thin','Fat','Hulk','Child','Baby')){foreach($dir in @('south','east','north')){
  $path="$root/Textures/$($suit.apparel.wornGraphicPath)_${body}_$dir.png"
  $b=[Drawing.Bitmap]::new($path);try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0){throw "Bad body sprite: $path"}}finally{$b.Dispose()};$count++
 }}
}
Write-Output "PASS: body-hiding patch removed, normal suit rendering restored; $count transparent directional body variants."
Write-Output 'LIMIT: live fit with third-party body/rendering mods still needs in-game verification.'

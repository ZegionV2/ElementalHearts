$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Add-Type -AssemblyName System.Drawing
function Get-SpriteBounds($bitmap) {
 $left=$bitmap.Width; $top=$bitmap.Height; $right=-1; $bottom=-1
 for($y=0;$y -lt $bitmap.Height;$y++){for($x=0;$x -lt $bitmap.Width;$x++){if($bitmap.GetPixel($x,$y).A -gt 40){$left=[Math]::Min($left,$x);$top=[Math]::Min($top,$y);$right=[Math]::Max($right,$x);$bottom=[Math]::Max($bottom,$y)}}}
 if($right -lt 0){throw 'Empty sprite'}
 return [Drawing.Rectangle]::FromLTRB($left,$top,$right+1,$bottom+1)
}
$count=0
foreach($element in @('AzureFlame','Thunderstorm')){foreach($body in @('Male','Female','Thin','Fat','Hulk','Child','Baby')){foreach($dir in @('south','east','north')){
 $p="$root/Textures/Things/Pawn/Humanlike/Apparel/${element}Regalia/${element}Regalia_${body}_${dir}.png"
 $b=[Drawing.Bitmap]::new($p);$normal=[Drawing.Bitmap]::new("$root/Textures/Things/Pawn/Humanlike/Apparel/FlameUniform/FlameUniform_${body}_${dir}.png")
 try {
  if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0){throw "Invalid RGBA sprite: $p"}
  $bounds=Get-SpriteBounds $b;$prior=Get-SpriteBounds $normal
  if($bounds.Top -gt $prior.Top -or $bounds.Bottom -lt $prior.Bottom){throw "Regalia is lowered or too short: $p"}
  foreach($height in @(.4,.5,.6)){foreach($width in @(.46,.5,.54)){$x=[int]($bounds.Left+$bounds.Width*$width);$y=[int]($bounds.Top+$bounds.Height*$height);if($b.GetPixel($x,$y).A -lt 230){throw "Transparent torso lining: $p at $x,$y"}}}
  $count++
 } finally {$b.Dispose();$normal.Dispose()}
}}}
foreach($name in @('AzureFlame','Thunderstorm')){
 [xml]$defs=Get-Content -LiteralPath "$root/Defs/$name.xml"
 $offset=$defs.SelectSingleNode("Defs/ThingDef[defName='EH_${name}Uniform']/apparel/drawData/dataSouth/offset")
 if(!$offset -or $offset.InnerText -ne '(0, 0, 0)'){throw 'Centered regalia requires zero front offset'}
}
$icons=@("$root/Textures/UI/Commands/ElementalHearts/Bestow.png")+@(0..5 | ForEach-Object {"$root/Textures/UI/Commands/ElementalHearts/DiscipleProgress/Progress_$('{0:00}' -f $_).png"})
foreach($p in $icons){$b=[Drawing.Bitmap]::new($p);try{if($b.Width -ne 256 -or $b.Height -ne 256 -or $b.GetPixel(0,0).A -ne 0){throw "Invalid icon: $p"}}finally{$b.Dispose()}}
if((@($icons | Select-Object -Skip 1 | ForEach-Object {Get-FileHash -LiteralPath $_} | Select-Object -ExpandProperty Hash | Sort-Object -Unique)).Count -ne 6){throw 'Progress stages are not distinct'}
[xml]$jobs=Get-Content -LiteralPath "$root/Defs/ElementalBestow.xml"
if($jobs.Defs.JobDef.Count -ne 2){throw 'Bestow jobs missing'}
foreach($job in $jobs.Defs.JobDef){if($job.playerInterruptible -ne 'false' -or $job.casualInterruptible -ne 'false' -or $job.suspendable -ne 'false'){throw 'Bestow jobs must hold both participants'}}
Write-Output "PASS: $count regal body/direction sprites have higher full-height fits and opaque torso samples; centered fronts, Bestow icon, six distinct progress icons and held job definitions validated."
Write-Output 'LIMIT: actual head/skin fit, native west mirroring, other body mods and GPU rendering require an in-game check.'

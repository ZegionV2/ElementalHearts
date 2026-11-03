param([Parameter(Mandatory=$true)][string]$SourcePack,[Parameter(Mandatory=$true)][string]$EffectsPack)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Drawing.Imaging;
public static class EarthTextureExport {
 public static void Resize(string source,string dest,int size){
  using(var im=new Bitmap(source))using(var output=new Bitmap(size,size,PixelFormat.Format32bppArgb)){
   using(var g=Graphics.FromImage(output)){g.CompositingMode=CompositingMode.SourceCopy;g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=PixelOffsetMode.HighQuality;g.DrawImage(im,new Rectangle(0,0,size,size),0,0,im.Width,im.Height,GraphicsUnit.Pixel);}
   output.Save(dest,ImageFormat.Png);
  }
 }
 static int[] Bounds(Bitmap b,int y,int threshold){int min=b.Width,max=-1;for(int x=0;x<b.Width;x++)if(b.GetPixel(x,y).A>threshold){min=Math.Min(min,x);max=x;}return new[]{min,max};}
 static int[] Vertical(Bitmap b,int threshold){int min=b.Height,max=-1;for(int y=0;y<b.Height;y++){var row=Bounds(b,y,threshold);if(row[1]>=row[0]){min=Math.Min(min,y);max=y;}}return new[]{min,max};}
 // Align each source row to the native garment silhouette. Native alpha supplies
 // direction/body coverage only; source colors remain entirely the generated Earth art.
 public static void Fit(string source,string silhouette,string dest){
  using(var src=new Bitmap(source))using(var mask=new Bitmap(silhouette))using(var output=new Bitmap(256,256,PixelFormat.Format32bppArgb)){
   var sy=Vertical(src,32);var my=Vertical(mask,16);
   for(int y=my[0];y<=my[1];y++){
    int sampleY=sy[0]+(int)Math.Round((y-my[0])*(sy[1]-sy[0])/(double)Math.Max(1,my[1]-my[0]));
    var sr=Bounds(src,sampleY,32);var mr=Bounds(mask,y,16);if(sr[1]<sr[0]||mr[1]<mr[0])continue;
    for(int x=mr[0];x<=mr[1];x++){var m=mask.GetPixel(x,y);if(m.A==0)continue;int sx=sr[0]+(int)Math.Round((x-mr[0])*(sr[1]-sr[0])/(double)Math.Max(1,mr[1]-mr[0]));var c=src.GetPixel(sx,sampleY);output.SetPixel(x,y,Color.FromArgb(Math.Min(c.A,m.A),c.R,c.G,c.B));}
   }
   output.Save(dest,ImageFormat.Png);
  }
 }
}
'@
$earthModRoot=Split-Path $PSScriptRoot -Parent
$earthArmor=Join-Path $earthModRoot 'Textures\Things\Pawn\Humanlike\Apparel\EarthUniform'
$earthIcons=Join-Path $earthModRoot 'Textures\UI\Commands\ElementalHearts\Earth'
$earthFrames=Join-Path $earthModRoot 'Textures\Effects\ElementalHearts\Earth\Frames'
New-Item -ItemType Directory -Force -Path $earthArmor,$earthIcons,$earthFrames | Out-Null
foreach($earthDirection in @('south','east','north')){
 foreach($earthBody in @('Male','Female','Thin','Fat','Hulk','Child','Baby')){
  $earthSource=Join-Path $SourcePack ("Armor\EarthUniform_Male_$earthDirection.png")
  $earthMask=Join-Path $earthModRoot ("Textures\Things\Pawn\Humanlike\Apparel\WindUniform\WindUniform"+"_$earthBody"+"_$earthDirection.png")
  [EarthTextureExport]::Fit($earthSource,$earthMask,(Join-Path $earthArmor ("EarthUniform"+"_$earthBody"+"_$earthDirection.png")))
 }
}
Copy-Item -LiteralPath (Join-Path $earthArmor 'EarthUniform_Male_south.png') -Destination (Join-Path $earthArmor 'EarthUniform.png')
[EarthTextureExport]::Resize((Join-Path $SourcePack 'Items\Faultbreaker.png'),(Join-Path $earthModRoot 'Textures\Things\Item\Equipment\WeaponMelee\EarthSword.png'),256)
[EarthTextureExport]::Resize((Join-Path $SourcePack 'Items\EarthSerum.png'),(Join-Path $earthModRoot 'Textures\Things\Item\Health\ElementalHearts\EarthSerum.png'),256)
foreach($earthName in @('FaultlineCleave','StonebreakerAdvance','BastionStance','Mountainfall')){
 [EarthTextureExport]::Resize((Join-Path $SourcePack ("Icons\$earthName.png")),(Join-Path $earthIcons ($earthName+'.png')),256)
}
Get-ChildItem -LiteralPath (Join-Path $EffectsPack 'Textures\Effects\ElementalHearts\Earth\Frames') -Filter '*.png' | ForEach-Object{
 [EarthTextureExport]::Resize($_.FullName,(Join-Path $earthFrames $_.Name),256)
}
Write-Output 'Exported 21 fitted body/direction textures, inventory art, four icons and 42 animation frames.'


param(
    [string]$RimWorldDir = 'C:\Games\RimWorld',
    [string]$HarmonyPath = ''
)
$ErrorActionPreference = 'Stop'
if (!$HarmonyPath) { $HarmonyPath = Join-Path $RimWorldDir 'Mods\2009463077\1.6\Assemblies\0Harmony.dll' }
$managed = Join-Path $RimWorldDir 'RimWorldWin64_Data\Managed'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
foreach ($p in @($compiler, "$managed\Assembly-CSharp.dll", "$managed\UnityEngine.CoreModule.dll", $HarmonyPath)) {
    if (!(Test-Path -LiteralPath $p)) { throw "Required build input missing: $p" }
}
$output = Join-Path $PSScriptRoot '..\Assemblies'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$sources = @(Get-ChildItem $PSScriptRoot -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\obj\\' } | ForEach-Object FullName)
& $compiler /noconfig /nologo /nostdlib+ /target:library /optimize+ "/out:$output\ElementalHearts.dll" "/reference:$managed\mscorlib.dll" "/reference:$managed\System.dll" "/reference:$managed\System.Core.dll" "/reference:$managed\netstandard.dll" "/reference:$managed\Unity.Mathematics.dll" "/reference:$managed\UnityEngine.IMGUIModule.dll" "/reference:$managed\UnityEngine.TextRenderingModule.dll" "/reference:$managed\Assembly-CSharp.dll" "/reference:$managed\UnityEngine.CoreModule.dll" "/reference:$HarmonyPath" $sources
if ($LASTEXITCODE -ne 0) { throw "Compilation failed ($LASTEXITCODE)" }
Write-Output 'Built Assemblies/ElementalHearts.dll against the specified local game and Harmony assemblies.'







param([string]$RimWorldDir='C:\Games\RimWorld')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$managed=Join-Path $RimWorldDir 'RimWorldWin64_Data\Managed'
$modDll=(Resolve-Path (Join-Path $root 'Assemblies\ElementalHearts.dll')).Path
$src=(Resolve-Path (Join-Path $PSScriptRoot 'StormBlitzRegression.cs')).Path
$folder=Join-Path $env:TEMP 'ElementalHeartsStormBlitzTests'
New-Item -ItemType Directory -Force -Path $folder | Out-Null
$output=Join-Path $folder 'StormBlitzRegression.exe'
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /noconfig /nostdlib+ /target:exe /reference:System.Xml.dll "/out:$output" "/reference:$managed\mscorlib.dll" "/reference:$managed\System.dll" "/reference:$managed\System.Core.dll" "/reference:$managed\netstandard.dll" "/reference:$managed\Unity.Mathematics.dll" "/reference:$managed\Assembly-CSharp.dll" "/reference:$managed\UnityEngine.CoreModule.dll" "/reference:$managed\UnityEngine.IMGUIModule.dll" "/reference:$modDll" $src
if($LASTEXITCODE -ne 0){throw 'Test compilation failed'}
$runtime=(& dotnet --list-runtimes | Where-Object {$_ -match '^Microsoft.NETCore.App '} | ForEach-Object {($_ -split ' ')[1]} | Sort-Object {[version]$_} -Descending | Select-Object -First 1)
if(!$runtime){throw 'A modern .NET runtime is required for native job/toil fixture checks.'}
@{runtimeOptions=@{tfm=('net'+([version]$runtime).Major+'.0');framework=@{name='Microsoft.NETCore.App';version=$runtime}}} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $folder 'StormBlitzRegression.runtimeconfig.json')
& dotnet $output $managed $modDll (Join-Path $RimWorldDir 'Mods\2009463077\1.6\Assemblies\0Harmony.dll')
if($LASTEXITCODE -ne 0){throw 'StormBlitz regressions failed'}






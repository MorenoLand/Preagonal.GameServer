$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$BuildDir = Join-Path $PSScriptRoot "build"
$VsDevCmd = "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat"
$VcpkgToolchain = "C:\vcpkg\scripts\buildsystems\vcpkg.cmake"

if (-not (Test-Path $VsDevCmd)) {
    throw "VsDevCmd.bat not found at $VsDevCmd"
}

if (-not (Test-Path $VcpkgToolchain)) {
    throw "vcpkg toolchain not found at $VcpkgToolchain"
}

New-Item -ItemType Directory -Force -Path $BuildDir | Out-Null

cmd /c "`"$VsDevCmd`" -arch=x64 && cmake -S `"$PSScriptRoot`" -B `"$BuildDir`" -G Ninja -DCMAKE_BUILD_TYPE=Release -DCMAKE_TOOLCHAIN_FILE=`"$VcpkgToolchain`" -DVCPKG_TARGET_TRIPLET=x64-windows && cmake --build `"$BuildDir`" && `"$BuildDir\gs2lib_filequeue_fixtures.exe`""
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

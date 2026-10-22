# Builds Emgu TF (managed code, and optionally packages) with CMake and Visual Studio.
#
#   -Doc      build the documentation
#   -Nuget    build the NuGet packages
#   -Package  build the zip / exe packages
#
# The native tfextern.dll has to be built first (bazel_build_tf.ps1), otherwise only the managed parts are built.
[CmdletBinding()]
param(
    [switch]$Doc,
    [switch]$Nuget,
    [switch]$Package
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\common.ps1"

$root = Get-EmguTfRoot
$hasTfFull = Test-Path (Join-Path $root 'lib\runtimes\win-x64\native\tfextern.dll')

# If we are a 32-bit process on 64-bit Windows, PROCESSOR_ARCHITEW6432 is set.
$arch = $env:PROCESSOR_ARCHITECTURE
if ($env:PROCESSOR_ARCHITEW6432) { $arch = $env:PROCESSOR_ARCHITEW6432 }
switch ($arch) {
    'AMD64' { $cmakeArch = 'x64' }
    'ARM64' { $cmakeArch = 'ARM64' }
    'x86'   { $cmakeArch = 'Win32' }
    default { throw "Unsupported processor architecture: $arch" }
}
Write-Host "Generating $cmakeArch solution"

$vsDir = Find-VisualStudio -Root $root
if (-not $vsDir) { throw 'Visual Studio 2019 or later with the C++ workload was not found.' }
$cmake = Find-CMake -VisualStudioDir $vsDir
$generator = @(Get-CMakeGenerator -VisualStudioDir $vsDir)
$generator[2] = $cmakeArch

Push-Location $root
try {
    New-Item -ItemType Directory -Force -Path 'b', 'package' | Out-Null

    $configureArgs = @('..', '-G', $generator[0], '-A', $generator[2])
    if ($Doc) { $configureArgs += '-DEMGU_TF_DOCUMENTATION_BUILD:BOOL=TRUE' }

    Push-Location 'b'
    try {
        & $cmake @configureArgs
        if ($LASTEXITCODE -ne 0) { throw "CMake configure failed with exit code $LASTEXITCODE" }

        $targets = @('ALL_BUILD')
        if ($Package) { $targets += 'PACKAGE' }
        if ($Doc -and $hasTfFull) { $targets += 'Emgu.TF.Document.Html' }
        if ($Nuget -and $hasTfFull) { $targets += 'Emgu.TF.runtime.windows.nuget' }

        Write-Host "BUILDING TARGETS: $($targets -join ' ')"
        & $cmake --build . --config Release --target @targets
        if ($LASTEXITCODE -ne 0) { throw "CMake build failed with exit code $LASTEXITCODE" }

        if ($Package) {
            Copy-Item '*.zip', '*.exe' -Destination '..\package' -Force
        }
    } finally {
        Pop-Location
    }

    if ($Nuget -and $hasTfFull) {
        Copy-Item 'platforms\nuget\*.nupkg' -Destination 'package' -Force
    }
} finally {
    Pop-Location
}

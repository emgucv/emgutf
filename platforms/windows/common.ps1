# Shared helpers for the Windows build scripts. Dot-source this file:  . "$PSScriptRoot\common.ps1"
# Works with Windows PowerShell 5.1 and PowerShell 7+.

function Get-EmguTfRoot {
    # <repo>\platforms\windows\common.ps1 -> <repo>
    return (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}

function Find-VisualStudio {
    # Returns the installation path of the newest Visual Studio (2019 or later) that has the C++ tools,
    # or $null if none is found.
    param([string]$Root = (Get-EmguTfRoot))

    $vswhere = Join-Path $Root 'miscellaneous\vswhere.exe'
    if (-not (Test-Path $vswhere)) { return $null }

    $path = & $vswhere -latest -products '*' -version '[16.0,)' `
        -requires 'Microsoft.VisualStudio.Component.VC.Tools.x86.x64' `
        -property installationPath | Select-Object -First 1
    if ($path) { return $path.Trim() }
    return $null
}

function Find-CMake {
    param([string]$VisualStudioDir)

    $candidates = @()
    if ($VisualStudioDir) {
        $candidates += Join-Path $VisualStudioDir 'Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe'
    }
    foreach ($dir in @($env:ProgramW6432, $env:ProgramFiles, ${env:ProgramFiles(x86)})) {
        if ($dir) { $candidates += Join-Path $dir 'CMake\bin\cmake.exe' }
    }
    foreach ($c in $candidates) {
        if (Test-Path $c) { return $c }
    }
    return 'cmake.exe'
}

function Get-CMakeGenerator {
    # Maps the installed Visual Studio to the CMake generator name (x64 only).
    param([string]$VisualStudioDir)

    $major = 0
    if ($VisualStudioDir) {
        $leaf = Split-Path $VisualStudioDir -Parent | Split-Path -Leaf
        if ($leaf -match '^(\d+)$') {
            $major = [int]$Matches[1]
        } elseif ($leaf -match '^20(\d\d)$') {
            # "...\Microsoft Visual Studio\2019\Professional" style folders
            $year = [int]("20" + $Matches[1])
            if ($year -ge 2022) { $major = 17 } elseif ($year -ge 2019) { $major = 16 }
        }
    }
    switch ($major) {
        18 { return @('Visual Studio 18 2026', '-A', 'x64') }
        17 { return @('Visual Studio 17 2022', '-A', 'x64') }
        16 { return @('Visual Studio 16 2019', '-A', 'x64') }
        default { throw "Unsupported or unknown Visual Studio installation: '$VisualStudioDir'" }
    }
}

function Find-PythonBase {
    # Returns the directory of the python.exe used to build TensorFlow, and sets the hermetic python version
    # when one of the known 3.12 locations is used. Matches the behavior of the old bazel_build_tf.bat.
    $base = $null
    $py = Get-Command python -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($py) {
        $exe = & $py.Source -c 'import sys; print(sys.executable)'
        if ($exe) { $base = Split-Path $exe.Trim() -Parent }
    }
    if (Test-Path 'C:\Python312') {
        $base = 'C:\Python312'
        $env:HERMETIC_PYTHON_VERSION = '3.12'
    }
    if (Test-Path 'C:\python-virt\python312') {
        $base = 'C:\python-virt\python312'
        $env:HERMETIC_PYTHON_VERSION = '3.12'
    }
    return $base
}

function Copy-MsvcRedist {
    # Copies the x64 MSVC runtime DLLs of the newest redistributable under <VC>\Redist\MSVC to $Destination.
    param(
        [string]$VcDir,
        [string]$Destination
    )

    if (-not $VcDir) { return }
    $redistRoot = Join-Path $VcDir 'Redist\MSVC'
    if (-not (Test-Path $redistRoot)) { return }

    $version = Get-ChildItem $redistRoot -Directory |
        Where-Object { $_.Name -match '^14' } |
        Sort-Object { [version]$_.Name } -Descending |
        Select-Object -First 1
    if (-not $version) { return }

    $crt = Get-ChildItem (Join-Path $version.FullName 'x64') -Directory -Filter 'Microsoft.VC*.CRT' |
        Select-Object -First 1
    if (-not $crt) { return }

    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    Get-ChildItem $crt.FullName -Filter '*.dll' | ForEach-Object {
        Copy-Item $_.FullName -Destination $Destination -Force
        Write-Host "Copied $($_.FullName)"
    }
}

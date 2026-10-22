# Builds the Emgu TF native library (tfextern.dll) for Windows x64 with Bazel.
#
#   -Avx2         add /arch:AVX2 to the compiler flags
#   -Docker       build inside a docker container (uses c:\bazel_output_* and remote/docker sandbox flags)
#   -Fast         low optimization / reduced debug info build. Much faster to compile, but the binary is slower
#                 and bigger. Use for CI and development checks, not for binaries you ship.
#   -FastOptLevel optimization level used by -Fast: Od (default, no optimization) or O1.
#                 Measured on cwise_op_select.cc: /O2 107 min, /O1 65 min, /Od 4 min.
#   -DiskCache    directory for Bazel's --disk_cache
#   -OutputRoot   put the Bazel output_base and output_user_root under this folder instead of next to the script
#                 (the output of a full build is 25-40 GB; useful when the repository drive is small)
#   -DryRun       print the Bazel command and environment without running it
#
# Requirements: Bazel (bazelisk) on PATH or at C:\msys64\usr\bin\bazel.exe, Python 3.12,
#               Visual Studio 2019+ with the C++ workload.
[CmdletBinding()]
param(
    [switch]$Avx2,
    [switch]$Docker,
    [switch]$Fast,
    [ValidateSet('O1', 'Od')]
    [string]$FastOptLevel = 'Od',
    [string]$DiskCache,
    [string]$OutputRoot,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\common.ps1"

$root = Get-EmguTfRoot
$nativeRuntimeDir = Join-Path $root 'lib\runtimes\win-x64\native'

# ---- Visual Studio / LLVM / Python -----------------------------------------------------------------------------
$vsDir = Find-VisualStudio -Root $root
if ($vsDir) {
    $env:BAZEL_VS = $vsDir
    $env:BAZEL_VC = Join-Path $vsDir 'VC'
}
Write-Host "Using BAZEL_VC=$($env:BAZEL_VC)"

# Prefer the standalone LLVM if installed. The clang bundled with VS2026 reports a git hash instead of a version
# number, which breaks Bazel's builtin include directory detection.
# The literal quotes are kept on purpose: they keep the generated toolchain identical to the one built by
# bazel_build_tf.bat, so an existing Bazel cache stays valid.
$standaloneLlvm = 'C:\Program Files\LLVM'
if (Test-Path (Join-Path $standaloneLlvm 'bin')) { $env:BAZEL_LLVM = '"' + $standaloneLlvm + '"' }
if ($env:BAZEL_VC -and -not (Test-Path (Join-Path $standaloneLlvm 'bin\clang-cl.exe'))) {
    $env:BAZEL_LLVM = Join-Path $env:BAZEL_VC 'Tools\Llvm\x64'
}
Write-Host "Using BAZEL_LLVM=$($env:BAZEL_LLVM)"

$pythonBase = Find-PythonBase
if (-not $pythonBase) { throw 'python.exe was not found. Install Python 3.12 and make sure it is on PATH.' }
$env:PYTHON_BIN_PATH = Join-Path $pythonBase 'python.exe'
$env:PYTHON_LIB_PATH = Join-Path $pythonBase 'lib\site-packages'
Write-Host "PYTHON_BASE_PATH=$pythonBase"
Write-Host "PYTHON_BIN_PATH=$($env:PYTHON_BIN_PATH)"

# ---- Bazel output folders ------------------------------------------------------------------------------------------
$dockerFlags = @()
if ($Docker) {
    $dockerFlags = @('--define=EXECUTOR=remote', '--experimental_docker_verbose',
        '--experimental_enable_docker_sandbox', '--jobs=2')
    $outputUserRoot = 'c:\bazel_output_user_root'
    $outputBase = 'c:\bazel_output_base'
} else {
    $outputParent = $PSScriptRoot
    if ($OutputRoot) { $outputParent = $OutputRoot }
    $outputUserRoot = Join-Path $outputParent 'output_user_root'
    $outputBase = Join-Path $outputParent 'output_base'
}

# ---- Bazel command line ------------------------------------------------------------------------------------------
$bazel = 'bazel.exe'
if (Test-Path 'C:\msys64\usr\bin\bazel.exe') { $bazel = 'C:\msys64\usr\bin\bazel.exe' }

$repoPath = "$pythonBase;$($env:PATH)"

$bazelArgs = @(
    "--output_base=$outputBase",
    "--output_user_root=$outputUserRoot",
    'build',
    "--repo_env=PATH=$repoPath",
    '//tensorflow/tfextern:libtfextern.so',
    '--verbose_failures'
) + $dockerFlags + @('--config=win_clang')

if ($Avx2) { $bazelArgs += '--copt=/arch:AVX2' }

if ($Fast) {
    Write-Warning '-Fast: building with reduced optimization. Do not ship the resulting tfextern.dll.'
    $bazelArgs += @(
        '--platform_suffix=fast',                 # separate output tree, keeps the normal build cache intact
        "--copt=/$FastOptLevel",                  # overrides the /O2 of the opt compilation mode
        '--copt=/clang:-w',                       # win_clang enables -Weverything; skip the diagnostics work
        '--linkopt=/DEBUG:NONE',
        '--linkopt=/OPT:NOICF',
        '--local_ram_resources=HOST_RAM*.75',     # parallel clang-cl jobs of heavy kernels can run out of memory
        '--show_progress_rate_limit=30'
    )
}

if ($DiskCache) { $bazelArgs += "--disk_cache=$DiskCache" }

if ($DryRun) {
    Write-Host ''
    Write-Host "Working directory: $(Join-Path $root 'tensorflow')"
    Write-Host "$bazel"
    $bazelArgs | ForEach-Object { Write-Host "  $_" }
    Write-Host ''
    foreach ($name in 'BAZEL_VS', 'BAZEL_VC', 'BAZEL_LLVM', 'PYTHON_BIN_PATH', 'PYTHON_LIB_PATH', 'HERMETIC_PYTHON_VERSION') {
        Write-Host ("  {0}={1}" -f $name, (Get-Item "env:$name" -ErrorAction SilentlyContinue).Value)
    }
    return
}

New-Item -ItemType Directory -Force -Path $outputUserRoot, $outputBase | Out-Null

# ---- Build -------------------------------------------------------------------------------------------------------
Push-Location (Join-Path $root 'tensorflow')
try {
    & $bazel @bazelArgs
    if ($LASTEXITCODE -ne 0) { throw "Bazel failed with exit code $LASTEXITCODE" }
} finally {
    Pop-Location
}

# ---- Deploy ---------------------------------------------------------------------------------------------------------
$built = Join-Path $root 'tensorflow\bazel-bin\tensorflow\tfextern\libtfextern.so'
if (-not (Test-Path $built)) { throw "Bazel reported success but $built does not exist" }

New-Item -ItemType Directory -Force -Path $nativeRuntimeDir | Out-Null
# Copy to a temporary file first, so a failed copy (for example a full disk) does not destroy the previous dll.
$target = Join-Path $nativeRuntimeDir 'tfextern.dll'
$temp = "$target.tmp"
try {
    Copy-Item $built $temp -Force
    Move-Item $temp $target -Force
} finally {
    if (Test-Path $temp) { Remove-Item $temp -Force }
}
Write-Host "Copied tfextern.dll to $nativeRuntimeDir"

Copy-MsvcRedist -VcDir $env:BAZEL_VC -Destination $nativeRuntimeDir

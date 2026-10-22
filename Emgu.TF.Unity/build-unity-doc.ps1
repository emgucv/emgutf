# Runs the build-unity-doc shell script through WSL.
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    wsl dos2unix build-unity-doc
    wsl ./build-unity-doc
} finally {
    Pop-Location
}

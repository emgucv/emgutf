# Runs the copy_unity_assets shell script through WSL.
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    wsl dos2unix copy_unity_assets
    wsl ./copy_unity_assets
} finally {
    Pop-Location
}

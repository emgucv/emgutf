# Runs the sync_unity_assets shell script through WSL.
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    wsl dos2unix sync_unity_assets
    wsl ./sync_unity_assets
} finally {
    Pop-Location
}

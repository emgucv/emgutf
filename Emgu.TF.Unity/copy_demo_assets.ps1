# Runs the copy_demo_assets shell script through WSL.
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    wsl dos2unix copy_demo_assets
    wsl ./copy_demo_assets
} finally {
    Pop-Location
}

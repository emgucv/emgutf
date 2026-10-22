# Converts a zip file into a 7-Zip self extracting exe:  .\zip2exe.ps1 <file.zip>
# The exe is created as <file.zip>.selfextract.exe inside the zip2exe_tmp folder.
param(
    [Parameter(Mandatory = $true)][string]$Zip,
    [string]$SevenZip = 'C:\Program Files\7-Zip\7z.exe'
)
$ErrorActionPreference = 'Stop'

$zipPath = (Resolve-Path $Zip).Path
$zipName = Split-Path $zipPath -Leaf
if (Test-Path zip2exe_tmp) { Remove-Item zip2exe_tmp -Recurse -Force }
New-Item -ItemType Directory -Path zip2exe_tmp | Out-Null

& $SevenZip x $zipPath -ozip2exe_tmp -y
if ($LASTEXITCODE -ne 0) { throw "7z extract failed with exit code $LASTEXITCODE" }

Push-Location zip2exe_tmp
try {
    & $SevenZip a -sfx7z.sfx "$zipName.selfextract.exe" '*' -mx=9 -m0=LZMA2 -mmt=off -md=512m
    if ($LASTEXITCODE -ne 0) { throw "7z archive failed with exit code $LASTEXITCODE" }
} finally {
    Pop-Location
}

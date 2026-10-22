# Removes all untracked and ignored files from the repository and the tensorflow submodule (git clean -d -fx).
# WARNING: this also deletes build outputs such as lib\, b\ and the Bazel output folders.
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

Push-Location (Join-Path $root 'tensorflow')
try {
    # bazel-* are symbolic links / junctions: delete the link only, never the folder it points to
    Get-ChildItem -Force -Filter 'bazel-*' | ForEach-Object {
        if ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            [IO.Directory]::Delete($_.FullName)
        } else {
            Remove-Item $_.FullName -Recurse -Force
        }
    }
    git clean -d -fx '.'
} finally {
    Pop-Location
}

Push-Location $root
try {
    git clean -d -fx '.'
} finally {
    Pop-Location
}

# Builds tfextern.dll for Windows x64. See bazel_build_tf.ps1 for the options (-Avx2, -Docker, -Fast, ...).
& "$PSScriptRoot\bazel_build_tf.ps1" @args
exit $LASTEXITCODE

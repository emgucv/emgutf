# Builds the Emgu TF native binary and packages inside a Windows docker container.
# Extra arguments are passed to bazel_build_tf_x86_64.ps1 (for example -Avx2 or -Fast).
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
docker run -v "${root}:c:\src" -w c:\src\platforms\windows\docker -m 16G emgu/vs2019_buildtools_cuda_bazel:latest `
    powershell -NoProfile -ExecutionPolicy Bypass -File Docker_Build_Helper_tf.ps1 @args
exit $LASTEXITCODE

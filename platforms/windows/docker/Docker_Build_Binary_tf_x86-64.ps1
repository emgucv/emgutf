# Builds the Emgu TF native binary and packages for Windows x64 (CPU) inside a docker container.
& "$PSScriptRoot\Docker_Build_Binary_tf.ps1" @args
exit $LASTEXITCODE

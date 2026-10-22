# Runs inside the docker container: copies the sources, builds tfextern.dll and the Emgu TF packages,
# then copies the results back to the mounted source folder (c:\src).
# Extra arguments are passed to bazel_build_tf_x86_64.ps1.
$ErrorActionPreference = 'Stop'

Set-Location c:\
xcopy c:\src c:\TEMP /s /e /h /i /y
Set-Location c:\TEMP\platforms\windows

.\bazel_build_tf_x86_64.ps1 -Docker @args
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

New-Item -ItemType Directory -Force -Path c:\src\lib | Out-Null
xcopy c:\TEMP\lib c:\src\lib /s /e /h /i /y

.\build_emgutf.ps1 -Doc -Nuget -Package
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

New-Item -ItemType Directory -Force -Path c:\src\package | Out-Null
Copy-Item c:\TEMP\package\*.zip, c:\TEMP\package\*.exe -Destination c:\src\package -Force

REM go to the folder of the current script
pushd %~p0
cd ..
docker run -v %cd%\..\..:c:\bb\tflite_x86-64\build -w c:\bb\tflite_x86-64\build\platforms\windows emgu/vs2019_buildtools_cuda_bazel:latest .\bazel_build_tflite_x86_64.bat 64 no_xnn docker

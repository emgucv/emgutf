REM @echo off

REM POSSIBLE OPTIONS: 
REM %1%: "64", "ARM"
REM %2%: "gpu", build with CUDA
REM %3%: "mkl", build with intel mkl
REM %4%: "docker", build within docker 

pushd %~p0
cd ..\..

IF "%1%"=="64" ECHO "BUILDING 64bit solution" 
IF "%1%"=="ARM" ECHO "BUILDING ARM solution"

SET OS_MODE=
IF "%1%"=="64" SET OS_MODE= Win64
IF "%1%"=="ARM" SET OS_MODE= ARM

SET BUILD_TOOLS_FOLDER=C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools

REM IF NOT "%4%"=="clean_bazel" GOTO END_OF_CLEAN_BAZEL
REM rm -rf %USERPROFILE%\_bazel_%USERNAME%
REM :END_OF_CLEAN_BAZEL


SET PROGRAMFILES_DIR_X86=%programfiles(x86)%
if NOT EXIST "%PROGRAMFILES_DIR_X86%" SET PROGRAMFILES_DIR_X86=%programfiles%
SET PROGRAMFILES_DIR=%programfiles%

REM SET Java path
REM IF EXIST "%PROGRAMFILES_DIR%\Java\jdk1.8.0_201" SET ABSOLUTE_JAVABASE=%PROGRAMFILES_DIR%\Java\jdk1.8.0_201

REM Find Visual Studio or Msbuild
FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [15.0^,16.0^) -property installationPath`) DO SET VS2017_DIR=%%F
SET VS2017=%VS2017_DIR%\Common7\IDE\devenv.com

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [16.0^,17.0^) -property installationPath`) DO SET VS2019_DIR=%%F
SET VS2019=%VS2019_DIR%\Common7\IDE\devenv.com

IF EXIST "%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe" SET MSBUILD35=%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe
IF EXIST "%windir%\Microsoft.NET\Framework64\v3.5\MSBuild.exe" SET MSBUILD35=%windir%\Microsoft.NET\Framework64\v3.5\MSBuild.exe
IF EXIST "%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" SET MSBUILD40=%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

IF EXIST "%MSBUILD35%" SET DEVENV=%MSBUILD35%
IF EXIST "%MSBUILD40%" SET DEVENV=%MSBUILD40%
IF EXIST "%VS2017%" SET DEVENV=%VS2017%

REM Only use VS2017 in GPU build: CUDA 10.0 only support up to VS2017
REM IF "%2%" == "gpu" GOTO SET_BAZEL_VS_VC
IF EXIST "%VS2019%" SET DEVENV=%VS2019%
IF EXIST "%BUILD_TOOLS_FOLDER%" SET DEVENV=%BUILD_TOOLS_FOLDER%

:SET_BAZEL_VS_VC
IF "%DEVENV%"=="%VS2017%" SET BAZEL_VS=%VS2017:\Common7\IDE\devenv.com=%
IF "%DEVENV%"=="%VS2019%" SET BAZEL_VS=%VS2019:\Common7\IDE\devenv.com=%
IF "%DEVENV%"=="%BUILD_TOOLS_FOLDER%" SET BAZEL_VS=%BUILD_TOOLS_FOLDER%
IF NOT "%BAZEL_VS%"=="" SET BAZEL_VC=%BAZEL_VS%\VC
ECHO Using BAZEL_VC=%BAZEL_VC%


IF EXIST "%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\Shared\Python37_64" SET PYTHON_BASE_PATH=%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\Shared\Python37_64
IF EXIST "C:\python-virt\python37" SET PYTHON_BASE_PATH=C:\python-virt\python37
IF EXIST "C:\python38" SET PYTHON_BASE_PATH=C:\python38

SET PYTHON_BIN_PATH=%PYTHON_BASE_PATH%\python.exe
SET PYTHON_LIB_PATH=%PYTHON_BASE_PATH%\lib\site-packages

SET PYTHON_BASE_PATH=%PYTHON_BASE_PATH:\=/%
SET PYTHON_BIN_PATH=%PYTHON_BIN_PATH:\=/%
SET PYTHON_LIB_PATH=%PYTHON_LIB_PATH:\=/%

IF NOT "%4%"=="docker" GOTO ENV_NOT_DOCKER

:ENV_DOCKER
SET DOCKER_FLAGS=--define=EXECUTOR=remote --experimental_docker_verbose --experimental_enable_docker_sandbox --jobs=2
SET OUTPUT_USER_ROOT_DIR=c:\bazel_output_user_root
SET OUTPUT_BASE_DIR=c:\bazel_output_base
GOTO END_OF_DOCKER

:ENV_NOT_DOCKER
SET OUTPUT_USER_ROOT_DIR=%~dp0output_user_root
SET OUTPUT_BASE_DIR=%~dp0output_base

:END_OF_DOCKER
IF NOT EXIST %OUTPUT_USER_ROOT_DIR% mkdir %OUTPUT_USER_ROOT_DIR%
IF NOT EXIST %OUTPUT_BASE_DIR% mkdir %OUTPUT_BASE_DIR%

REM BUILD TENSORFLOW
@echo on
REM cp -r tfextern tensorflow/tensorflow
REM cp platforms/windows/libtensorflow_cpu.sh tensorflow/tensorflow/tools/ci_build/windows/
cd tensorflow\tensorflow\tools\ci_build\windows

SET MSYS64_PATH=c:\msys64
SET MSYS64_BIN=%MSYS64_PATH%\usr\bin

IF "%3%" == "mkl" GOTO BUILD_WITH_MKL
GOTO END_MKL
:BUILD_WITH_MKL
SET TF_BAZEL_EXTRA_CONFIG=--config=mkl

:END_MKL

IF "%2%" == "gpu" GOTO BUILD_GPU
:BUILD_CPU
call cmd.exe /v /c "set PATH=%MSYS64_BIN%;%PATH% & %MSYS64_BIN%\bash.exe libtensorflow_cpu.sh"
GOTO END_OF_BUILD

:BUILD_GPU
REM SET TF_CUDA_VERSION=10.0
REM SET TF_CUDA_VERSION=10.1
REM SET TF_CUDA_VERSION=10.2
REM SET TF_CUDA_VERSION=11.0
REM SET TF_CUDA_VERSION=11.1
SET TF_CUDA_VERSION=11.3
REM SET TF_CUDNN_VERSION=7.4
REM SET TF_CUDNN_VERSION=7.5
REM SET TF_CUDNN_VERSION=7.6
REM SET TF_CUDNN_VERSION=7
SET TF_CUDNN_VERSION=8
REM SET TF_CUDA_COMPUTE_CAPABILITIES=3.5,7.0
REM SET TF_CUDA_COMPUTE_CAPABILITIES=3.7
SET TF_CUDA_COMPUTE_CAPABILITIES=5.2
REM SET TF_CUDA_COMPUTE_CAPABILITIES=6.0
REM SET TF_CUDA_COMPUTE_CAPABILITIES=7.0
SET CUDA_TOOLKIT_PATH=C:/Program Files/NVIDIA GPU Computing Toolkit/CUDA/v%TF_CUDA_VERSION%
SET CUDNN_INSTALL_PATH=%CUDA_TOOLKIT_PATH%
echo %CUDA_TOOLKIT_PATH% > ../../CUDA_TOOLKIT_PATH.txt
call cmd.exe /v /c "set PATH=%MSYS64_BIN%;%CUDA_TOOLKIT_PATH%/extras/CUPTI/lib64;%PATH% & %MSYS64_BIN%\bash.exe libtensorflow_gpu.sh"

:END_OF_BUILD

set PATH=%MSYS64_BIN%;%PATH%
REM %MSYS64_PATH%\usr\bin\bash.exe libtensorflow_cpu.sh

cd ../../../../../
IF NOT EXIST lib\x64 mkdir lib\x64

REM one more try to make sure it builds, in-case bazel doesn't like msys64 bash.
cd tensorflow

SET BAZEL_COMMAND=bazel.exe
SET MSYS_PATH=C:\msys64
SET MSYS_BIN=%MSYS_PATH%\usr\bin
IF EXIST "%MSYS_BIN%\bazel.exe" SET BAZEL_COMMAND=%MSYS_BIN%\bazel.exe

REM call %BAZEL_COMMAND% --output_base=%OUTPUT_BASE_DIR% --output_user_root=%OUTPUT_USER_ROOT_DIR% build //tensorflow/tfextern:libtfextern.so --verbose_failures %DOCKER_FLAGS% --local_ram_resources="HOST_RAM*.2" --local_cpu_resources="HOST_CPUS*.2" --jobs=2
call %BAZEL_COMMAND% --output_base=%OUTPUT_BASE_DIR% --output_user_root=%OUTPUT_USER_ROOT_DIR% build //tensorflow/tfextern:libtfextern.so --verbose_failures %DOCKER_FLAGS% %TF_BAZEL_EXTRA_CONFIG%
cd ..

cp -f tensorflow/bazel-bin/tensorflow/tfextern/libtfextern.so lib/x64/tfextern.dll

:START_OF_MSVC_DEPENDENCY
IF "%BAZEL_VC%"=="" GOTO END_OF_MSVC_DEPENDENCY
IF "%DEVENV%"=="%VS2017%" GOTO VS2017_DEPENDENCY
IF "%DEVENV%"=="%VS2019%" GOTO VS2019_DEPENDENCY
IF "%DEVENV%"=="%BUILD_TOOLS_FOLDER%" GOTO VS2019_DEPENDENCY
GOTO END_OF_MSVC_DEPENDENCY

:VS2017_DEPENDENCY
for /d %%i in ( "%BAZEL_VC%\Redist\MSVC\*" ) do SET VS2017_REDIST=%%i\x64\Microsoft.VC141.CRT
copy /Y "%VS2017_REDIST%\*140.dll" lib\x64\
copy /Y "%VS2017_REDIST%\*140_1.dll" lib\x64\
copy /Y "%VS2017_REDIST%\*140_2.dll" lib\x64\
REM　rm lib\x64\vccorlib140.dll
GOTO END_OF_MSVC_DEPENDENCY

:VS2019_DEPENDENCY
for /d %%i in ( "%BAZEL_VC%\Redist\MSVC\14*" ) do SET VS2019_REDIST=%%i\x64\Microsoft.VC142.CRT
copy /Y "%VS2019_REDIST%\*.dll" lib\x64\
REM copy /Y "%VS2019_REDIST%\*140_1.dll" lib\x64\
REM copy /Y "%VS2019_REDIST%\*140_2.dll" lib\x64\
REM rm lib\x64\vccorlib140.dll

:END_OF_MSVC_DEPENDENCY

IF "%2%" == "gpu" GOTO DEPLOY_DEPENDENCY_GPU
GOTO END_OF_DEPLOY_DEPENDENCY_GPU
:DEPLOY_DEPENDENCY_GPU
SET CUDA_TOOLKIT_BIN_PATH=%CUDA_TOOLKIT_PATH%/bin
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cusolver*.dll" lib\x64\ 
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cublas*.dll" lib\x64\
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cudnn*.dll" lib\x64\
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cufft*.dll" lib\x64\
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\curand*.dll" lib\x64\
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cudart64_*.dll" lib\x64\
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cusparse*.dll" lib\x64\
:END_OF_DEPLOY_DEPENDENCY_GPU

cp -rf tensorflow\bazel-bin\external\protobuf_archive .
cp -rf tensorflow\bazel-tensorflow\external\protobuf_archive .

IF "%3%" == "mkl" GOTO DEPLOY_DEPENDENCY_MKL
GOTO END_OF_DEPLOY_DEPENDENCY_MKL
:DEPLOY_DEPENDENCY_MKL
SET TF_BAZEL_EXTRA_CONFIG=--config=mkl

:END_OF_DEPLOY_DEPENDENCY_MKL
SET INTEL_REDIST=%ONEAPI_ROOT%compiler\latest\windows\redist\intel64_win\compiler
copy /Y "%INTEL_REDIST%\LIBIOMP5MD.DLL" lib\x64\

REM IF "%4%"=="dev" GOTO END_OF_CLEAN
REM :CLEAN
REM cd tensorflow
REM bazel clean
REM bazel shutdown
REM rm -rf c:\tmp\_bazel_canming
REM rm -rf c:\tmp\install\*
REM :END_OF_CLEAN

popd

Exit /B

TITLE=BAZEL_BUILD_TF
REM TASKKILL /FI "WINDOWTITLE eq BAZEL_BUILD_TF" /F /IM cmd.txt /T

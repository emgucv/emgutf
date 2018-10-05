REM @echo off
pushd %~p0
cd ..\..

IF "%1%"=="64" ECHO "BUILDING 64bit solution" 
IF "%1%"=="ARM" ECHO "BUILDING ARM solution"
IF "%1%"=="32" ECHO "BUILDING 32bit solution"

SET OS_MODE=
IF "%1%"=="64" SET OS_MODE= Win64
IF "%1%"=="ARM" SET OS_MODE= ARM

SET PROGRAMFILES_DIR_X86=%programfiles(x86)%
if NOT EXIST "%PROGRAMFILES_DIR_X86%" SET PROGRAMFILES_DIR_X86=%programfiles%
SET PROGRAMFILES_DIR=%programfiles%

REM Find Visual Studio or Msbuild
SET VS2005=%VS80COMNTOOLS%..\IDE\devenv.com
SET VS2008=%VS90COMNTOOLS%..\IDE\devenv.com
SET VS2010=%VS100COMNTOOLS%..\IDE\devenv.com
SET VS2012=%VS110COMNTOOLS%..\IDE\devenv.com
SET VS2013=%VS120COMNTOOLS%..\IDE\devenv.com
SET VS2015=%VS140COMNTOOLS%..\IDE\devenv.com

SET VS2017_DIR=%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\2017\Community
IF EXIST "%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com" SET VS2017_DIR=%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\2017\Professional
IF EXIST "%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.com" SET VS2017_DIR=%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\2017\Enterprise
IF EXIST "%VS2017INSTALLDIR%\Common7\IDE\devenv.com" SET VS2017_DIR=%VS2017INSTALLDIR%
IF EXIST "%VS150COMNTOOLS%..\IDE\devenv.com" SET VS2017_DIR =%VS150COMNTOOLS%..\..
SET VS2017=%VS2017_DIR%\Common7\IDE\devenv.com

IF EXIST "%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe" SET MSBUILD35=%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe
IF EXIST "%windir%\Microsoft.NET\Framework64\v3.5\MSBuild.exe" SET MSBUILD35=%windir%\Microsoft.NET\Framework64\v3.5\MSBuild.exe
IF EXIST "%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" SET MSBUILD40=%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

IF EXIST "%MSBUILD35%" SET DEVENV="%MSBUILD35%"
IF EXIST "%MSBUILD40%" SET DEVENV="%MSBUILD40%"
IF EXIST "%VS2005%" SET DEVENV=%VS2005% 
IF EXIST "%VS2008%" SET DEVENV=%VS2008%
IF EXIST "%VS2010%" SET DEVENV=%VS2010%
IF EXIST "%VS2012%" SET DEVENV=%VS2012%
IF EXIST "%VS2013%" SET DEVENV=%VS2013%
IF EXIST "%VS2015%" SET DEVENV=%VS2015%
REM IF EXIST "%VS2017%" SET DEVENV=%VS2017%

:SET_BAZEL_VS_VC
IF "%DEVENV%"=="%VS2015%" SET BAZEL_VS=%VS2015:\Common7\Tools\..\IDE\devenv.com=%
IF "%DEVENV%"=="%VS2017%" SET BAZEL_VS=%VS2017:\Common7\IDE\devenv.com=%
IF NOT "%BAZEL_VS%"=="" SET BAZEL_VC=%BAZEL_VS%\VC

REM BUILD TENSORFLOW
@echo on
REM cp -r tfextern tensorflow/tensorflow
REM cp platforms/windows/libtensorflow_cpu.sh tensorflow/tensorflow/tools/ci_build/windows/
cd tensorflow\tensorflow\tools\ci_build\windows

SET MSYS64_PATH=c:\msys64
SET MSYS64_BIN=%MSYS64_PATH%\usr\bin

IF "%2%" == "gpu" GOTO BUILD_GPU
:BUILD_CPU
call cmd.exe /v /c "set PATH=%MSYS64_BIN%;%PATH% & %MSYS64_BIN%\bash.exe libtensorflow_cpu.sh"
GOTO END_OF_BUILD

:BUILD_GPU
SET TF_CUDA_VERSION=9.1
SET TF_CUDNN_VERSION=7.1
REM SET TF_CUDA_COMPUTE_CAPABILITIES=3.5,7.0
SET TF_CUDA_COMPUTE_CAPABILITIES=3.7
SET CUDA_TOOLKIT_PATH=C:/Program Files/NVIDIA GPU Computing Toolkit/CUDA/v%TF_CUDA_VERSION%
SET CUDNN_INSTALL_PATH=%CUDA_TOOLKIT_PATH%
echo C:/Program Files/NVIDIA GPU Computing Toolkit/CUDA/v%TF_CUDA_VERSION% > ../../CUDA_TOOLKIT_PATH.txt
call cmd.exe /v /c "set PATH=%MSYS64_BIN%;%PATH% & %MSYS64_BIN%\bash.exe libtensorflow_gpu.sh"

:END_OF_BUILD

set PATH=%MSYS64_BIN%;%PATH%
REM %MSYS64_PATH%\usr\bin\bash.exe libtensorflow_cpu.sh

cd ../../../../../
IF NOT EXIST lib\x64 mkdir lib\x64
cp -f tensorflow/bazel-bin/tensorflow/tfextern/libtfextern.so lib/x64/tfextern.dll

:START_OF_MSVC_DEPENDENCY
IF "%BAZEL_VC%"=="" GOTO END_OF_MSVC_DEPENDENCY
IF "%DEVENV%"=="%VS2015%" GOTO VS2015_DEPEDENCY
IF "%DEVENV%"=="%VS2017%" GOTO VS2017_DEPEDENCY
GOTO END_OF_MSVC_DEPENDENCY

:VS2015_DEPEDENCY
copy /Y "%BAZEL_VC%\redist\x64\Microsoft.VC140.CRT\*" lib\x64\
GOTO END_OF_MSVC_DEPENDENCY

:VS2017_DEPEDENCY
copy /Y "%BAZEL_VC%\Redist\MSVC\14.15.26706\x64\Microsoft.VC141.CRT\*140.dll" lib\x64\

:END_OF_MSVC_DEPENDENCY

IF "%2%" == "gpu" GOTO DEPLOY_DEPENDENCY_GPU
GOTO END_OF_DEPLOY_DEPENDENCY_GPU
:DEPLOY_DEPENDENCY_GPU
SET CUDA_TOOLKIT_BIN_PATH=%CUDA_TOOLKIT_PATH%/bin
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cusolver64_*.dll" lib\x64\ 
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cublas64_*.dll" lib\x64\
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cudnn64_*.dll" lib\x64\
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cufft64_*.dll" lib\x64\
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\curand64_*.dll" lib\x64\
copy /Y "%CUDA_TOOLKIT_BIN_PATH:/=\%\cudart64_*.dll" lib\x64\
:END_OF_DEPLOY_DEPENDENCY_GPU

IF "%3%"=="dev" GOTO END_OF_CLEAN

:CLEAN
cd tensorflow
bazel clean
bazel shutdown
rm -rf c:\tmp\_bazel_canming
rm -rf c:\tmp\install\*
:END_OF_CLEAN

popd

Exit /B

TITLE=BAZEL_BUILD_TF
REM TASKKILL /FI "WINDOWTITLE eq BAZEL_BUILD_TF" /F /IM cmd.txt /T

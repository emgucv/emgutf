@echo on

pushd %~p0
cd ../..

SET BUILD_FOLDER=build_%1%
SET BUILD_TOOLS_FOLDER=C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools

SET CPU_FLAGS=
IF "%1%"=="32" GOTO ENV_x86
IF "%1%"=="64" GOTO ENV_x64
IF "%1%"=="ARM" GOTO ENV_ARM
IF "%1%"=="ARM64" GOTO ENV_ARM64

GOTO ENV_END

:ENV_x86
REM SET BUILD_FOLDER=%BUILD_FOLDER%_x86
ECHO "BUILDING 32bit solution in %BUILD_FOLDER%"
IF EXIST "%BUILD_TOOLS_FOLDER%\vc\Auxiliary\Build\vcvars32.bat" SET ENV_SETUP_SCRIPT=%BUILD_TOOLS_FOLDER%\vc\Auxiliary\Build\vcvars32.bat
SET CPU_FLAGS=--host_cpu=x64_windows --cpu=x64_x86_windows --copt=/arch:IA32 --linkopt=/MACHINE:x86 --compiler=msvc-cl
GOTO ENV_END

:ENV_x64
REM SET BUILD_FOLDER=%BUILD_FOLDER%_x64
ECHO "BUILDING 64bit solution in %BUILD_FOLDER%" 
IF EXIST "%BUILD_TOOLS_FOLDER%\vc\Auxiliary\Build\vcvars64.bat" SET ENV_SETUP_SCRIPT=%BUILD_TOOLS_FOLDER%\vc\Auxiliary\Build\vcvars64.bat
SET CPU_FLAGS=--config=win_clang 
GOTO ENV_END

:ENV_ARM
REM SET BUILD_FOLDER=%BUILD_FOLDER%_ARM
ECHO "BUILDING ARM solution in %BUILD_FOLDER%"
IF EXIST "%BUILD_TOOLS_FOLDER%\vc\Auxiliary\Build\vcvarsamd64_arm.bat" SET ENV_SETUP_SCRIPT=%BUILD_TOOLS_FOLDER%\vc\Auxiliary\Build\vcvarsamd64_arm.bat
SET CPU_FLAGS=--config=win_clang
GOTO ENV_END

:ENV_ARM64
REM SET BUILD_FOLDER=%BUILD_FOLDER%_ARM64
ECHO "BUILDING ARM64 solution in %BUILD_FOLDER%"
IF EXIST "%BUILD_TOOLS_FOLDER%\vc\Auxiliary\Build\vcvarsamd64_arm64.bat" SET ENV_SETUP_SCRIPT=%BUILD_TOOLS_FOLDER%\vc\Auxiliary\Build\vcvarsamd64_arm64.bat
SET CPU_FLAGS=--host_cpu=x64_windows --cpu=x64_arm64_windows --linkopt=/MACHINE:ARM64 --compiler=msvc-cl
:ENV_END
IF "%ENV_SETUP_SCRIPT%"=="" GOTO ENV_SETUP_END

call %ENV_SETUP_SCRIPT%

@echo on

:ENV_SETUP_END

SET BAZEL_XNN_FLAGS=--define tflite_with_xnnpack=false
IF NOT "%2%"=="xnn" GOTO END_OF_XNN
SET BAZEL_XNN_FLAGS=--define tflite_with_xnnpack=true
:END_OF_XNN

IF NOT "%3%"=="docker" GOTO ENV_NOT_DOCKER

:ENV_DOCKER
SET DOCKER_FLAGS=--define=EXECUTOR=remote --experimental_docker_verbose --experimental_enable_docker_sandbox
SET OUTPUT_USER_ROOT_DIR=c:\bazel_output_user_root
SET OUTPUT_BASE_DIR=c:\bazel_output_base
GOTO END_OF_DOCKER

:ENV_NOT_DOCKER
SET OUTPUT_USER_ROOT_DIR=%~dp0output_user_root
SET OUTPUT_BASE_DIR=%~dp0output_base

:END_OF_DOCKER
IF NOT EXIST %OUTPUT_USER_ROOT_DIR% mkdir %OUTPUT_USER_ROOT_DIR%
IF NOT EXIST %OUTPUT_BASE_DIR% mkdir %OUTPUT_BASE_DIR%

REM IF NOT "%3%"=="clean" GOTO END_OF_CLEAN
REM rm -rf %USERPROFILE%\_bazel_%USERNAME%
REM :END_OF_CLEAN

SET PROGRAMFILES_DIR_X86=%programfiles(x86)%
if NOT EXIST "%PROGRAMFILES_DIR_X86%" SET PROGRAMFILES_DIR_X86=%programfiles%
SET PROGRAMFILES_DIR=%programfiles%

REM Find Visual Studio or Msbuild
FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [15.0^,16.0^) -property installationPath`) DO SET VS2017_DIR=%%F
SET VS2017=%VS2017_DIR%\Common7\IDE\devenv.com

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [16.0^,17.0^) -property installationPath`) DO SET VS2019_DIR=%%F
SET VS2019=%VS2019_DIR%\Common7\IDE\devenv.com

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [17.0^,18.0^) -property installationPath`) DO SET VS2022_DIR=%%F
SET VS2022=%VS2022_DIR%\Common7\IDE\devenv.com

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [18.0^,19.0^) -property installationPath`) DO SET VS2026_DIR=%%F
SET VS2026=%VS2026_DIR%\Common7\IDE\devenv.com

IF EXIST "%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe" SET MSBUILD35=%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe
IF EXIST "%windir%\Microsoft.NET\Framework64\v3.5\MSBuild.exe" SET MSBUILD35=%windir%\Microsoft.NET\Framework64\v3.5\MSBuild.exe
IF EXIST "%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" SET MSBUILD40=%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

IF EXIST "%MSBUILD35%" SET DEVENV=%MSBUILD35%
IF EXIST "%MSBUILD40%" SET DEVENV=%MSBUILD40%
IF EXIST "%VS2017%" SET DEVENV=%VS2017%
IF EXIST "%VS2019%" SET DEVENV=%VS2019%
IF EXIST "%VS2022%" SET DEVENV=%VS2022%
IF EXIST "%VS2026%" SET DEVENV=%VS2026%
IF EXIST "%BUILD_TOOLS_FOLDER%" SET DEVENV=%BUILD_TOOLS_FOLDER%

rem Get full path to the running Python executable
for /f "usebackq delims=" %%A in (`python -c "import sys; print(sys.executable)"`) do set "PYEXE=%%~A"
rem Extract just the drive+path (ends with a backslash)
for %%A in ("%PYEXE%") do set "PYTHON_BASE_PATH=%%~dpA"
rem (Optional) remove trailing backslash
if "%PYTHON_BASE_PATH:~-1%"=="\" set "PYTHON_BASE_PATH=%PYTHON_BASE_PATH:~0,-1%"

IF EXIST "%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\Shared\Python37_64" SET PYTHON_BASE_PATH=%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\Shared\Python37_64
IF EXIST "C:\Python312" SET PYTHON_BASE_PATH=C:\Python312
IF EXIST "C:\Python312" SET HERMETIC_PYTHON_VERSION=3.12
IF EXIST "C:\python-virt\python312" SET PYTHON_BASE_PATH=C:\python-virt\python312
IF EXIST "C:\python-virt\python312" SET HERMETIC_PYTHON_VERSION=3.12

ECHO PYTHON_BASE_PATH=%PYTHON_BASE_PATH%

SET PYTHON_BIN_PATH=%PYTHON_BASE_PATH%\python.exe
SET PYTHON_LIB_PATH=%PYTHON_BASE_PATH%\lib\site-packages

SET PYTHON_BASE_PATH=%PYTHON_BASE_PATH:\=/%
SET PYTHON_BIN_PATH=%PYTHON_BIN_PATH:\=/%
SET PYTHON_LIB_PATH=%PYTHON_LIB_PATH:\=/%

:SET_BAZEL_VS_VC
IF "%DEVENV%"=="%VS2017%" SET BAZEL_VS=%VS2017:\Common7\IDE\devenv.com=%
IF "%DEVENV%"=="%VS2019%" SET BAZEL_VS=%VS2019:\Common7\IDE\devenv.com=%
IF "%DEVENV%"=="%VS2022%" SET BAZEL_VS=%VS2022:\Common7\IDE\devenv.com=%
IF "%DEVENV%"=="%VS2026%" SET BAZEL_VS=%VS2026:\Common7\IDE\devenv.com=%
IF "%DEVENV%"=="%BUILD_TOOLS_FOLDER%" SET BAZEL_VS=%BUILD_TOOLS_FOLDER%
IF NOT "%BAZEL_VS%"=="" SET BAZEL_VC=%BAZEL_VS%\VC
REM SET BAZEL_VS="%BAZEL_VS%"
ECHO Using BAZEL_VC=%BAZEL_VC%
IF EXIST "C:\Program Files\LLVM\bin" SET BAZEL_LLVM="C:\Program Files\LLVM"
IF NOT "%BAZEL_VC%"=="" SET BAZEL_LLVM=%BAZEL_VC%\Tools\Llvm\x64
ECHO Using BAZEL_LLVM=%BAZEL_LLVM%

cd tensorflow

SET BAZEL_COMMAND=bazel.exe
SET MSYS_PATH=C:\msys64
SET MSYS_BIN=%MSYS_PATH%\usr\bin
IF EXIST "%MSYS_BIN%\bazel.exe" SET BAZEL_COMMAND=%MSYS_BIN%\bazel.exe

call %BAZEL_COMMAND% --output_base=%OUTPUT_BASE_DIR% --output_user_root=%OUTPUT_USER_ROOT_DIR% build --repo_env=BAZEL_LLVM="%BAZEL_LLVM%"  %CPU_FLAGS% %BAZEL_XNN_FLAGS% %DOCKER_FLAGS% -c opt //tensorflow/lite:version --verbose_failures

REM Patch pthreadpool bazel build script
cp ../platforms/windows/pthreadpool.BUILD.bazel ../platforms/windows/output_base/external/pthreadpool/BUILD.bazel

call %BAZEL_COMMAND% --output_base=%OUTPUT_BASE_DIR% --output_user_root=%OUTPUT_USER_ROOT_DIR% build --repo_env=BAZEL_LLVM="%BAZEL_LLVM%"  %CPU_FLAGS% %BAZEL_XNN_FLAGS% %DOCKER_FLAGS% -c opt //tensorflow/tfliteextern:libtfliteextern.so --verbose_failures
REM call %BAZEL_COMMAND% --output_base=%OUTPUT_BASE_DIR% --output_user_root=%OUTPUT_USER_ROOT_DIR% build --config=win_clang %BAZEL_XNN_FLAGS% %DOCKER_FLAGS% -c opt //tensorflow/lite/c:c_api //tensorflow/tfliteextern:libtfliteextern.so --verbose_failures
REM call %BAZEL_COMMAND% --output_base=%OUTPUT_BASE_DIR% --output_user_root=%OUTPUT_USER_ROOT_DIR% build  --copt="-O2" --cxxopt="-O2" %BAZEL_XNN_FLAGS% %DOCKER_FLAGS% -c opt //tensorflow/tfliteextern:libtfliteextern.so --verbose_failures
REM call %BAZEL_COMMAND% --output_base=%OUTPUT_BASE_DIR% --output_user_root=%OUTPUT_USER_ROOT_DIR% build  --copt="-O2" --cxxopt="-O2" --conlyopt=/std:c11 --conlyopt=/experimental:c11atomics %BAZEL_XNN_FLAGS% %DOCKER_FLAGS% -c opt //tensorflow/tfliteextern:libtfliteextern.so --verbose_failures
      
cd ..

IF NOT EXIST lib\runtimes\win-x64\native mkdir lib\runtimes\win-x64\native
copy /Y "tensorflow\bazel-bin\tensorflow\tfliteextern\libtfliteextern.so" lib\runtimes\win-x64\native\tfliteextern.dll

:START_OF_MSVC_DEPENDENCY
IF "%BAZEL_VC%"=="" GOTO END_OF_MSVC_DEPENDENCY
IF "%DEVENV%"=="%VS2017%" GOTO VS2017_DEPENDENCY
IF "%DEVENV%"=="%VS2019%" GOTO VS2019_DEPENDENCY
IF "%DEVENV%"=="%VS2022%" GOTO VS2022_DEPENDENCY
IF "%DEVENV%"=="%VS2026%" GOTO VS2026_DEPENDENCY
IF "%DEVENV%"=="%BUILDTOOLS%" GOTO VS2019_DEPENDENCY
GOTO END_OF_MSVC_DEPENDENCY

:VS2017_DEPENDENCY
for /d %%i in ( "%BAZEL_VC%\Redist\MSVC\*" ) do SET VS2017_REDIST=%%i\x64\Microsoft.VC141.CRT
copy /Y "%VS2017_REDIST%\*140.dll" lib\runtimes\win-x64\native\
copy /Y "%VS2017_REDIST%\*140_1.dll" lib\runtimes\win-x64\native\
copy /Y "%VS2017_REDIST%\*140_2.dll" lib\runtimes\win-x64\native\
REM rm lib\runtimes\win-x64\native\vccorlib140.dll
GOTO END_OF_MSVC_DEPENDENCY

:VS2019_DEPENDENCY
for /d %%i in ( "%BAZEL_VC%\Redist\MSVC\14*" ) do SET VS2019_REDIST=%%i\x64\Microsoft.VC142.CRT
copy /Y "%VS2019_REDIST%\*.dll" lib\runtimes\win-x64\native\
REM copy /Y "%VS2019_REDIST%\*140_1.dll" lib\runtimes\win-x64\native\
REM copy /Y "%VS2019_REDIST%\*140_2.dll" lib\runtimes\win-x64\native\
REM rm lib\runtimes\win-x64\native\vccorlib140.dll
GOTO END_OF_MSVC_DEPENDENCY

:VS2022_DEPENDENCY
for /d %%i in ( "%BAZEL_VC%\Redist\MSVC\14*" ) do SET VS2022_REDIST=%%i\x64\Microsoft.VC143.CRT
copy /Y "%VS2022_REDIST%\*.dll" lib\runtimes\win-x64\native\

:VS2026_DEPENDENCY
for /d %%i in ( "%BAZEL_VC%\Redist\MSVC\14*" ) do SET VS2026_REDIST=%%i\x64\Microsoft.VC145.CRT
copy /Y "%VS2026_REDIST%\*.dll" lib\runtimes\win-x64\native\

:END_OF_MSVC_DEPENDENCY
popd
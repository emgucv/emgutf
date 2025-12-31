REM @echo off
@echo on

REM POSSIBLE OPTIONS: 
REM %1%: "doc", build documentation
REM %2%: "nuget", build nuget packages
REM %3%: "package", build zip packages

setlocal

pushd %~p0

cd ..\..

IF EXIST "lib\runtimes\win-x86\native\tfliteextern.dll" SET HAS_TF_LITE=Y
IF EXIST "lib\runtimes\win-x64\native\tfliteextern.dll" SET HAS_TF_LITE=Y
IF EXIST "lib\runtimes\win-x64\native\tfextern.dll" SET HAS_TF_FULL=Y

REM If we're a 32-bit process on 64-bit Windows, PROCESSOR_ARCHITEW6432 is set.
set "arch=%PROCESSOR_ARCHITECTURE%"
if defined PROCESSOR_ARCHITEW6432 set "arch=%PROCESSOR_ARCHITEW6432%"

IF /I "%arch%"=="AMD64" ECHO "Generating 64bit solution" 
IF /I "%arch%"=="ARM" ECHO "Generating ARM solution"
IF /I "%arch%"=="x86" ECHO "Generating 32bit solution"
IF /I "%arch%"=="ARM64" ECHO "Generating ARM64 solution"

SET OS_MODE=
IF /I "%arch%"=="AMD64" SET OS_MODE= Win64
IF /I "%arch%"=="ARM" SET OS_MODE= ARM

SET BUILD_ARCH=
IF /I "%arch%"=="AMD64" SET BUILD_ARCH=-A x64
IF /I "%arch%"=="x86" SET BUILD_ARCH=-A Win32
IF /I "%arch%"=="ARM" SET BUILD_ARCH=-A ARM
IF /I "%arch%"=="ARM64" SET BUILD_ARCH=-A ARM64

SET PROGRAMFILES_DIR_X86=%programfiles(x86)%
if NOT EXIST "%PROGRAMFILES_DIR_X86%" SET PROGRAMFILES_DIR_X86=%programfiles%
SET PROGRAMFILES_DIR=%programfiles%

SET BUILD_TOOLS_2019_FOLDER=C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools

REM Find Visual Studio or Msbuild
FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [15.0^,16.0^) -property installationPath`) DO SET VS2017_DIR=%%F
SET VS2017="%VS2017_DIR%\Common7\IDE\devenv.com" 

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [16.0^,17.0^) -property installationPath`) DO SET VS2019_DIR=%%F
SET VS2019="%VS2019_DIR%\Common7\IDE\devenv.com"

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [17.0^,18.0^) -property installationPath`) DO SET VS2022_DIR=%%F
SET VS2022="%VS2022_DIR%\Common7\IDE\devenv.com"

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [18.0^,19.0^) -property installationPath`) DO SET VS2026_DIR=%%F
SET VS2026="%VS2026_DIR%\Common7\IDE\devenv.com"

IF EXIST "%BUILD_TOOLS_2019_FOLDER%\MSBuild\Current\Bin\MSBuild.exe" SET MSBUILD_BUILDTOOLS_2019="%BUILD_TOOLS_2019_FOLDER%\MSBuild\Current\Bin\MSBuild.exe"

IF EXIST %VS2017% SET DEVENV=%VS2017%
IF EXIST %VS2019% SET DEVENV=%VS2019%
IF EXIST %MSBUILD_BUILDTOOLS_2019% SET DEVENV=%MSBUILD_BUILDTOOLS_2019%
IF EXIST %VS2022% SET DEVENV=%VS2022%
IF EXIST %VS2026% SET DEVENV=%VS2026%

REM Find CMake  
SET CMAKE="cmake.exe"
IF EXIST "%PROGRAMFILES_DIR_X86%\CMake\bin\cmake.exe" SET CMAKE="%PROGRAMFILES_DIR_X86%\CMake\bin\cmake.exe"
IF EXIST "%PROGRAMFILES_DIR%\CMake\bin\cmake.exe" SET CMAKE="%PROGRAMFILES_DIR%\CMake\bin\cmake.exe"
IF EXIST "%PROGRAMW6432%\CMake\bin\cmake.exe" SET CMAKE="%PROGRAMW6432%\CMake\bin\cmake.exe"
IF EXIST "%VS2022_DIR%\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe" SET CMAKE="%VS2022_DIR%\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
IF EXIST "%VS2026_DIR%\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe" SET CMAKE="%VS2026_DIR%\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"


:SET_BUILD_TYPE

IF %DEVENV%==%VS2017% SET BUILD_TYPE=/Build Release
IF %DEVENV%==%VS2019% SET BUILD_TYPE=/Build Release
IF %DEVENV%==%VS2022% SET BUILD_TYPE=/Build Release
IF %DEVENV%==%MSBUILD_BUILDTOOLS_2019% SET BUILD_TYPE=/property:Configuration=Release

IF %DEVENV%==%VS2017% SET CMAKE_CONF="Visual Studio 15%OS_MODE%"

IF %DEVENV%==%VS2019% SET CMAKE_CONF="Visual Studio 16" %BUILD_ARCH%
IF %DEVENV%==%MSBUILD_BUILDTOOLS_2019% SET CMAKE_CONF="Visual Studio 16" %BUILD_ARCH%

IF %DEVENV%==%VS2022% SET CMAKE_CONF="Visual Studio 17" %BUILD_ARCH%

REM build EMGU TF

IF "%1%"=="doc" ^
SET CMAKE_CONF_FLAGS=%CMAKE_CONF_FLAGS% -DEMGU_TF_DOCUMENTATION_BUILD:BOOL=TRUE 

IF NOT EXIST b mkdir b
IF NOT EXIST package mkdir package
cd b

%CMAKE% .. ^
-G %CMAKE_CONF% ^
%CMAKE_CONF_FLAGS% 

SET CMAKE_BUILD_TARGET=ALL_BUILD
IF NOT "%3%"=="package" GOTO CHECK_BUILD_TYPE
SET CMAKE_BUILD_TARGET=%CMAKE_BUILD_TARGET% PACKAGE
SET MOVE_ZIP_SCRIPT=copy *.zip ..\package
SET MOVE_EXE_SCRIPT=copy *.exe ..\package

:CHECK_BUILD_TYPE
REM IF "%TF_TYPE%"=="LITE" goto BUILD_TF_LITE

:BUILD_TF_FULL
IF NOT "%1%"=="doc" GOTO BUILD_TF_FULL_NUGET
IF "%HAS_TF_FULL%"=="Y" SET CMAKE_BUILD_TARGET=%CMAKE_BUILD_TARGET% Emgu.TF.Document.Html

:BUILD_TF_FULL_NUGET
IF NOT "%2%"=="nuget" GOTO BUILD_TF_LITE
IF "%HAS_TF_FULL%"=="Y" SET CMAKE_BUILD_TARGET=%CMAKE_BUILD_TARGET% Emgu.TF.runtime.windows.nuget
IF "%HAS_TF_FULL%"=="Y" SET MOVE_NUGET_SCRIPT=copy ..\platforms\nuget\*.nupkg ..\package
REM GOTO BUILD

:BUILD_TF_LITE
IF NOT "%1%"=="doc" GOTO BUILD_TF_LITE_NUGET
IF "%HAS_TF_LITE%"=="Y" SET CMAKE_BUILD_TARGET=%CMAKE_BUILD_TARGET% Emgu.TF.Lite.Document.Html 
IF "%HAS_TF_LITE%"=="Y" SET ZIP_HELP_SCRIPT=zip package\Help.zip -r Help

:BUILD_TF_LITE_NUGET
IF NOT "%2%"=="nuget" GOTO BUILD
IF "%HAS_TF_LITE%"=="Y" SET CMAKE_BUILD_TARGET=%CMAKE_BUILD_TARGET% Emgu.TF.Lite.runtime.windows.nuget
IF "%HAS_TF_LITE%"=="Y" SET MOVE_NUGET_SCRIPT=copy platforms\nuget\*.nupkg package

:BUILD
ECHO BUILDING TARGETS: %CMAKE_BUILD_TARGET%
%CMAKE% --build . --config Release --target %CMAKE_BUILD_TARGET%
%MOVE_ZIP_SCRIPT%
%MOVE_EXE_SCRIPT%
cd ..
%MOVE_NUGET_SCRIPT%
%ZIP_HELP_SCRIPT%


:END_OF_SCRIPT
popd
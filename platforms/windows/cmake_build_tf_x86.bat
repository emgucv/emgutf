REM @echo off
pushd %~p0
cd ..\..

IF "%1%"=="64" ECHO "BUILDING 64bit solution" 
IF "%1%"=="ARM" ECHO "BUILDING ARM solution"
IF "%1%"=="32" ECHO "BUILDING 32bit solution"

SET OS_MODE=
IF "%1%"=="64" SET OS_MODE= Win64
IF "%1%"=="ARM" SET OS_MODE= ARM

IF "%2%"=="gpu" SET GPU_MODE= -Dtensorflow_ENABLE_GPU=ON ^
-DCUDNN_HOME="%CUDA_PATH_V9_0:\=/%" ^
-DCUDA_TOOLKIT_ROOT_DIR="%CUDA_PATH_V9_0:\=/%" 

SET PROJECT=tfextern

SET MSBUILD_MULTIPROCESS=/m
IF "%3%"=="" GOTO END_SETTING_COMPILE_PROCESS_COUNT
SET COMPILE_PROCESS_COUNT=%3%
SET MSBUILD_MULTIPROCESS=/m:%3%

:END_SETTING_COMPILE_PROCESS_COUNT

SET PROGRAMFILES_DIR_X86=%programfiles(x86)%
if NOT EXIST "%PROGRAMFILES_DIR_X86%" SET PROGRAMFILES_DIR_X86=%programfiles%
SET PROGRAMFILES_DIR=%programfiles%

REM Find CMake  
SET CMAKE="cmake.exe"
IF EXIST "%PROGRAMFILES_DIR_X86%\CMake 2.8\bin\cmake.exe" SET CMAKE="%PROGRAMFILES_DIR_X86%\CMake 2.8\bin\cmake.exe"
IF EXIST "%PROGRAMFILES_DIR_X86%\CMake\bin\cmake.exe" SET CMAKE="%PROGRAMFILES_DIR_X86%\CMake\bin\cmake.exe"
IF EXIST "%PROGRAMFILES_DIR%\CMake\bin\cmake.exe" SET CMAKE="%PROGRAMFILES_DIR%\CMake\bin\cmake.exe"
IF EXIST "%PROGRAMW6432%\CMake\bin\cmake.exe" SET CMAKE="%PROGRAMW6432%\CMake\bin\cmake.exe"

REM Find Python Executable
SET PYTHON_EXECUTABLE="python.exe"
IF EXIST "%PROGRAMFILES_DIR_X86%\Anaconda2\python.exe" SET PYTHON_EXECUTABLE=%PROGRAMFILES_DIR_X86%\Anaconda2\python.exe
IF EXIST "%PROGRAMFILES_DIR_X86%\Anaconda3\python.exe" SET PYTHON_EXECUTABLE=%PROGRAMFILES_DIR_X86%\Anaconda3\python.exe
IF EXIST "%PROGRAMFILES_DIR%\Anaconda2\python.exe" SET PYTHON_EXECUTABLE=%PROGRAMFILES_DIR%\Anaconda2\python.exe
IF EXIST "%PROGRAMFILES_DIR%\Anaconda3\python.exe" SET PYTHON_EXECUTABLE=%PROGRAMFILES_DIR%\Anaconda3\python.exe
IF EXIST "%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\Shared\Anaconda3_64\python.exe" SET PYTHON_EXECUTABLE=%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\Shared\Anaconda3_64\python.exe

REM Find Visual Studio or Msbuild
SET VS2005="%VS80COMNTOOLS%..\IDE\devenv.com"
SET VS2008="%VS90COMNTOOLS%..\IDE\devenv.com"
SET VS2010="%VS100COMNTOOLS%..\IDE\devenv.com"
SET VS2012="%VS110COMNTOOLS%..\IDE\devenv.com"
SET VS2013="%VS120COMNTOOLS%..\IDE\devenv.com"
SET VS2015="%VS140COMNTOOLS%..\IDE\devenv.com"

SET VS2017_DIR=%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\2017\Community
IF EXIST "%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com" SET VS2017_DIR=%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\2017\Professional
IF EXIST "%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.com" SET VS2017_DIR=%PROGRAMFILES_DIR_X86%\Microsoft Visual Studio\2017\Enterprise
IF EXIST "%VS2017INSTALLDIR%\Common7\IDE\devenv.com" SET VS2017_DIR=%VS2017INSTALLDIR%
IF EXIST "%VS150COMNTOOLS%..\IDE\devenv.com" SET VS2017_DIR =%VS150COMNTOOLS%..\..
SET VS2017="%VS2017_DIR%\Common7\IDE\devenv.com" 


IF EXIST "%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe" SET MSBUILD35=%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe
IF EXIST "%windir%\Microsoft.NET\Framework64\v3.5\MSBuild.exe" SET MSBUILD35=%windir%\Microsoft.NET\Framework64\v3.5\MSBuild.exe
IF EXIST "%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" SET MSBUILD40=%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
IF EXIST "%PROGRAMFILES_DIR_X86%\MSBuild\14.0\bin\MSBuild.exe" SET MSBUILD140=%PROGRAMFILES_DIR_X86%\MSBuild\14.0\bin\MSBuild.exe
IF EXIST "%VS2017_DIR%\MSBuild\15.0\Bin\MSBuild.exe" SET MSBUILD150=%VS2017_DIR%\MSBuild\15.0\Bin\MSBuild.exe

IF EXIST "%MSBUILD35%" SET DEVENV="%MSBUILD35%"
IF EXIST "%MSBUILD40%" SET DEVENV="%MSBUILD40%"
IF EXIST "%MSBUILD140%" SET DEVENV="%MSBUILD140%"
REM IF EXIST "%MSBUILD150%" SET DEVENV="%MSBUILD150%"

REM if compile process count is set we cannot use devenv
IF NOT ("%COMPILE_PROCESS_COUNT%" == "") GOTO SET_BUILD_TYPE

IF EXIST %VS2005% SET DEVENV=%VS2005% 
IF EXIST %VS2008% SET DEVENV=%VS2008%
IF EXIST %VS2010% SET DEVENV=%VS2010%
IF EXIST %VS2012% SET DEVENV=%VS2012%
IF EXIST %VS2013% SET DEVENV=%VS2013%
IF EXIST %VS2015% SET DEVENV=%VS2015%
REM IF EXIST %VS2017% SET DEVENV=%VS2017%
REM IF "%2%"=="gpu" GOTO SET_BUILD_TYPE
REM IF NOT "%3%"=="WindowsStore10" GOTO SET_BUILD_TYPE



:SET_BUILD_TYPE
IF %DEVENV%=="%MSBUILD35%" SET BUILD_TYPE=/property:Configuration=Release /p:PreferredToolArchitecture=x64 %MSBUILD_MULTIPROCESS% /t:%PROJECT% 
IF %DEVENV%=="%MSBUILD40%" SET BUILD_TYPE=/property:Configuration=Release /p:PreferredToolArchitecture=x64 %MSBUILD_MULTIPROCESS% /t:%PROJECT% 
IF %DEVENV%=="%MSBUILD140%" SET BUILD_TYPE=/property:Configuration=Release /p:PreferredToolArchitecture=x64 %MSBUILD_MULTIPROCESS% /t:%PROJECT% 
IF %DEVENV%=="%MSBUILD150%" SET BUILD_TYPE=/property:Configuration=Release /p:PreferredToolArchitecture=x64 %MSBUILD_MULTIPROCESS% /t:%PROJECT% 
IF %DEVENV%==%VS2005% SET BUILD_TYPE=/Build Release /project %PROJECT%
IF %DEVENV%==%VS2008% SET BUILD_TYPE=/Build Release /project %PROJECT%
IF %DEVENV%==%VS2010% SET BUILD_TYPE=/Build Release /project %PROJECT%
IF %DEVENV%==%VS2012% SET BUILD_TYPE=/Build Release /project %PROJECT%
IF %DEVENV%==%VS2013% SET BUILD_TYPE=/Build Release /project %PROJECT%
IF %DEVENV%==%VS2015% SET BUILD_TYPE=/Build Release /project %PROJECT%
IF %DEVENV%==%VS2017% SET BUILD_TYPE=/Build Release /project %PROJECT%

:SET_CLEAN_TYPE
IF %DEVENV%=="%MSBUILD35%" SET CLEAN_TYPE=/property:Configuration=Release /t:clean %MSBUILD_MULTIPROCESS%
IF %DEVENV%=="%MSBUILD40%" SET CLEAN_TYPE=/property:Configuration=Release /t:clean %MSBUILD_MULTIPROCESS%
IF %DEVENV%=="%MSBUILD140%" SET CLEAN_TYPE=/property:Configuration=Release /t:clean %MSBUILD_MULTIPROCESS%
IF %DEVENV%=="%MSBUILD150%" SET CLEAN_TYPE=/property:Configuration=Release /t:clean %MSBUILD_MULTIPROCESS%
IF %DEVENV%==%VS2005% SET CLEAN_TYPE=/Clean Release
IF %DEVENV%==%VS2008% SET CLEAN_TYPE=/Clean Release
IF %DEVENV%==%VS2010% SET CLEAN_TYPE=/Clean Release
IF %DEVENV%==%VS2012% SET CLEAN_TYPE=/Clean Release
IF %DEVENV%==%VS2013% SET CLEAN_TYPE=/Clean Release
IF %DEVENV%==%VS2015% SET CLEAN_TYPE=/Clean Release
IF %DEVENV%==%VS2017% SET CLEAN_TYPE=/Clean Release

IF %DEVENV%=="%MSBUILD35%" SET CMAKE_CONF="Visual Studio 12%OS_MODE%"
IF %DEVENV%=="%MSBUILD40%" SET CMAKE_CONF="Visual Studio 12%OS_MODE%"
IF %DEVENV%=="%MSBUILD140%" SET CMAKE_CONF="Visual Studio 14%OS_MODE%"
IF %DEVENV%=="%MSBUILD150%" SET CMAKE_CONF="Visual Studio 15%OS_MODE%"
IF %DEVENV%==%VS2005% SET CMAKE_CONF="Visual Studio 8 2005%OS_MODE%"
IF %DEVENV%==%VS2008% SET CMAKE_CONF="Visual Studio 9 2008%OS_MODE%"
IF %DEVENV%==%VS2010% SET CMAKE_CONF="Visual Studio 10%OS_MODE%"
IF %DEVENV%==%VS2012% SET CMAKE_CONF="Visual Studio 11%OS_MODE%"
IF %DEVENV%==%VS2013% SET CMAKE_CONF="Visual Studio 12%OS_MODE%"
IF %DEVENV%==%VS2015% SET CMAKE_CONF="Visual Studio 14%OS_MODE%"
IF %DEVENV%==%VS2017% SET CMAKE_CONF="Visual Studio 15%OS_MODE%"

call tensorflow\tensorflow\contrib\cmake\make.bat

cd tensorflow\tensorflow\contrib\cmake

mkdir build
cd build
%CMAKE% .. ^
-DCMAKE_BUILD_TYPE=Release ^
-G %CMAKE_CONF% %GPU_MODE%^
-DPYTHON_EXECUTABLE="%PYTHON_EXECUTABLE%" ^
-Dtensorflow_BUILD_PYTHON_BINDINGS:BOOL=OFF

REM download and build protobuf
REM call %DEVENV% %BUILD_TYPE% tensorflow.sln /project protobuf
REM Fix protobuf 64MB limit
REM sed -i 's/kDefaultTotalBytesLimit = 64/kDefaultTotalBytesLimit = 500/g' .\protobuf\src\protobuf\src\google\protobuf\io\coded_stream.h
REM need to clean the project to for the rebuild of protobuf
REM call %DEVENV% %CLEAN_TYPE% tensorflow.sln /project protobuf
REM build tfextern
call %DEVENV% %BUILD_TYPE% tensorflow.sln 

cd ..\..\..\..\..


popd
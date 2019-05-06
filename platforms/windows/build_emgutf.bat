REM @echo off
pushd %~p0
cd ..\..

SET TF_TYPE=FULL
IF "%1%"=="lite" SET TF_TYPE=LITE

IF "%2%"=="64" ECHO "BUILDING 64bit solution" 
IF "%2%"=="ARM" ECHO "BUILDING ARM solution"
IF "%2%"=="32" ECHO "BUILDING 32bit solution"

SET OS_MODE=
IF "%2%"=="64" SET OS_MODE= Win64
IF "%2%"=="ARM" SET OS_MODE= ARM

SET PROGRAMFILES_DIR_X86=%programfiles(x86)%
if NOT EXIST "%PROGRAMFILES_DIR_X86%" SET PROGRAMFILES_DIR_X86=%programfiles%
SET PROGRAMFILES_DIR=%programfiles%

REM Find CMake  
SET CMAKE="cmake.exe"
IF EXIST "%PROGRAMFILES_DIR_X86%\CMake 2.8\bin\cmake.exe" SET CMAKE="%PROGRAMFILES_DIR_X86%\CMake 2.8\bin\cmake.exe"
IF EXIST "%PROGRAMFILES_DIR_X86%\CMake\bin\cmake.exe" SET CMAKE="%PROGRAMFILES_DIR_X86%\CMake\bin\cmake.exe"
IF EXIST "%PROGRAMFILES_DIR%\CMake\bin\cmake.exe" SET CMAKE="%PROGRAMFILES_DIR%\CMake\bin\cmake.exe"
IF EXIST "%PROGRAMW6432%\CMake\bin\cmake.exe" SET CMAKE="%PROGRAMW6432%\CMake\bin\cmake.exe"

REM Find Visual Studio or Msbuild
SET VS2005="%VS80COMNTOOLS%..\IDE\devenv.com"
SET VS2008="%VS90COMNTOOLS%..\IDE\devenv.com"
SET VS2010="%VS100COMNTOOLS%..\IDE\devenv.com"
SET VS2012="%VS110COMNTOOLS%..\IDE\devenv.com"
SET VS2013="%VS120COMNTOOLS%..\IDE\devenv.com"
SET VS2015="%VS140COMNTOOLS%..\IDE\devenv.com"

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [15.0^,16.0^) -property installationPath`) DO SET VS2017_DIR=%%F
SET VS2017="%VS2017_DIR%\Common7\IDE\devenv.com" 

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [16.0^,17.0^) -property installationPath`) DO SET VS2019_DIR=%%F
SET VS2019="%VS2019_DIR%\Common7\IDE\devenv.com"

IF EXIST "%MSBUILD35%" SET DEVENV="%MSBUILD35%"
IF EXIST "%MSBUILD40%" SET DEVENV="%MSBUILD40%"
IF EXIST %VS2005% SET DEVENV=%VS2005% 
IF EXIST %VS2008% SET DEVENV=%VS2008%
IF EXIST %VS2010% SET DEVENV=%VS2010%
IF EXIST %VS2012% SET DEVENV=%VS2012%

IF EXIST %VS2013% SET DEVENV=%VS2013%
IF EXIST %VS2015% SET DEVENV=%VS2015%
IF EXIST %VS2017% SET DEVENV=%VS2017%
IF EXIST %VS2019% SET DEVENV=%VS2019%


:SET_BUILD_TYPE
IF %DEVENV%=="%MSBUILD35%" SET BUILD_TYPE=/property:Configuration=Release
IF %DEVENV%=="%MSBUILD40%" SET BUILD_TYPE=/property:Configuration=Release
IF %DEVENV%==%VS2005% SET BUILD_TYPE=/Build Release
IF %DEVENV%==%VS2008% SET BUILD_TYPE=/Build Release
IF %DEVENV%==%VS2010% SET BUILD_TYPE=/Build Release
IF %DEVENV%==%VS2012% SET BUILD_TYPE=/Build Release
IF %DEVENV%==%VS2013% SET BUILD_TYPE=/Build Release
IF %DEVENV%==%VS2015% SET BUILD_TYPE=/Build Release
IF %DEVENV%==%VS2017% SET BUILD_TYPE=/Build Release
IF %DEVENV%==%VS2019% SET BUILD_TYPE=/Build Release

IF %DEVENV%=="%MSBUILD35%" SET CMAKE_CONF="Visual Studio 12 2005%OS_MODE%"
IF %DEVENV%=="%MSBUILD40%" SET CMAKE_CONF="Visual Studio 12 2005%OS_MODE%"
IF %DEVENV%==%VS2005% SET CMAKE_CONF="Visual Studio 8 2005%OS_MODE%"
IF %DEVENV%==%VS2008% SET CMAKE_CONF="Visual Studio 9 2008%OS_MODE%"
IF %DEVENV%==%VS2010% SET CMAKE_CONF="Visual Studio 10%OS_MODE%"
IF %DEVENV%==%VS2012% SET CMAKE_CONF="Visual Studio 11%OS_MODE%"
IF %DEVENV%==%VS2013% SET CMAKE_CONF="Visual Studio 12%OS_MODE%"
IF %DEVENV%==%VS2015% SET CMAKE_CONF="Visual Studio 14%OS_MODE%"
IF %DEVENV%==%VS2017% SET CMAKE_CONF="Visual Studio 15%OS_MODE%"
IF %DEVENV%==%VS2019% IF "%2%"=="64" SET CMAKE_CONF="Visual Studio 16" -A x64
IF %DEVENV%==%VS2019% IF "%2%"=="32" SET CMAKE_CONF="Visual Studio 16" -A Win32

REM build EMGU TF
SET BUILD_PROJECT=
IF "%4%"=="package" SET BUILD_PROJECT= /project PACKAGE 

IF "%3%"=="doc" ^
SET CMAKE_CONF_FLAGS=%CMAKE_CONF_FLAGS% -DEMGU_TF_DOCUMENTATION_BUILD:BOOL=TRUE 
IF "%3%"=="htmldoc" ^
SET CMAKE_CONF_FLAGS=%CMAKE_CONF_FLAGS% -DEMGU_TF_DOCUMENTATION_BUILD:BOOL=TRUE 
%CMAKE% . ^
-G %CMAKE_CONF% ^
%CMAKE_CONF_FLAGS% 

IF "%TF_TYPE%"=="LITE" goto BUILD_TF_LITE

:BUILD_TF_FULL
call %DEVENV% %BUILD_TYPE% emgutf.sln %BUILD_PROJECT% 
IF "%3%"=="htmldoc" ^
call %DEVENV% %BUILD_TYPE% emgutf.sln /project Emgu.TF.Document.Html 

IF "%5%"=="nuget" ^
call %DEVENV% %BUILD_TYPE% emgutf.sln /project Emgu.TF.Models.nuget 
IF "%5%"=="nuget" ^
call %DEVENV% %BUILD_TYPE% emgutf.sln /project Emgu.TF.Protobuf.nuget 
goto END_OF_SCRIPT

:BUILD_TF_LITE
call %DEVENV% %BUILD_TYPE% emgutf.sln %BUILD_PROJECT% 
IF "%3%"=="htmldoc" ^
call %DEVENV% %BUILD_TYPE% emgutf.sln /project Emgu.TF.Lite.Document.Html 

IF "%5%"=="nuget" ^
call %DEVENV% %BUILD_TYPE% emgutf.sln /project Emgu.TF.Lite.nuget 
IF "%5%"=="nuget" ^
call %DEVENV% %BUILD_TYPE% emgutf.sln /project Emgu.TF.Lite.Models.nuget 


:END_OF_SCRIPT
popd
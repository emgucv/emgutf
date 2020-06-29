
pushd %~p0
cd ../..

IF "%1%"=="64" ECHO "BUILDING 64bit solution" 
IF "%1%"=="ARM" ECHO "BUILDING ARM solution"
IF "%1%"=="32" ECHO "BUILDING 32bit solution"

SET OS_MODE=
IF "%1%"=="64" SET OS_MODE= Win64
IF "%1%"=="ARM" SET OS_MODE= ARM

IF NOT "%2%"=="clean" GOTO END_OF_CLEAN
rm -rf %USERPROFILE%\_bazel_%USERNAME%
:END_OF_CLEAN

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

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [15.0^,16.0^) -property installationPath`) DO SET VS2017_DIR=%%F
SET VS2017=%VS2017_DIR%\Common7\IDE\devenv.com

FOR /F "tokens=* USEBACKQ" %%F IN (`miscellaneous\vswhere.exe -version [16.0^,17.0^) -property installationPath`) DO SET VS2019_DIR=%%F
SET VS2019=%VS2019_DIR%\Common7\IDE\devenv.com

IF EXIST "%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe" SET MSBUILD35=%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe
IF EXIST "%windir%\Microsoft.NET\Framework64\v3.5\MSBuild.exe" SET MSBUILD35=%windir%\Microsoft.NET\Framework64\v3.5\MSBuild.exe
IF EXIST "%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" SET MSBUILD40=%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

IF EXIST "%MSBUILD35%" SET DEVENV=%MSBUILD35%
IF EXIST "%MSBUILD40%" SET DEVENV=%MSBUILD40%
IF EXIST "%VS2005%" SET DEVENV=%VS2005% 
IF EXIST "%VS2008%" SET DEVENV=%VS2008%
IF EXIST "%VS2010%" SET DEVENV=%VS2010%
IF EXIST "%VS2012%" SET DEVENV=%VS2012%
IF EXIST "%VS2013%" SET DEVENV=%VS2013%
IF EXIST "%VS2015%" SET DEVENV=%VS2015%
IF EXIST "%VS2017%" SET DEVENV=%VS2017%
IF EXIST "%VS2019%" SET DEVENV=%VS2019%


:SET_BAZEL_VS_VC
IF "%DEVENV%"=="%VS2015%" SET BAZEL_VS=%VS2015:\Common7\Tools\..\IDE\devenv.com=%
IF "%DEVENV%"=="%VS2017%" SET BAZEL_VS=%VS2017:\Common7\IDE\devenv.com=%
IF "%DEVENV%"=="%VS2019%" SET BAZEL_VS=%VS2019:\Common7\IDE\devenv.com=%
IF NOT "%BAZEL_VS%"=="" SET BAZEL_VC=%BAZEL_VS%\VC
REM SET BAZEL_VS="%BAZEL_VS%"
ECHO Using BAZEL_VC=%BAZEL_VC%

cd tensorflow

SET MSYS_PATH=C:\msys64
SET MSYS_BIN=%MSYS_PATH%\usr\bin


%MSYS_BIN%\bazel build -c opt //tensorflow/tfliteextern:libtfliteextern.so --verbose_failures
      
cd ..

IF NOT EXIST lib\x64 mkdir lib\x64
cp -f tensorflow/bazel-bin/tensorflow/tfliteextern/libtfliteextern.so lib/x64/tfliteextern.dll

:START_OF_MSVC_DEPENDENCY
IF "%BAZEL_VC%"=="" GOTO END_OF_MSVC_DEPENDENCY
IF "%DEVENV%"=="%VS2015%" GOTO VS2015_DEPEDENCY
IF "%DEVENV%"=="%VS2017%" GOTO VS2017_DEPEDENCY
IF "%DEVENV%"=="%VS2019%" GOTO VS2019_DEPEDENCY
GOTO END_OF_MSVC_DEPENDENCY

:VS2015_DEPEDENCY
copy /Y "%BAZEL_VC%\redist\x64\Microsoft.VC140.CRT\*" lib\x64\
rm lib\x64\vccorlib140.dll
GOTO END_OF_MSVC_DEPENDENCY

:VS2017_DEPEDENCY
for /d %%i in ( "%BAZEL_VC%\Redist\MSVC\*" ) do SET VS2017_REDIST=%%i\x64\Microsoft.VC141.CRT
copy /Y "%VS2017_REDIST%\*140.dll" lib\x64\
copy /Y "%VS2017_REDIST%\*140_1.dll" lib\x64\
copy /Y "%VS2017_REDIST%\*140_2.dll" lib\x64\
REM　rm lib\x64\vccorlib140.dll
GOTO END_OF_MSVC_DEPENDENCY

:VS2019_DEPEDENCY
for /d %%i in ( "%BAZEL_VC%\Redist\MSVC\14*" ) do SET VS2019_REDIST=%%i\x64\Microsoft.VC142.CRT
copy /Y "%VS2019_REDIST%\*140.dll" lib\x64\
copy /Y "%VS2019_REDIST%\*140_1.dll" lib\x64\
copy /Y "%VS2019_REDIST%\*140_2.dll" lib\x64\
REM rm lib\x64\vccorlib140.dll

:END_OF_MSVC_DEPENDENCY
popd
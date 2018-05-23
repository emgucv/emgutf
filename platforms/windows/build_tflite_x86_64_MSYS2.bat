REM @echo off
pushd %~p0
cd ..\..

cd tensorflow

SET MSYS_PATH=C:\msys64
SET MSYS_BIN=%MSYS_PATH%\usr\bin
SET PATH=%MSYS_BIN%;%PATH%
sh.exe tensorflow\contrib\lite\download_dependencies.sh
make.exe  -f tensorflow\contrib\lite\Makefile clean 
make.exe  -f tensorflow\contrib\lite\Makefile tfliteextern.so 
cd ..
mkdir -p lib\x64
cp tensorflow\tensorflow\contrib\lite\gen\bin\tfliteextern.so lib\x64\tfliteextern.dll
cp %MSYS_BIN%\msys-2.0.dll lib\x64
cp %MSYS_BIN%\msys-gcc_s-seh-1.dll lib\x64
cp %MSYS_BIN%\msys-stdc++-6.dll lib\x64
popd
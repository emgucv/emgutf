REM @echo off
pushd %~p0
cd ..\..

cd tensorflow

SET MINGW_PATH=C:\mingw-w64\x86_64-7.3.0-posix-seh-rt_v5-rev0\mingw64
SET MINGW_BIN=%MINGW_PATH%\bin
SET PATH=%MINGW_BIN%;%PATH%
cd tensorflow\contrib\lite\
sh download_dependencies.sh
cd ..\..\..
mingw32-make.exe  -f tensorflow\contrib\lite\Makefile clean 
mingw32-make.exe  -f tensorflow\contrib\lite\Makefile tfliteextern.so CXX=x86_64-w64-mingw32-gcc
cd ..
mkdir -p lib\x64
cp tensorflow\tensorflow\contrib\lite\gen\bin\tfliteextern.so lib\x64\tfliteextern.dll

popd

pushd %~p0
cd ../..
cd tensorflow

SET MSYS_PATH=C:\msys64
SET MSYS_BIN=%MSYS_PATH%\usr\bin
REM SET PATH=%MSYS_BIN%;%PATH%

SET VS2015=%VS140COMNTOOLS%..\IDE\devenv.com
IF EXIST "%VS2015%" SET DEVENV=%VS2015%
IF "%DEVENV%"=="%VS2015%" SET BAZEL_VS=%VS2015:\Common7\Tools\..\IDE\devenv.com=%
IF NOT "%BAZEL_VS%"=="" SET BAZEL_VC=%BAZEL_VS%\VC

%MSYS_BIN%\bazel build -c opt //tensorflow/tfliteextern:libtfliteextern.so --verbose_failures
      
cd ..

mkdir -p lib/x64
cp tensorflow/bazel-bin/tensorflow/tfliteextern/libtfliteextern.so lib/x64/tfliteextern.dll
popd
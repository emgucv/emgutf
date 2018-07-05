
pushd %~p0
cd ../..
cd tensorflow

SET MSYS_PATH=C:\msys64
SET MSYS_BIN=%MSYS_PATH%\usr\bin
REM SET PATH=%MSYS_BIN%;%PATH%

%MSYS_BIN%\bazel build -c opt //tensorflow/tfliteextern:libtfliteextern.so
      
cd ..

REM mkdir -p lib/android/$1
REM cp tensorflow/bazel-bin/tensorflow/tfliteextern/*.so lib/android/$1
popd
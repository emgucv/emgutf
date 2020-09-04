REM go to the folder of the current script
pushd %~p0
cd ..
cd ..
cd ..
docker run -v %cd%:c:\src -w c:\src\platforms\windows\docker -m 6G emgu/vs2019_buildtools_cuda_bazel:latest Docker_Build_Helper_tf.bat %*
popd
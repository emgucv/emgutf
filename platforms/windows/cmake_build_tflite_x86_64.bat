pushd %~p0
call cmake_build_tflite_x86 64 xnn
popd

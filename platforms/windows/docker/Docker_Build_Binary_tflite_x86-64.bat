REM go to the folder of the current script
pushd %~p0
call Docker_Build_Binary_tflite 64 no_xnn
popd
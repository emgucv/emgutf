REM go to the folder of the current script
pushd %~p0
call Docker_Build_Binary_tf 64 cuda
popd
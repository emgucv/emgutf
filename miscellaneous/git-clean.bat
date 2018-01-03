REM go to the folder of the current script
pushd %~p0
cd ..
cd tensorflow
rm -rf bazel-*
rm -rf tensorflow/contrib/cmake/build
git clean -d -fx "."
cd ..

git clean -d -fx "." 
popd

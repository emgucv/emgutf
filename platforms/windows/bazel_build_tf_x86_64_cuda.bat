
pushd %~p0

call bazel_build_tf_x86_64 64 gpu 

popd



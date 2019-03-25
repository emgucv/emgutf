
pushd %~p0

call bazel_build_tf 64 cpu

popd
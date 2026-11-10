#!/bin/bash -v

# Ubuntu 26.04 changes compared to 24.04:
#  - mono-complete and python3-future are no longer in the archive. mono-devel
#    provides the mono runtime used to run nuget.exe when packaging. The TF
#    build uses Bazel's hermetic Python, so python3-future is not needed.
#  - dotnet is required by CMake and is in the archive as dotnet-sdk-10.0.
sudo apt install build-essential protobuf-compiler libprotobuf-dev \
  python-is-python3 mono-devel dotnet-sdk-10.0

if [ "$1" == "cuda" ]; then
  sudo apt-get install nvidia-cuda-dev nvidia-cuda-toolkit nvidia-cudnn libcudnn-frontend-dev nvidia-cuda-toolkit-gcc 
fi

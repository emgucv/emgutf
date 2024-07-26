#!/bin/bash -v

sudo apt install build-essential protobuf-compiler libprotobuf-dev \
  python-is-python3 python3-future mono-complete 
  
if [ "$1" == "cuda" ]; then
  sudo apt-get install nvidia-cuda-dev nvidia-cuda-toolkit nvidia-cudnn libcudnn-frontend-dev nvidia-cuda-toolkit-gcc 
fi

#!/bin/bash

sudo apt-get update
sudo apt-get install android-sdk

# The MAUI Android workload is used to build the MAUI Android runtime package
# (Emgu.TF.Runtime/Maui/Android). It needs the .NET SDK and a JDK.
# Note: Microsoft does not officially support MAUI on Linux.
sudo apt-get install dotnet-sdk-10.0 openjdk-21-jdk-headless
sudo dotnet workload install maui-android

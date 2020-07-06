FROM mcr.microsoft.com/dotnet/core/sdk:3.1
RUN apt update
RUN apt -y install cmake build-essential protobuf-compiler libprotobuf-dev python-future python3-future

# Set the Environment Variable for the DOTNET CLI HOME PATH
ARG dotnet_cli_home_arg=/tmp/
ENV DOTNET_CLI_HOME=$dotnet_cli_home_arg



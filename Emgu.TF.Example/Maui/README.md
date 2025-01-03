This is a MAUI demo project.

To make sure you have dotnet MAUI workload installed, open the terminal and run this 
> dotnet workload restore

To restore the nuget package, in the terminal run this command:
> dotnet restore

To build the Android MAUI app, first start up an Android simulator (or connect to an Android device), then use the following command and build and run the Android app:
> dotnet build -t:Run -f net9.0-android

For Windows build, you can use Visual Studio on windows.

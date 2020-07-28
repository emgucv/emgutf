REM go to the folder of the current script
pushd %~p0
tasklist | find /i "java.exe" && taskkill /im java.exe /F || echo process "java.exe" not running.
tasklist | find /i "adb.exe" && taskkill /im adb.exe /F || echo process "adb.exe" not running.
popd

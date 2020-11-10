cd c:\
xcopy c:\src c:\TEMP /s /e /h 
cd c:\TEMP\platforms\windows 
call bazel_build_tf_x86_64.bat %*
IF NOT EXIST c:\src\lib mkdir c:\src\lib
xcopy c:\TEMP\lib c:\src\lib /s /e /h
call build_emgutf_x64_doc.bat full 64 doc nuget package
IF NOT EXIST c:\src\package mkdir c:\src\package
copy /Y c:\TEMP\*.zip c:\src\package
copy /Y c:\TEMP\*.exe c:\src\package
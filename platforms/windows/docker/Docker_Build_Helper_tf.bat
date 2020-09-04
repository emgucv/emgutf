cd c:\
xcopy c:\src c:\TEMP /s /e /h 
cd c:\TEMP\platforms\windows 
call bazel_build_tf_x86_64.bat %*
IF NOT EXIST c:\src\package mkdir c:\src\package
call build_emgutf_x64_doc.bat full 64 doc nuget package
xcopy c:\TEMP\package c:\src\package /s /e /h /Y
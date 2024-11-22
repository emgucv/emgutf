# - Try to find the csharp compilers and utilities
#
# defines
#
# AL_EXECUTABLE - Path to the 'al' command
# CSC_EXECUTABLE - Path to the csharp compiler
# DOTNET_EXECUTABLE - Path to the 'dotnet' command
# DOTNET_FOUND - If 'dotnet' command is found
# NUGET_EXECUTABLE - Path to the nuget executable
# NUGET_FOUND - If 'nuget' command is found
# MONO_EXECUTABLE - Path to the mono executable
# MONO_FOUND - If 'mono' command is found
# SIGNTOOL_EXECUTABLE - Path to the signtool executable
# SIGNTOOL_FOUND - If 'signtool' command is found
# GACUTIL_EXECUTABLE - Path to the 'gacutil'
# MSBUILD_EXECUTABLE - Path to 'msbuild'
#
# copyright (c) 2007 Arno Rehn arno@arnorehn.de
#
# Redistribution and use is allowed according to the terms of the GPL license.
#
# Modified by canming to find .NET on Windows
# copyright (c) 2009 - 2019 Canming Huang support@emgu.com


SET (PROGRAM_FILES_X86_ENV_STR "programfiles(x86)")
SET (PROGRAM_FILES_X64_ENV_STR "programfiles")

FIND_PROGRAM (CSC_EXECUTABLE_20 
NAMES csc gmcs
PATHS
$ENV{windir}/Microsoft.NET/Framework/v2.0.50727/
"C:/WINDOWS/Microsoft.NET/Framework/v2.0.50727"
/Library/Frameworks/Mono.framework/Commands/
CMAKE_FIND_ROOT_PATH_BOTH
)

FIND_PROGRAM (MSBUILD_EXECUTABLE_20 
NAMES msbuild 
PATHS
$ENV{windir}/Microsoft.NET/Framework/v2.0.50727/
"C:/WINDOWS/Microsoft.NET/Framework/v2.0.50727"
/Library/Frameworks/Mono.framework/Commands/
CMAKE_FIND_ROOT_PATH_BOTH
)
IF(CSC_EXECUTABLE_20)
SET (CSC_EXECUTABLE ${CSC_EXECUTABLE_20})
SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_20})
ENDIF()

FIND_PROGRAM (CSC_EXECUTABLE_35 csc
$ENV{windir}/Microsoft.NET/Framework64/v3.5/
"C:/Windows/Microsoft.NET/Framework64/v3.5"
$ENV{windir}/Microsoft.NET/Framework/v3.5/
"C:/Windows/Microsoft.NET/Framework/v3.5"
CMAKE_FIND_ROOT_PATH_BOTH
)
FIND_PROGRAM (MSBUILD_EXECUTABLE_35 msbuild
$ENV{windir}/Microsoft.NET/Framework64/v3.5/
"C:/Windows/Microsoft.NET/Framework64/v3.5"
$ENV{windir}/Microsoft.NET/Framework/v3.5/
"C:/Windows/Microsoft.NET/Framework/v3.5"
CMAKE_FIND_ROOT_PATH_BOTH
)
IF(CSC_EXECUTABLE_35)
SET (CSC_EXECUTABLE ${CSC_EXECUTABLE_35})
SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_35})
ENDIF()

FIND_FILE (CSC_MSCORLIB_35 mscorlib.dll
"$ENV{programfiles}/Reference Assemblies/Microsoft/Framework/.NETFramework/v3.5/Profile/Client/"
"C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v3.5/Profile/Client/"
)

FIND_PROGRAM (CSC_EXECUTABLE_40 
NAMES csc mcs dmcs
PATHS
$ENV{windir}/Microsoft.NET/Framework64/v4.0.30319/
"C:/Microsoft.NET/Framework64/v4.0.30319/"
$ENV{windir}/Microsoft.NET/Framework/v4.0.30319/
"C:/Microsoft.NET/Framework/v4.0.30319/"
/Library/Frameworks/Mono.framework/Commands/
CMAKE_FIND_ROOT_PATH_BOTH
)

FIND_PROGRAM (MSBUILD_EXECUTABLE_40 
NAMES msbuild 
PATHS
$ENV{windir}/Microsoft.NET/Framework64/v4.0.30319/
"C:/Microsoft.NET/Framework64/v4.0.30319/"
$ENV{windir}/Microsoft.NET/Framework/v4.0.30319/
"C:/Microsoft.NET/Framework/v4.0.30319/"
/Library/Frameworks/Mono.framework/Commands/
CMAKE_FIND_ROOT_PATH_BOTH
)

FIND_PROGRAM (CSC_EXECUTABLE_120 
NAMES csc 
PATHS
$ENV{${PROGRAM_FILES_X86_ENV_STR}}/MSBuild/12.0/Bin/
CMAKE_FIND_ROOT_PATH_BOTH
)
MESSAGE(STATUS "CSC_EXECUTABLE_120: ${CSC_EXECUTABLE_120}")

FIND_PROGRAM (MSBUILD_EXECUTABLE_120 
NAMES msbuild 
PATHS
$ENV{${PROGRAM_FILES_X86_ENV_STR}}/MSBuild/12.0/Bin/)
MESSAGE(STATUS "MSBUILD_EXECUTABLE_120 : ${MSBUILD_EXECUTABLE_120}")

FIND_PROGRAM (CSC_EXECUTABLE_140 
NAMES csc 
PATHS
$ENV{${PROGRAM_FILES_X86_ENV_STR}}/MSBuild/14.0/Bin/
CMAKE_FIND_ROOT_PATH_BOTH
)
MESSAGE(STATUS "CSC_EXECUTABLE_140: ${CSC_EXECUTABLE_140}")

FIND_PROGRAM (MSBUILD_EXECUTABLE_140 
NAMES msbuild 
PATHS
$ENV{${PROGRAM_FILES_X86_ENV_STR}}/MSBuild/14.0/Bin/
CMAKE_FIND_ROOT_PATH_BOTH
)
MESSAGE(STATUS "MSBUILD_EXECUTABLE_140 : ${MSBUILD_EXECUTABLE_140}")

FIND_PROGRAM (CSC_EXECUTABLE_150 
NAMES csc 
PATHS
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2017/Professional/MSBuild/15.0/Bin/Roslyn"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2017/Community/MSBuild/15.0/Bin/Roslyn"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2017/Enterprise/MSBuild/15.0/Bin/Roslyn"
CMAKE_FIND_ROOT_PATH_BOTH
)
MESSAGE(STATUS "CSC_EXECUTABLE_150: ${CSC_EXECUTABLE_150}")

FIND_PROGRAM (MSBUILD_EXECUTABLE_150 
NAMES msbuild 
PATHS
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2017/Professional/MSBuild/15.0/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2017/Community/MSBuild/15.0/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2017/Enterprise/MSBuild/15.0/Bin"
CMAKE_FIND_ROOT_PATH_BOTH
)
MESSAGE(STATUS "MSBUILD_EXECUTABLE_150 : ${MSBUILD_EXECUTABLE_150}")

FIND_PROGRAM (CSC_EXECUTABLE_160 
NAMES csc 
PATHS
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2019/Professional/MSBuild/Current/Bin/Roslyn"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/Roslyn"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2019/Enterprise/MSBuild/Current/Bin/Roslyn"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2019/BuildTools/MSBuild/Current/Bin/Roslyn"
CMAKE_FIND_ROOT_PATH_BOTH
)
MESSAGE(STATUS "CSC_EXECUTABLE_160: ${CSC_EXECUTABLE_160}")

FIND_PROGRAM (MSBUILD_EXECUTABLE_160 
NAMES msbuild 
PATHS
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2019/Professional/MSBuild/Current/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2019/Enterprise/MSBuild/Current/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft Visual Studio/2019/BuildTools/MSBuild/Current/Bin"
CMAKE_FIND_ROOT_PATH_BOTH
)
MESSAGE(STATUS "MSBUILD_EXECUTABLE_160 : ${MSBUILD_EXECUTABLE_160}")

FIND_PROGRAM (CSC_EXECUTABLE_170 
NAMES csc 
PATHS
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/Microsoft Visual Studio/2022/Professional/Msbuild/Current/Bin/Roslyn"
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/Microsoft Visual Studio/2022/Community/Msbuild/Current/Bin/Roslyn"
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/Microsoft Visual Studio/2022/Enterprise/Msbuild/Current/Bin/Roslyn"
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/Microsoft Visual Studio/2022/BuildTools/Msbuild/Current/Bin/Roslyn"
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/Microsoft Visual Studio/2022/Preview/Msbuild/Current/Bin/Roslyn"
CMAKE_FIND_ROOT_PATH_BOTH
)
MESSAGE(STATUS "CSC_EXECUTABLE_170: ${CSC_EXECUTABLE_170}")

FIND_PROGRAM (MSBUILD_EXECUTABLE_170 
NAMES msbuild 
PATHS
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/Microsoft Visual Studio/2022/Professional/Msbuild/Current/Bin"
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/Microsoft Visual Studio/2022/Community/Msbuild/Current/Bin"
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/Microsoft Visual Studio/2022/Enterprise/Msbuild/Current/Bin"
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/Microsoft Visual Studio/2022/BuildTools/Msbuild/Current/Bin"
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/Microsoft Visual Studio/2022/Preview/Msbuild/Current/Bin"
CMAKE_FIND_ROOT_PATH_BOTH
)
MESSAGE(STATUS "MSBUILD_EXECUTABLE_170 : ${MSBUILD_EXECUTABLE_170}")

SET(CSC_FOUND FALSE)

IF(CSC_EXECUTABLE_20)
  SET (CSC_EXECUTABLE ${CSC_EXECUTABLE_20})
  IF (CSC_PREFERRED_VERSION MATCHES "2.0")
	SET(CSC_FOUND TRUE)
  ENDIF()
ENDIF()

IF(CSC_EXECUTABLE_35 AND (NOT ${CSC_FOUND}))
  SET (CSC_EXECUTABLE ${CSC_EXECUTABLE_35})
  IF (CSC_PREFERRED_VERSION MATCHES "3.5")
	SET(CSC_FOUND TRUE)
  ENDIF()
ENDIF()

IF(CSC_EXECUTABLE_40 AND (NOT ${CSC_FOUND}))
  SET (CSC_EXECUTABLE ${CSC_EXECUTABLE_40})
  IF (CSC_PREFERRED_VERSION MATCHES "4.0")
	SET(CSC_FOUND TRUE)
  ENDIF()  
ENDIF()

IF(CSC_EXECUTABLE_120 AND (NOT ${CSC_FOUND}))
  SET (CSC_EXECUTABLE ${CSC_EXECUTABLE_120})
  IF (CSC_PREFERRED_VERSION MATCHES "12.0")
	SET(CSC_FOUND TRUE)
  ENDIF()  
ENDIF()

IF(CSC_EXECUTABLE_140 AND (NOT ${CSC_FOUND}))
  SET (CSC_EXECUTABLE ${CSC_EXECUTABLE_140})
  
  IF (CSC_PREFERRED_VERSION MATCHES "14.0")
	SET(CSC_FOUND TRUE)
  ENDIF()  
ENDIF()

IF(CSC_EXECUTABLE_150 AND (NOT ${CSC_FOUND}))
  SET (CSC_EXECUTABLE ${CSC_EXECUTABLE_150})
  IF (CSC_PREFERRED_VERSION MATCHES "15.0")
	SET(CSC_FOUND TRUE)
  ENDIF()  
ENDIF()

IF(CSC_EXECUTABLE_160 AND (NOT ${CSC_FOUND}))
  SET (CSC_EXECUTABLE ${CSC_EXECUTABLE_160})
  IF (CSC_PREFERRED_VERSION MATCHES "16.0")
	SET(CSC_FOUND TRUE)
  ENDIF()  
ENDIF()

IF(CSC_EXECUTABLE_170 AND (NOT ${CSC_FOUND}))
  SET (CSC_EXECUTABLE ${CSC_EXECUTABLE_170})
  IF (CSC_PREFERRED_VERSION MATCHES "17.0")
	SET(CSC_FOUND TRUE)
  ENDIF()  
ENDIF()

IF(MSBUILD_EXECUTABLE_20)
  SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_20})
ENDIF()

IF(MSBUILD_EXECUTABLE_35)
  SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_35})
ENDIF()

IF(MSBUILD_EXECUTABLE_40)
  SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_40})
ENDIF()

IF(MSBUILD_EXECUTABLE_120)
  SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_120})
ENDIF()

IF(MSBUILD_EXECUTABLE_140)
  SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_140})
ENDIF()

IF(MSBUILD_EXECUTABLE_150)
  SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_150})
ENDIF()

IF(MSBUILD_EXECUTABLE_160)
  SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_160})
ENDIF()


IF(MSBUILD_EXECUTABLE_170)
  SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_170})
ENDIF()

#IF(CSC_EXECUTABLE_40 AND CSC_PREFERRED_VERSION MATCHES "4.0")
#SET (MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_40})
#ENDIF()
#ELSE(WIN32)
#FIND_PROGRAM (CSC_EXECUTABLE mcs)
#FIND_PROGRAM (MSBUILD_EXECUTABLE xbuild)
#ENDIF(WIN32)

IF (MSBUILD_EXECUTABLE)
  SET (MSBUILD_FOUND TRUE)
ELSE()
  SET (MSBUILD_FOUND FALSE)
ENDIF()  


FIND_PROGRAM (GACUTIL_EXECUTABLE
NAMES gacutil
PATHS
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.8 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.6.1 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.6 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v8.1A/bin/NETFX 4.5.1 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v8.0A/bin/NETFX 4.0 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v7.1A/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v7.0/Bin" 
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v7.0A/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v6.0/Bin" 
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v6.0A/Bin"
"C:/Program Files/Microsoft SDKs/Windows/v6.0/bin" 
"C:/Program Files/Microsoft SDKs/Windows/v6.0A/bin" 
/usr/lib/mono/2.0
/usr/bin
/Library/Frameworks/Mono.framework/Commands/
CMAKE_FIND_ROOT_PATH_BOTH
)

FIND_PROGRAM (AL_EXECUTABLE
NAMES al
PATHS
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.8 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.6.1 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.6 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v8.1A/bin/NETFX 4.5.1 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v8.0A/bin/NETFX 4.0 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v7.1A/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v7.0/Bin" 
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v7.0A/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v6.0/Bin" 
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v6.0A/Bin"
"C:/Program Files/Microsoft SDKs/Windows/v7.0/bin"
"C:/Program Files/Microsoft SDKs/Windows/v7.0A/bin"
"C:/Program Files/Microsoft SDKs/Windows/v6.0/bin" 
"C:/Program Files/Microsoft SDKs/Windows/v6.0A/bin"
"C:/WINDOWS/Microsoft.NET/Framework/v2.0.50727"
"C:/Windows/Microsoft.NET/Framework/v3.5" 
$ENV{windir}/Microsoft.NET/Framework/v3.5
$ENV{windir}/Microsoft.NET/Framework/v2.0.50727
/usr/lib/mono/2.0
/usr/bin
/Library/Frameworks/Mono.framework/Commands/
CMAKE_FIND_ROOT_PATH_BOTH
)

FIND_PROGRAM (RESGEN_EXECUTABLE
NAMES resgen
PATHS
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.8 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.6.1 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.6 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v8.1A/bin/NETFX 4.5.1 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v8.0A/bin/NETFX 4.0 Tools"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v7.1A/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v7.0/Bin" 
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v7.0A/Bin"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v6.0/Bin" 
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Microsoft SDKs/Windows/v6.0A/Bin"
"$ENV{programfiles}/Microsoft Visual Studio 8/SDK/v2.0/Bin"
"C:/Program Files/Microsoft SDKs/Windows/v7.0/bin"
"C:/Program Files/Microsoft SDKs/Windows/v7.0A/bin"
"C:/Program Files/Microsoft SDKs/Windows/v6.0/bin" 
"C:/Program Files/Microsoft SDKs/Windows/v6.0A/bin"
"C:/Program Files/Microsoft Visual Studio 8/SDK/v2.0/Bin"
/usr/bin
/Library/Frameworks/Mono.framework/Commands/
CMAKE_FIND_ROOT_PATH_BOTH
)

FIND_PROGRAM(DOTNET_EXECUTABLE
NAMES dotnet
PATHS
"$ENV{${PROGRAM_FILES_X64_ENV_STR}}/dotnet"
/usr/local/share/dotnet/
CMAKE_FIND_ROOT_PATH_BOTH
)
IF (DOTNET_EXECUTABLE)
  SET (DOTNET_FOUND TRUE)
  EXECUTE_PROCESS(
	COMMAND ${DOTNET_EXECUTABLE} --version
	OUTPUT_VARIABLE DOTNET_VERSION
	RESULT_VARIABLE DOTNET_VERSION_RESULT
  )
  MESSAGE(STATUS "DOTNET_VERSION: ${DOTNET_VERSION}")
  # Extract the major version using a regular expression
  string(REGEX MATCH "^[0-9]+" DOTNET_VERSION_MAJOR "${DOTNET_VERSION}")
  MESSAGE(STATUS "DOTNET_VERSION_MAJOR: ${DOTNET_VERSION_MAJOR}")
ELSE()
  SET (DOTNET_FOUND FALSE)
ENDIF()

FIND_PROGRAM(MONO_EXECUTABLE
NAMES mono
PATHS
/usr/bin/
CMAKE_FIND_ROOT_PATH_BOTH
)
IF (MONO_EXECUTABLE)
  SET (MONO_FOUND TRUE)
ELSE()
  SET (MONO_FOUND FALSE)
ENDIF()  
MESSAGE(STATUS "MONO_EXECUTABLE : ${MONO_EXECUTABLE}")

FIND_PROGRAM (NUGET_EXECUTABLE 
NAMES nuget nuget.exe
PATHS
"${CMAKE_CURRENT_LIST_DIR}/../../miscellaneous"
CMAKE_FIND_ROOT_PATH_BOTH
)
IF (NUGET_EXECUTABLE)
  SET (NUGET_FOUND TRUE)
ELSE()
  SET (NUGET_FOUND FALSE)
ENDIF()  
MESSAGE(STATUS "NUGET_EXECUTABLE : ${NUGET_EXECUTABLE}")

FIND_PROGRAM (SIGNTOOL_EXECUTABLE 
NAMES signtool 
PATHS
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Windows Kits/10/bin/10.0.22621.0/x64"
"$ENV{${PROGRAM_FILES_X86_ENV_STR}}/Windows Kits/10/bin/10.0.19041.0/x64"
CMAKE_FIND_ROOT_PATH_BOTH
)
IF (SIGNTOOL_EXECUTABLE)
  SET (SIGNTOOL_FOUND TRUE)
ELSE()
  SET (SIGNTOOL_FOUND FALSE)
ENDIF()  
MESSAGE(STATUS "SIGNTOOL_EXECUTABLE : ${SIGNTOOL_EXECUTABLE}")

FIND_PROGRAM(VSTOOL_EXECUTABLE
NAMES vstool
PATHS
"/Applications/Visual Studio.app/Contents/MacOS"
CMAKE_FIND_ROOT_PATH_BOTH
)
IF (VSTOOL_EXECUTABLE)
  SET (VSTOOL_FOUND TRUE)
ELSE()
  SET (VSTOOL_FOUND FALSE)
ENDIF()

SET (CSharp_FOUND FALSE)
IF (CSC_EXECUTABLE AND AL_EXECUTABLE AND RESGEN_EXECUTABLE AND MSBUILD_EXECUTABLE)
  SET (CSharp_FOUND TRUE)
ENDIF ()

IF (NOT CSharp_FIND_QUIETLY)
  IF (CSC_EXECUTABLE)
    MESSAGE(STATUS "Found csc: ${CSC_EXECUTABLE}")
  ELSE()
    MESSAGE(STATUS "Could not find csc")
  ENDIF (CSC_EXECUTABLE)
  IF (GACUTIL_EXECUTABLE)
    MESSAGE(STATUS "Found gacutil: ${GACUTIL_EXECUTABLE}")
  ELSE()
    MESSAGE(STATUS "Could not find gacutil")
  ENDIF (GACUTIL_EXECUTABLE)
  IF (AL_EXECUTABLE)
    MESSAGE(STATUS "Found al: ${AL_EXECUTABLE}")
  ELSE()
    MESSAGE(STATUS "Could not find al")
  ENDIF (AL_EXECUTABLE)
  IF (RESGEN_EXECUTABLE)
    MESSAGE(STATUS "Found resgen: ${RESGEN_EXECUTABLE}")
  ELSE()
    MESSAGE(STATUS "Could not find resgen")
  ENDIF(RESGEN_EXECUTABLE)
  IF (MSBUILD_EXECUTABLE)
    MESSAGE(STATUS "Found msbuild: ${MSBUILD_EXECUTABLE}")
  ELSE()
    MESSAGE(STATUS "Could not find msbuild")
  ENDIF()
  IF (VSTOOL_EXECUTABLE)
    MESSAGE(STATUS "Found vstool: ${VSTOOL_EXECUTABLE}")
  ELSE()
    MESSAGE(STATUS "Could not find vstool")
  ENDIF()
  IF(DOTNET_EXECUTABLE)
    MESSAGE(STATUS "Found dotnet: ${DOTNET_EXECUTABLE}")
  ELSE()
    MESSAGE(STATUS "Could not find dotnet")
  ENDIF()
ENDIF (NOT CSharp_FIND_QUIETLY)

IF (CSharp_FOUND OR DOTNET_FOUND)
ELSE()
  IF (CSharp_FIND_REQUIRED)
    MESSAGE(FATAL_ERROR "Could not find one or more of the following programs: csc, gacutil, al, resgen, msbuild, dotnet")
  ENDIF() 
ENDIF()

MARK_AS_ADVANCED(
  CSC_EXECUTABLE 
  VSTOOL_EXECUTABLE 
  AL_EXECUTABLE 
  GACUTIL_EXECUTABLE 
  MSBUILD_EXECUTABLE 
  DOTNET_EXECUTABLE 
  DOTNET_VERSION
  DOTNET_VERSION_MAJOR
  DOTNET_FOUND 
  MSBUILD_FOUND 
  VSTOOLS_FOUND 
  NUGET_EXECUTABLE 
  NUGET_FOUND 
  MONO_EXECUTABLE 
  MONO_FOUND)



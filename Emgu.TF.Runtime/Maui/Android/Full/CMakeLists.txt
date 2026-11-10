# --------------------------------------------------------
#  Copyright (C) 2004-2026 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

# On Windows and macOS the Android project is always built. On other hosts (e.g. Ubuntu) it is only built when the
# "android" dotnet workload is installed (e.g. "sudo dotnet workload install maui-android"), because the build fails without it.
SET(MAUI_ANDROID_BUILD_HOST OFF)
IF(WIN32 OR APPLE)
  SET(MAUI_ANDROID_BUILD_HOST ON)
ELSEIF(DOTNET_FOUND)
  EXECUTE_PROCESS(COMMAND "${DOTNET_EXECUTABLE}" workload list OUTPUT_VARIABLE DOTNET_WORKLOAD_LIST ERROR_QUIET OUTPUT_STRIP_TRAILING_WHITESPACE)
  IF("${DOTNET_WORKLOAD_LIST}" MATCHES "(^|\n)android[ \t]")
    SET(MAUI_ANDROID_BUILD_HOST ON)
  ELSEIF(HAVE_ANDROID)
    MESSAGE(STATUS "The android dotnet workload is not installed, skipping Emgu.TF.runtime.maui.android. To build it, run: sudo dotnet workload install maui-android")
  ENDIF()
ENDIF()

IF(HAVE_ANDROID AND MAUI_ANDROID_BUILD_HOST)
  PROJECT(Emgu.TF.runtime.maui.android)
  INSTALL(
    DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
    DESTINATION Emgu.TF.Runtime/Maui/Android
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "*.cs"
    PATTERN "Directory.Build.props"
    PATTERN "${PROJECT_NAME}.csproj"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  SET(EXTRA_BUILD_FLAGS "/p:Platform=\"AnyCPU\"")
  
  BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj" "${EXTRA_BUILD_FLAGS}" ALL)
  
  #if(ENABLE_SOLUTION_FOLDERS)
  #    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "emgu")
  #endif() 
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF)
  
  INSTALL(
    DIRECTORY 
    ${CMAKE_CURRENT_SOURCE_DIR}/../../../../libs/Build/${PROJECT_NAME}
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}/Build
    COMPONENT emgutf_binary
	FILES_MATCHING 
        PATTERN "*.dll"
        PATTERN "*.pdb"
        PATTERN "*.xml")
ENDIF()


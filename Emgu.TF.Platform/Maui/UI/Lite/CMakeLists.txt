# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

# Only build MAUI projects on Windows or MacOS. MAUI compilation on Linux seems to be a problem.
IF(HAVE_MAUI_LITE AND (APPLE OR WIN32))
  PROJECT(Emgu.TF.Lite.Platform.Maui.UI)
  INSTALL(
    DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
    DESTINATION Emgu.TF.Platform/Maui/UI
    COMPONENT emgutf_source
    FILES_MATCHING 
	PATTERN "*.cs"
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
  
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite)

  INSTALL(
    DIRECTORY
    ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF.Lite.Platform.Maui.UI
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)

ENDIF()
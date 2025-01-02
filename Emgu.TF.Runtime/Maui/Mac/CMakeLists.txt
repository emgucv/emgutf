# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_MAUI_MAC_LITE)

  INSTALL(
      DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
      DESTINATION Emgu.TF.Runtime/Maui
      COMPONENT emgutf_source
      FILES_MATCHING 
      PATTERN "*.csproj"
      PATTERN "*.cs"
      PATTERN "Directory.Build.props"
      PATTERN "README.md"
      PATTERN ".svn" EXCLUDE
      PATTERN "obj" EXCLUDE
      PATTERN "CMakeFiles" EXCLUDE
      PATTERN "${PROJECT_NAME}.dir" EXCLUDE
      )
	  
  IF (APPLE)
    PROJECT(Emgu.TF.Lite.runtime.maui.macos)
    SET(EXTRA_BUILD_FLAGS "/p:Platform=\"AnyCPU\"")
    BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj" "${EXTRA_BUILD_FLAGS}" ALL)

    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite)  
    if(ENABLE_SOLUTION_FOLDERS)
      set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "emgu")
    endif() 
  ENDIF()

  INSTALL(
    DIRECTORY
    ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF.Lite.runtime.maui.macos
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
	
ENDIF()

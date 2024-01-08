# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_ANDROID AND HAVE_WINDESKTOP)
  PROJECT(Emgu.TF.Platform.Maui.UI)
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
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)
  

ENDIF()
# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_MAUI_LITE)
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
  
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard)

  #FOREACH (PLATFORMFORM_FOLDER IN ITEMS ${EMGUTF_MAUI_ANDROID_LITE_SRC_FOLDER} ${EMGUTF_MAUI_IOS_LITE_SRC_FOLDER} ${EMGUTF_MAUI_WINDOWS_LITE_SRC_FOLDER})
  #  INSTALL(
  #    FILES 
  #    ${CMAKE_CURRENT_SOURCE_DIR}/../../../lib/${PLATFORMFORM_FOLDER}/${PROJECT_NAME}.xml
  #    ${CMAKE_CURRENT_SOURCE_DIR}/../../../lib/${PLATFORMFORM_FOLDER}/${PROJECT_NAME}.dll 
  #    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}/${PLATFORMFORM_FOLDER}
  #    COMPONENT emgutf_binary)
  #ENDFOREACH ()

ENDIF()
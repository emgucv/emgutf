# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_IOS_LITE)

  INSTALL(
      DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
      DESTINATION Emgu.TF.Platform/Maui
      COMPONENT emgutf_source
      FILES_MATCHING 
      PATTERN "Emgu.TF.Lite.Platform.Maui.iOS.csproj"
	  PATTERN "*.cs"
      PATTERN ".svn" EXCLUDE
      PATTERN "obj" EXCLUDE
      PATTERN "CMakeFiles" EXCLUDE
      PATTERN "${PROJECT_NAME}.dir" EXCLUDE
      )
	  
  IF (APPLE)
    PROJECT(Emgu.TF.Lite.Platform.Maui.iOS)
	SET(EXTRA_BUILD_FLAGS "/p:Platform=\"AnyCPU\"")
    BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj" "${EXTRA_BUILD_FLAGS}" ALL)
    #DOTNET_MSBUILD_CSPROJ_IN_SOLUTION(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/../../../Solution/iOS/Emgu.CV.iOS.Maui.sln" "${PROJECT_NAME}" "${EXTRA_BUILD_FLAGS}" ALL)

    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard.IOS)  
    if(ENABLE_SOLUTION_FOLDERS)
      set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "emgu")
    endif() 
  ENDIF()
  
ENDIF()

IF(HAVE_MAUI_IOS_LITE)
  INSTALL(
      FILES 
      ${CMAKE_CURRENT_SOURCE_DIR}/../../../lib/Emgu.TF.Lite.Platform.Maui.iOS.xml
      ${CMAKE_CURRENT_SOURCE_DIR}/../../../lib/Emgu.TF.Lite.Platform.Maui.iOS.dll
      DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
      COMPONENT emgutf_binary)	  
ENDIF()

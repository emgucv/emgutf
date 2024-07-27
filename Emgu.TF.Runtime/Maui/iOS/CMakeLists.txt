# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_IOS_LITE)

  INSTALL(
      DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
      DESTINATION Emgu.TF.Runtime/Maui
      COMPONENT emgutf_source
      FILES_MATCHING 
      PATTERN "*.csproj"
	  PATTERN "*.cs"
      PATTERN ".svn" EXCLUDE
      PATTERN "obj" EXCLUDE
      PATTERN "CMakeFiles" EXCLUDE
      PATTERN "${PROJECT_NAME}.dir" EXCLUDE
      )
	  
  IF (APPLE)
    PROJECT(Emgu.TF.Lite.runtime.maui.ios)
	SET(EXTRA_BUILD_FLAGS "/p:Platform=\"AnyCPU\"")
    BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj" "${EXTRA_BUILD_FLAGS}" ALL)
    #DOTNET_MSBUILD_CSPROJ_IN_SOLUTION(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/../../../Solution/iOS/Emgu.CV.iOS.Maui.sln" "${PROJECT_NAME}" "${EXTRA_BUILD_FLAGS}" ALL)

    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite)  
    #if(ENABLE_SOLUTION_FOLDERS)
    #  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "emgu")
    #endif() 
  ENDIF()

  INSTALL(
    DIRECTORY
    ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF.Lite.runtime.maui.ios
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
	
ENDIF()

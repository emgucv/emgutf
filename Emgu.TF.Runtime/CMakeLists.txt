# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------


IF(WIN32 OR APPLE)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}
    DESTINATION .
    COMPONENT emgutf_source
    FILES_MATCHING 
    
    PATTERN "*.cs"
    PATTERN "*.resx"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
ENDIF()  

IF(HAVE_IOS_LITE)
  #APPLE IOS
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/ios
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Lite.Runtime.iOS.csproj"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  IF(APPLE)
    PROJECT(Emgu.TF.Lite.Runtime.iOS)
    BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/ios/${PROJECT_NAME}.csproj" "" ALL)
    INSTALL(
      FILES 
      ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
      ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
      DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
      COMPONENT emgutf_binary)
  ENDIF()	
ENDIF()

IF(HAVE_FULL_BUILD)
  #MacOS
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Mac
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Runtime.Mac.shproj"
    PATTERN "Emgu.TF.Runtime.Mac.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )

  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Windows
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
	PATTERN "Emgu.TF.Runtime.Windows.shproj"
	PATTERN "Emgu.TF.Runtime.Windows.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
	
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Ubuntu
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
	PATTERN "Emgu.TF.Runtime.Ubuntu.shproj"
	PATTERN "Emgu.TF.Runtime.Ubuntu.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
	
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Raspbian
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
	PATTERN "Emgu.TF.Runtime.Raspbian.shproj"
	PATTERN "Emgu.TF.Runtime.Raspbian.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
ENDIF()

IF(HAVE_ANDROID)
  PROJECT(Emgu.TF.Runtime.Android)
  INSTALL(
    DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}/Android
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "${PROJECT_NAME}.csproj"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/Android/${PROJECT_NAME}.csproj" "${EXTRA_BUILD_FLAGS}" ALL)
  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND ${DOTNET_EXECUTABLE} restore ${PROJECT_NAME}.csproj
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/Android")
  INSTALL(
    FILES 
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
ENDIF()

IF(HAVE_LITE_BUILD)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Mac
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
	PATTERN "Emgu.TF.Lite.Runtime.Mac.shproj"
	PATTERN "Emgu.TF.Lite.Runtime.Mac.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Windows
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
	PATTERN "Emgu.TF.Lite.Runtime.Windows.shproj"
	PATTERN "Emgu.TF.Lite.Runtime.Windows.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Ubuntu
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
	PATTERN "Emgu.TF.Lite.Runtime.Ubuntu.shproj"
	PATTERN "Emgu.TF.Lite.Runtime.Ubuntu.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Android
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Lite.Runtime.Android.csproj"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Raspbian
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
	PATTERN "Emgu.TF.Lite.Runtime.Raspbian.shproj"
	PATTERN "Emgu.TF.Lite.Runtime.Raspbian.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
ENDIF()

IF(HAVE_ANDROID_LITE)
  PROJECT(Emgu.TF.Lite.Runtime.Android)
  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/Android/${PROJECT_NAME}.csproj" "${EXTRA_BUILD_FLAGS}" ALL)
  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND ${DOTNET_EXECUTABLE} restore ${PROJECT_NAME}.csproj
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/Android"
	COMMENT "Restpring nuget packages")
  INSTALL(
    FILES 
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
ENDIF()

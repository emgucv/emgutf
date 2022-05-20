# --------------------------------------------------------
#  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_LITE_BUILD)
INSTALL(
  DIRECTORY
  ${CMAKE_CURRENT_SOURCE_DIR}
  DESTINATION .
  COMPONENT emgutf_source
  FILES_MATCHING 
  PATTERN "*.cs"
  PATTERN "*.shproj"
  PATTERN "*.projitems"
  PATTERN "*.resx"
  PATTERN "README.txt"
  PATTERN ".svn" EXCLUDE
  PATTERN "obj" EXCLUDE
  PATTERN "CMakeFiles" EXCLUDE
  PATTERN "${PROJECT_NAME}.dir" EXCLUDE
  PATTERN "*Unity*" EXCLUDE
  )

INSTALL(
  DIRECTORY
  ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.Netstandard
  DESTINATION Emgu.TF.Lite
  COMPONENT emgutf_source
  FILES_MATCHING 
  PATTERN "*.cs"
  PATTERN "*.projitems"
  PATTERN "*.shproj"
  PATTERN "*.csproj"
  PATTERN ".svn" EXCLUDE
  PATTERN "obj" EXCLUDE
  PATTERN "CMakeFiles" EXCLUDE
  PATTERN "x64" EXCLUDE #exclude build artifacts from Visual Studio build process
  PATTERN "x86" EXCLUDE #exclude build artifacts from Visual Studio build process
  PATTERN "${PROJECT_NAME}.dir" EXCLUDE
  )

INSTALL(
  DIRECTORY
  ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.Netstandard.IOS
  DESTINATION Emgu.TF.Lite
  COMPONENT emgutf_source
  FILES_MATCHING 
  PATTERN "*.cs"
  PATTERN "*.projitems"
  PATTERN "*.shproj"
  PATTERN "*.csproj"
  PATTERN ".svn" EXCLUDE
  PATTERN "obj" EXCLUDE
  PATTERN "CMakeFiles" EXCLUDE
  PATTERN "x64" EXCLUDE #exclude build artifacts from Visual Studio build process
  PATTERN "x86" EXCLUDE #exclude build artifacts from Visual Studio build process
  PATTERN "${PROJECT_NAME}.dir" EXCLUDE
  )


IF (DOTNET_FOUND)
  PROJECT(Emgu.TF.Lite.Netstandard)
  #MESSAGE("In Emgu.TF.Lite.Netstandard project now. HAVE_WINDESKTOP_X64: ${HAVE_WINDESKTOP_X64}")
  BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)
  
  IF (EMGU_SIGN_FOUND)
    IF (WIN32 AND HAVE_WINDESKTOP_X64_LITE)
      EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/runtimes/win-x64/native/tfliteextern.dll)
    ENDIF()
	IF (WIN32 AND HAVE_WINDESKTOP_X86_LITE)
      EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/runtimes/win-x86/native/tfliteextern.dll)
    ENDIF()
    EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll)
  ENDIF()

  INSTALL(
    FILES 
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
	
  PROJECT(Emgu.TF.Lite.Netstandard.IOS)
  BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)
  
  IF (EMGU_SIGN_FOUND)
    EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/iOS/Emgu.TF.Lite.Netstandard.dll)
  ENDIF()

  INSTALL(
    FILES 
    ${LIBRARY_OUTPUT_PATH}/iOS/Emgu.TF.Lite.Netstandard.xml
    ${LIBRARY_OUTPUT_PATH}/iOS/Emgu.TF.Lite.Netstandard.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
ENDIF()
ENDIF()
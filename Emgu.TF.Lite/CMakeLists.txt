# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
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
	PATTERN "*.csproj"
    PATTERN "*.projitems"
    PATTERN "*.resx"
    PATTERN "README.txt"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    PATTERN "*Unity*" EXCLUDE
    )
  
#  INSTALL(
#    DIRECTORY
#    ${CMAKE_CURRENT_SOURCE_DIR}/NetStandard
#    DESTINATION Emgu.TF.Lite
#    COMPONENT emgutf_source
#    FILES_MATCHING 
#    PATTERN "*.cs"
#    PATTERN "*.projitems"
#    PATTERN "*.shproj"
#    PATTERN "*.csproj"
#    PATTERN ".svn" EXCLUDE
#    PATTERN "obj" EXCLUDE
#    PATTERN "CMakeFiles" EXCLUDE
#    PATTERN "x64" EXCLUDE #exclude build artifacts from Visual Studio build process
#    PATTERN "x86" EXCLUDE #exclude build artifacts from Visual Studio build process
#    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
#    )
  
  IF (DOTNET_FOUND)
    PROJECT(Emgu.TF.Lite)
    #MESSAGE("In Emgu.TF.Lite.Netstandard project now. HAVE_WINDESKTOP_X64: ${HAVE_WINDESKTOP_X64}")
    BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/NetStandard/Emgu.TF.Lite.csproj" "" ALL)
    
    IF (EMGU_SIGN_FOUND)
      IF (WIN32 AND HAVE_WINDESKTOP_X64_LITE)
        EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/runtimes/win-x64/native/tfliteextern.dll)
      ENDIF()
  	IF (WIN32 AND HAVE_WINDESKTOP_X86_LITE)
        EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/runtimes/win-x86/native/tfliteextern.dll)
      ENDIF()
      #Binary signing should be done during project build time
      #EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Lite.dll)
    ENDIF()
  
    INSTALL(
      DIRECTORY
      ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF.Lite
      DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
      COMPONENT emgutf_binary)
  	
  ENDIF()
ENDIF()

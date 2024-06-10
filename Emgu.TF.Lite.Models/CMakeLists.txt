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
    PATTERN "*.projitems"
    PATTERN "*.shproj"
    PATTERN "*.csproj"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "x64" EXCLUDE #exclude build artifacts from Visual Studio build process
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.Models.Netstandard
    DESTINATION .
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
    PROJECT(Emgu.TF.Lite.Models.Netstandard)

    BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)

    # Binary signing should be done during project build time.
    #IF (EMGU_SIGN_FOUND)
    #  EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF.Lite.Models/AnyCPU/Release/netstandard2.0/Emgu.TF.Lite.Models.dll)
    #  EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF.Lite.Models/AnyCPU/Release/net8.0-ios/Emgu.TF.Lite.Models.dll)
    #  EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF.Lite.Models/AnyCPU/Release/net8.0-maccatalyst/Emgu.TF.Lite.Models.dll)
    #ENDIF()

    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard)

    INSTALL(
      DIRECTORY 
      ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF.Lite.Models
      DESTINATION lib
      COMPONENT emgutf_binary)
  ENDIF()
ENDIF()



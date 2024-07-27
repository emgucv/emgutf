# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------
IF(HAVE_FULL_BUILD)

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
#    DESTINATION Emgu.TF
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
#  MESSAGE(STATUS "Adding Emgu.TF project")
  PROJECT(Emgu.TF)
  
  BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/NetStandard/Emgu.TF.csproj" "" ALL)

  INSTALL(
    DIRECTORY
    ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)

ENDIF()
ENDIF()

# --------------------------------------------------------
#  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------


PROJECT(Emgu.TF.Protobuf)

#MESSAGE("In protobug project now. HAVE_WINDESKTOP_X64: ${HAVE_WINDESKTOP_X64}")

INSTALL(
  DIRECTORY
  ${CMAKE_CURRENT_SOURCE_DIR}
  DESTINATION .
  COMPONENT emgutf_source
FILES_MATCHING 
PATTERN "*.cs"
PATTERN "*.projitems"
PATTERN "*.shproj"
PATTERN ".svn" EXCLUDE
PATTERN "obj" EXCLUDE
PATTERN "CMakeFiles" EXCLUDE
PATTERN "${PROJECT_NAME}.dir" EXCLUDE
)

IF(HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64)

#MESSAGE("Adding protobuf project")
BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Protobuf/Emgu.TF.Protobuf.csproj" "" ALL)

INSTALL(
FILES 
${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
COMPONENT emgutf_binary)

ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.World)

ENDIF()


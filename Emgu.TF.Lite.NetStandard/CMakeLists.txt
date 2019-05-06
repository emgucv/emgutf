# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

PROJECT(Emgu.TF.Lite.Netstandard)

#MESSAGE("In Emgu.TF.Lite.Netstandard project now. HAVE_WINDESKTOP_X64: ${HAVE_WINDESKTOP_X64}")

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
	PATTERN "x86" EXCLUDE #exclude build artifacts from Visual Studio build process
	PATTERN "${PROJECT_NAME}.dir" EXCLUDE
)

IF(WIN32 OR APPLE)
  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.Netstandard.csproj" "" ALL)

  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND dotnet restore Emgu.TF.Lite.Netstandard.csproj
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")

  INSTALL(
  FILES 
  ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
  ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
  DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
  COMPONENT emgutf_binary)

  IF(HAVE_WINDESKTOP)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.World)
  ENDIF()

  IF(HAVE_OSX)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.World.Mac)
  ENDIF()
ENDIF()


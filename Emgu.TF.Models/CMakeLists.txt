# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

PROJECT(Emgu.TF.Models.Net45)

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

BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)


INSTALL(
  FILES 
  ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
  ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
  DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
  COMPONENT emgutf_binary)

ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Net45)
  

PROJECT(Emgu.TF.Models.Netstandard)

INSTALL(
  DIRECTORY
  ${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}
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

  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)

  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND dotnet restore ${PROJECT_NAME}.csproj
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}")

  INSTALL(
  FILES 
  ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
  ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
  DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
  COMPONENT emgutf_binary)


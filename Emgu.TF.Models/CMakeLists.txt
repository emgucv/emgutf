# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

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

#IF(HAVE_WINDESKTOP)
#  PROJECT(Emgu.TF.Models.Net)
#  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)  
#  INSTALL(
#    FILES 
#    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
#    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
#    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
#    COMPONENT emgutf_binary)
#  
#  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)
#ENDIF()

IF(HAVE_ANDROID)
  PROJECT(Emgu.TF.Models.Android)
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
  #MESSAGE("+++++++++++++++++++++ ${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj")
  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "${EXTRA_BUILD_FLAGS}" ALL)
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
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard Emgu.TF.Runtime.Android)
ENDIF()

IF (NOT "${DOTNET_EXECUTABLE}" STREQUAL "DOTNET_EXECUTABLE-NOTFOUND")
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
  
  BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)
  
  # ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
  #	PRE_BUILD
  #	COMMAND dotnet restore ${PROJECT_NAME}.csproj
  #	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}")
  
  INSTALL(
    FILES 
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
	
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)
ENDIF()

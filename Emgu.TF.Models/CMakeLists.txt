# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_FULL_BUILD)

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
    ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Models.Netstandard
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
  PROJECT(Emgu.TF.Models.Netstandard)
  
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

IF(HAVE_OSX)
  PROJECT(Emgu.TF.Models.Mac)
  
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
  
  IF(APPLE)
    BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)
    INSTALL(
      FILES 
      ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
      ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
      DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
      COMPONENT emgutf_binary)
    IF(TARGET Emgu.TF.Models.Netstandard)
      ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Models.Netstandard)
    ENDIF()
  ENDIF()
  

ENDIF()

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
  BUILD_CSPROJ_IN_SOLUTION(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/../Solution/Android/Emgu.TF.Android.sln" "Emgu.TF.Models.Android" "${EXTRA_BUILD_FLAGS}" ALL)
  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
    PRE_BUILD
    COMMAND ${DOTNET_EXECUTABLE} restore ${PROJECT_NAME}.csproj
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}")
  
  INSTALL(
    FILES 
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
  
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)
  
  IF(TARGET Emgu.TF.Models.Netstandard)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Models.Netstandard)
  ENDIF()
  
  IF(TARGET Emgu.TF.Protobuf.Netstandard)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Protobuf.Netstandard)
  ENDIF()
  
ENDIF()

ENDIF()
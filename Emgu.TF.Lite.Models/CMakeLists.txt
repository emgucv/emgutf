# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
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
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard)
    INSTALL(
      FILES 
      ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Lite.Models.Netstandard.dll
      ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Lite.Models.Netstandard.xml
      DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
      COMPONENT emgutf_binary)
  ENDIF()
ENDIF()

IF(HAVE_OSX_LITE)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.Models.Mac
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
  INSTALL(
    FILES 
    ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Lite.Models.Mac.xml
    ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Lite.Models.Mac.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
  
  IF(APPLE)
    PROJECT(Emgu.TF.Lite.Models.Mac)
    BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)    
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard)
  ENDIF()
ENDIF()

IF(HAVE_ANDROID_LITE)
  PROJECT(Emgu.TF.Lite.Models.Android)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}"
    DESTINATION Emgu.TF.Lite.Models/${PROJECT_NAME}/
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "*.cs"
    PATTERN "*.csproj"
    PATTERN "*.config"
    PATTERN "*.shproj"
    PATTERN "*.Xamarin"
    PATTERN "*.projitems"
    PATTERN "*.axml"
    PATTERN "*.xaml"
    PATTERN "*.xml"
    PATTERN "*.txt"
    PATTERN "*.png"
    PATTERN "*.dll"
    PATTERN "*.json"
    PATTERN ".git" EXCLUDE
    PATTERN "Debug" EXCLUDE
    PATTERN "Release" EXCLUDE  
    )
  BUILD_CSPROJ_IN_SOLUTION(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/../Solution/Android/Emgu.TF.Lite.Android.sln" "Emgu.TF.Lite.Models.Android" "${EXTRA_BUILD_FLAGS}" ALL)	
  #BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)
  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
    PRE_BUILD
    COMMAND ${MSBUILD_EXECUTABLE} /t:restore ${PROJECT_NAME}.csproj
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}")
  
  INSTALL(
    FILES 
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
  
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard)
ENDIF()

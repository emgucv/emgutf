# --------------------------------------------------------
#  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.
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
    
    IF (EMGU_SIGN_FOUND)
      EMGU_SIGN_BINARY(${PROJECT_NAME}  ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Lite.Models.dll)
    ENDIF()

    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard)

    INSTALL(
      FILES 
      ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Lite.Models.dll
      ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Lite.Models.xml
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
    ${LIBRARY_OUTPUT_PATH}/macos/Emgu.TF.Lite.Models.xml
    ${LIBRARY_OUTPUT_PATH}/macos/Emgu.TF.Lite.Models.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
  
  IF(APPLE)
    PROJECT(Emgu.TF.Lite.Models.Mac)
    MSBUILD_CSPROJ_IN_SOLUTION(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/../Solution/macos/Emgu.TF.Lite.Mac.sln" "${PROJECT_NAME}" "" ALL)    
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard)
  ENDIF()
ENDIF()

IF(HAVE_IOS_LITE)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.Models.IOS
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
	
  #IF (EMGU_SIGN_FOUND)
  #  EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/ios/Emgu.TF.Lite.Models.dll)
  #ENDIF()
  
  INSTALL(
    FILES 
    ${LIBRARY_OUTPUT_PATH}/ios/Emgu.TF.Lite.Models.xml
    ${LIBRARY_OUTPUT_PATH}/ios/Emgu.TF.Lite.Models.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
  
  IF(APPLE)
    PROJECT(Emgu.TF.Lite.Models.IOS)
    MSBUILD_CSPROJ_IN_SOLUTION(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/../Solution/iOS/Emgu.TF.Lite.Example.iOS.sln" "${PROJECT_NAME}" "" ALL)    
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard)
  ENDIF()
ENDIF()

IF(HAVE_ANDROID_LITE)

  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.Models.Android"
    DESTINATION Emgu.TF.Lite.Models/Emgu.TF.Lite.Models.Android/
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
    PATTERN "obj" EXCLUDE
    )

  IF (WIN32 OR APPLE)
    PROJECT(Emgu.TF.Lite.Models.Android)
    MSBUILD_CSPROJ_IN_SOLUTION(
      ${PROJECT_NAME} 
      "${CMAKE_CURRENT_SOURCE_DIR}/../Solution/Android/Emgu.TF.Lite.Android.sln" 
      "Emgu.TF.Lite.Models.Android" 
      "${EXTRA_BUILD_FLAGS}" 
      ALL)
    
    IF (EMGU_SIGN_FOUND)
      EMGU_SIGN_BINARY(${PROJECT_NAME}  ${LIBRARY_OUTPUT_PATH}/android/Emgu.TF.Lite.Models.dll)
    ENDIF()
    
    INSTALL(
      FILES 
      ${LIBRARY_OUTPUT_PATH}/android/Emgu.TF.Lite.Models.xml
      ${LIBRARY_OUTPUT_PATH}/android/Emgu.TF.Lite.Models.dll 
      DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
      COMPONENT emgutf_binary)
    
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard)
  ENDIF()
ENDIF()

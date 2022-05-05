# --------------------------------------------------------
#  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.
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
    IF (EMGU_SIGN_FOUND)
      EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Models.dll)
    ENDIF()
    
    INSTALL(
      FILES 
      ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Models.xml
      ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Models.dll 
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
	${LIBRARY_OUTPUT_PATH}/macos/Emgu.TF.Models.xml
	${LIBRARY_OUTPUT_PATH}/macos/Emgu.TF.Models.dll 
	DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
	COMPONENT emgutf_binary)
      
      ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)
      
      IF(TARGET Emgu.TF.Models.Netstandard)
	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Models.Netstandard)
      ENDIF()
    ENDIF()
    
    
  ENDIF()
  
  IF(HAVE_ANDROID)
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
    IF (WIN32 OR APPLE)
      PROJECT(Emgu.TF.Models.Android)
      #MESSAGE("+++++++++++++++++++++ ${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj")
      MSBUILD_CSPROJ_IN_SOLUTION(
	${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../Solution/Android/Emgu.TF.Android.sln" 
	"Emgu.TF.Models.Android" 
	"${EXTRA_BUILD_FLAGS}" 
	ALL)
      
      IF (EMGU_SIGN_FOUND)
	EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/android/Emgu.TF.Models.dll)
      ENDIF()
      
      INSTALL(
	FILES 
	${LIBRARY_OUTPUT_PATH}/android/Emgu.TF.Models.xml
	${LIBRARY_OUTPUT_PATH}/android/Emgu.TF.Models.dll 
	DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
	COMPONENT emgutf_binary)
      
      ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)
      
      IF(TARGET Emgu.TF.Models.Netstandard)
	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Models.Netstandard)
      ENDIF()
      
      #IF(TARGET Emgu.TF.Protobuf.Netstandard)
      #  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Protobuf.Netstandard)
      #ENDIF()
    ENDIF()
    
  ENDIF()
ENDIF()

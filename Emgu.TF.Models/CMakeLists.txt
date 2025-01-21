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
    PATTERN "*.projitems"
    PATTERN "*.shproj"
    PATTERN "*.csproj"
	PATTERN "Directory.Build.props"
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
    PROJECT(Emgu.TF.Models)
    
    BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/NetStandard/Emgu.TF.Models.csproj" "" ALL)
        
    #INSTALL(
    #  FILES 
    #  ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Models.xml
    #  ${LIBRARY_OUTPUT_PATH}/Emgu.TF.Models.dll 
    #  DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    #  COMPONENT emgutf_binary)

    INSTALL(
      DIRECTORY 
      ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF.Models
      DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
      COMPONENT emgutf_binary)

    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF)
  ENDIF()
  
ENDIF()

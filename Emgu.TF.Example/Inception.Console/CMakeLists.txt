# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_FULL_BUILD)
  INSTALL(
    FILES
    "${CMAKE_CURRENT_SOURCE_DIR}/Directory.Build.props"
	"${CMAKE_CURRENT_SOURCE_DIR}/Program.cs"
	"${CMAKE_CURRENT_SOURCE_DIR}/Inception.Console.csproj"
	"${CMAKE_CURRENT_SOURCE_DIR}/tulips.jpg"
    DESTINATION Emgu.TF.Example/Inception.Console/
    COMPONENT emgutf_example_source
    )

  IF (DOTNET_FOUND)
    PROJECT(Inception.Console)
    BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj" "" ALL)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF Emgu.TF.Protobuf Emgu.TF.Models)
  ENDIF()
ENDIF()

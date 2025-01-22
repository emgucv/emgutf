# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_LITE_BUILD)
  INSTALL(
    FILES
    "${CMAKE_CURRENT_SOURCE_DIR}/Directory.Build.props"
	"${CMAKE_CURRENT_SOURCE_DIR}/Program.cs"
	"${CMAKE_CURRENT_SOURCE_DIR}/Inception.Console.Lite.csproj"
	"${CMAKE_CURRENT_SOURCE_DIR}/tulips.jpg"
	"${CMAKE_CURRENT_SOURCE_DIR}/runtimeconfig.template.json"
    DESTINATION Emgu.TF.Example/Inception.Console.Lite/
    COMPONENT emgutf_example_source
    )

  IF (DOTNET_FOUND)
    PROJECT(Inception.Console.Lite)
    BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj" "" ALL)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite Emgu.TF.Lite.Models)
  ENDIF()
ENDIF()

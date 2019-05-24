# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAS_LITE_BUILD)
  PROJECT(Inception.Console.Lite.NetCore)

  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj"  "" ALL)

  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND "${DOTNET_EXECUTABLE}" restore "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj")  

  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard Emgu.TF.Lite.Models.Netstandard)
ENDIF()

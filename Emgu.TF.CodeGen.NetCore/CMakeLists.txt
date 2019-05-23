# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(WIN32 AND HAVE_WINDESKTOP_X64)
  PROJECT(Emgu.TF.CodeGen.NetCore)

  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj"  "" ALL)

  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND "${DOTNET_EXECUTABLE}" restore "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj") 

  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	POST_BUILD
	COMMAND "${PROJECT_NAME}.exe"
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../Emgu.TF/") 

  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard Emgu.TF.Protobuf.Netstandard)
ENDIF()

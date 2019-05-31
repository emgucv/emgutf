# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(WIN32 AND HAVE_WINDESKTOP_X64)
  PROJECT(Emgu.TF.CodeGen.NetCore)

  BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj"  "" ALL)

  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	POST_BUILD
	COMMAND ${CMAKE_COMMAND} -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/../lib/x64" "${CMAKE_CURRENT_SOURCE_DIR}/../Emgu.TF/x64"
	COMMAND ${DOTNET_EXECUTABLE} "${PROJECT_NAME}.dll"
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../Emgu.TF/") 

  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard Emgu.TF.Protobuf.Netstandard)
ENDIF()

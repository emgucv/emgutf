# --------------------------------------------------------
#  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(WIN32)
  PROJECT(Emgu.TF.CodeGen)

  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.CodeGen.csproj"  "" ALL)

  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../miscellaneous/nuget.exe" restore "${CMAKE_CURRENT_SOURCE_DIR}/packages.config") 
  
  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	POST_BUILD
	COMMAND "${PROJECT_NAME}.exe"
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../Emgu.TF/") 

  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.World)
ENDIF()

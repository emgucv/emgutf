# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------
IF(HAVE_FULL_BUILD)
	IF (DOTNET_FOUND)
	#  MESSAGE(STATUS "Adding Emgu.TF project")
	  PROJECT(Emgu.TF)
	  
	  BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.csproj" "" ALL)

	  INSTALL(
		DIRECTORY
		${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF
		DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
		COMPONENT emgutf_binary)
	ENDIF()
ENDIF()
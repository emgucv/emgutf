# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_OSX)

PROJECT(Emgu.TF.runtime.macos.nuget)
#IF(WIN32)
#ADD_CUSTOM_TARGET(
#   ${PROJECT_NAME} ALL
#   COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
#   WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
#)
#ELSE()
#ADD_CUSTOM_TARGET(
#   ${PROJECT_NAME} ALL
#   COMMAND mono "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
#   WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
#)
#ENDIF()
  get_filename_component(NUGET_OUTPUT_DIR ${CMAKE_CURRENT_SOURCE_DIR} DIRECTORY)

  BUILD_NUGET_PACKAGE(
    ${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF/Emgu.TF.Netstandard/Emgu.TF.Netstandard.csproj"  #csproj_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
	"${NUGET_OUTPUT_DIR}" #output_dir 
	"${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
	) 

  IF (EMGU_NUGET_SIGN_FOUND)
    EMGU_SIGN_NUGET(${PROJECT_NAME} "${NUGET_OUTPUT_DIR}/${EMGUTF_MACOS_NUGET_ID}.${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}.nupkg")
  ENDIF()

ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF)

if(ENABLE_SOLUTION_FOLDERS)
  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
endif() 

ENDIF()

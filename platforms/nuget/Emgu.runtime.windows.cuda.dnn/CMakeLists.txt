# --------------------------------------------------------
#  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (WIN32 AND (HAVE_WINDESKTOP_X64_CUDA) )

PROJECT(Emgu.runtime.windows.cuda.dnn.nuget)
#ADD_CUSTOM_TARGET(
#   ${PROJECT_NAME} ALL
#   COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
#   WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
#)
  BUILD_NUGET_PACKAGE(
    ${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF/Emgu.TF.Netstandard/Emgu.TF.Netstandard.csproj"  #csproj_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/../" #output_dir 
	"${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
	) 
ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.runtime.windows.cuda.rt.nuget)

if(ENABLE_SOLUTION_FOLDERS)
  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
endif() 

ENDIF()

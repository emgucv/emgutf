# --------------------------------------------------------
#  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

#IF (HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64 OR HAVE_ANDROID OR HAVE_OSX)
IF (TARGET Emgu.TF.Netstandard)
  PROJECT(Emgu.TF.nuget)

  BUILD_NUGET_PACKAGE(
    ${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF/Emgu.TF.Netstandard/Emgu.TF.Netstandard.csproj"  #csproj_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/../" #output_dir 
	"${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
	) 

  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)
  
  if(ENABLE_SOLUTION_FOLDERS)
    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
  endif() 
  
  #IF (WIN32 AND (HAVE_WINDESKTOP_X64_CUDA) )
  #	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Dnn.nuget)
  #	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Cufft.nuget)
  #ENDIF()
ENDIF()

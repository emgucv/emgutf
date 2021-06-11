# --------------------------------------------------------
#  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (TARGET Emgu.TF.Protobuf.Netstandard)
  
  PROJECT(Emgu.TF.Protobuf.nuget)

  BUILD_NUGET_PACKAGE(
    ${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF.Protobuf/Emgu.TF.Protobuf.Netstandard/Emgu.TF.Protobuf.Netstandard.csproj"  #csproj_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/../" #output_dir 
	"${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
	) 

  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Protobuf.Netstandard)
  
  if(ENABLE_SOLUTION_FOLDERS)
    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
  endif() 
  
ENDIF()

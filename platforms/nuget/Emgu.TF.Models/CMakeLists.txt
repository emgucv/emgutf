# --------------------------------------------------------
#  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (TARGET Emgu.TF.Models.Netstandard)  
  PROJECT(Emgu.TF.Models.nuget)

  BUILD_NUGET_PACKAGE(
    ${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF.Models/Emgu.TF.Models.Netstandard/Emgu.TF.Models.Netstandard.csproj"  #csproj_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/../" #output_dir 
	"${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
	)
	
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Models.Netstandard Emgu.TF.nuget)
  
  IF(TARGET Emgu.TF.Models.Mac)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Models.Mac)
  ENDIF()

  IF(TARGET Emgu.TF.Models.Android)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Models.Android)
  ENDIF()
  
  if(ENABLE_SOLUTION_FOLDERS)
    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
  endif() 
ENDIF()

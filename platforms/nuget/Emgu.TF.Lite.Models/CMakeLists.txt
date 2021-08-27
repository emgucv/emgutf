# --------------------------------------------------------
#  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF ( TARGET Emgu.TF.Lite.Models.Netstandard )
  
  PROJECT(Emgu.TF.Lite.Models.nuget)
  
  BUILD_NUGET_PACKAGE(
    ${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF.Lite.Models/Emgu.TF.Lite.Models.Netstandard/Emgu.TF.Lite.Models.Netstandard.csproj" #csproj_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/../" #output_dir 
	"${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
	)

  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Models.Netstandard Emgu.TF.Lite.nuget)
  
  IF(TARGET Emgu.TF.Lite.Models.Mac)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Models.Mac)
  ENDIF()

  IF(TARGET Emgu.TF.Lite.Models.Android)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Models.Android)
  ENDIF()
  
  IF(TARGET Emgu.TF.Lite.Models.IOS)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Models.IOS)
  ENDIF()
  
  if(ENABLE_SOLUTION_FOLDERS)
    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
  endif() 
  
ENDIF()

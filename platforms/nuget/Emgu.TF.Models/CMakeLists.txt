# --------------------------------------------------------
#  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (TARGET Emgu.TF.Models.Netstandard)  
  PROJECT(Emgu.TF.Models.nuget)

  get_filename_component(NUGET_OUTPUT_DIR ${CMAKE_CURRENT_SOURCE_DIR} DIRECTORY)

  BUILD_NUGET_PACKAGE(
    ${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF.Models/Emgu.TF.Models.Netstandard/Emgu.TF.Models.Netstandard.csproj"  #csproj_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
	"${NUGET_OUTPUT_DIR}" #output_dir 
	"${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
	)

  IF (EMGU_NUGET_SIGN_FOUND)
    EMGU_SIGN_NUGET(${PROJECT_NAME} "${NUGET_OUTPUT_DIR}/${EMGUTF_MODEL_NUGET_ID}.${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}.nupkg")
  ENDIF()

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

# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_WINDESKTOP_LITE)

PROJECT(Emgu.TF.Lite.runtime.windows.nuget)

  get_filename_component(NUGET_OUTPUT_DIR ${CMAKE_CURRENT_SOURCE_DIR} DIRECTORY)
  BUILD_NUGET_PACKAGE(
    ${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF.Lite/NetStandard/Emgu.TF.Lite.csproj"  #csproj_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
	"${NUGET_OUTPUT_DIR}" #output_dir 
	"${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
	) 

  IF (EMGU_NUGET_SIGN_FOUND)
    EMGU_SIGN_NUGET(${PROJECT_NAME} "${NUGET_OUTPUT_DIR}/${EMGUTF_LITE_WINDOWS_NUGET_ID}.${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}.nupkg")
  ENDIF()
  
  IF (TARGET Emgu.runtime.windows.msvc.rt.x64.nuget)
  	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.runtime.windows.msvc.rt.x64.nuget)
  ENDIF()
  
  IF (TARGET Emgu.runtime.windows.msvc.rt.x86.nuget)
  	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.runtime.windows.msvc.rt.x86.nuget)
  ENDIF()
  
#ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.nuget)

if(ENABLE_SOLUTION_FOLDERS)
  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
endif() 

ENDIF()

# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_MAUI_LITE)

  PROJECT(Emgu.TF.Lite.runtime.maui.nuget)

  get_filename_component(NUGET_OUTPUT_DIR ${CMAKE_CURRENT_SOURCE_DIR} DIRECTORY)

    BUILD_NUGET_PACKAGE(
      ${PROJECT_NAME} 
      "${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF.Lite/Emgu.TF.Lite.Netstandard/Emgu.TF.Lite.Netstandard.csproj"  #csproj_file 
      "${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
      "${NUGET_OUTPUT_DIR}" #output_dir 
      "${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
      ) 
	  
    IF (EMGU_NUGET_SIGN_FOUND)
      EMGU_SIGN_NUGET(${PROJECT_NAME} "${NUGET_OUTPUT_DIR}/${EMGUTF_LITE_MAUI_NUGET_ID}.${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}.nupkg")
    ENDIF()
    
  if(ENABLE_SOLUTION_FOLDERS)
    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
  endif() 
  
  IF(TARGET Emgu.TF.Lite.Platform.Maui.IOS)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Platform.Maui.IOS)
  ENDIF()
  
  #IF (TARGET Emgu.TF.Lite.Platform.Maui.Mac)
  #  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Platform.Maui.Mac)
  #ENDIF()
  
  IF (TARGET Emgu.TF.Lite.Platform.Maui.Android)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Platform.Maui.Android)
  ENDIF()
  
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Platform.Maui.UI)

  #ADD_DEPENDENCIES(${PROJECT_NAME} tfliteextern)
  
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite)
  
  #IF (EMGU_NUGET_SIGN_FOUND)
  #  EMGU_SIGN_NUGET(${PROJECT_NAME} "${NUGET_OUTPUT_DIR}/${EMGUCV_MAUI_NUGET_ID}.${CPACK_PACKAGE_VERSION}.nupkg")
  #ENDIF()

  #if(ENABLE_SOLUTION_FOLDERS)
  #  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "nuget")
  #endif() 

ENDIF()

# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (WIN32 AND (HAVE_WINDESKTOP_X64_CUDA))

  PROJECT(Emgu.TF.runtime.windows.cuda.cufft.nuget)
  ADD_CUSTOM_TARGET(
     ${PROJECT_NAME} ALL
     #COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/../../../tensorflow/tensorflow/contrib/cmake/nuget/Cufft/Package.nuspec ${CMAKE_CURRENT_SOURCE_DIR}
     COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec  -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
     WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
  )
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)

  if(ENABLE_SOLUTION_FOLDERS)
    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
  endif() 
ENDIF()

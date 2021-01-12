# --------------------------------------------------------
#  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_WINDESKTOP_X64 OR HAVE_WINDESKTOP_X64_CUDA)

PROJECT(Emgu.TF.runtime.windows.nuget)
ADD_CUSTOM_TARGET(
   ${PROJECT_NAME} ALL
   COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack ${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
   WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.nuget)

IF (TARGET Emgu.runtime.windows.cuda.solver.mg.nuget)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.runtime.windows.cuda.solver.mg.nuget)
ENDIF()
IF (TARGET Emgu.runtime.windows.cuda.solver.nuget)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.runtime.windows.cuda.solver.nuget)
ENDIF()
IF (TARGET Emgu.runtime.windows.cuda.dnn.adv.nuget)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.runtime.windows.cuda.dnn.adv.nuget)
ENDIF()
IF (TARGET Emgu.runtime.windows.cuda.dnn.ops.nuget)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.runtime.windows.cuda.dnn.ops.nuget)
ENDIF()
IF (TARGET Emgu.runtime.windows.cuda.dnn.cnn.nuget)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.runtime.windows.cuda.dnn.cnn.nuget)
ENDIF()
IF (TARGET Emgu.runtime.windows.cuda.fft.nuget)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.runtime.windows.cuda.fft.nuget)
ENDIF()
IF (TARGET Emgu.TF.runtime.windows.cuda.blas.nuget)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.runtime.windows.cuda.blas.nuget)
ENDIF()


if(ENABLE_SOLUTION_FOLDERS)
  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
endif() 

ENDIF()

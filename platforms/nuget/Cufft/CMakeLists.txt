# --------------------------------------------------------
#  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (WIN32 AND (HAVE_WINDESKTOP_X64_CUDA) )

PROJECT(Emgu.Cufft.nuget)
ADD_CUSTOM_TARGET(
   ${PROJECT_NAME}
   COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/../../../tensorflow/tensorflow/contrib/cmake/nuget/Cufft/Package.nuspec ${CMAKE_CURRENT_SOURCE_DIR}
   COMMAND ${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe pack Package.nuspec
)
ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.World)

ENDIF()

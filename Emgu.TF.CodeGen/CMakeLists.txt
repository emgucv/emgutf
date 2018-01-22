# --------------------------------------------------------
#  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(WIN32)
  PROJECT(Emgu.TF.CodeGen)

  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.CodeGen.csproj"  "" ALL)
  ADD_DEPENDENCIES(Emgu.TF.CodeGen Emgu.TF.World)
ENDIF()

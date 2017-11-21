# --------------------------------------------------------
#  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(WIN32)
  PROJECT(Emgu.TF.CodeGen)
  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.CodeGen.csproj"  "" ALL)
ENDIF()

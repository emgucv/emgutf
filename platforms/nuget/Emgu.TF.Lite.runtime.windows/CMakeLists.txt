# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_WINDESKTOP_X64_LITE)

PROJECT(Emgu.TF.Lite.runtime.windows.nuget)
ADD_CUSTOM_TARGET(
   ${PROJECT_NAME} ALL
   COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack ${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
)

ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.nuget)

if(ENABLE_SOLUTION_FOLDERS)
  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
endif() 

ENDIF()

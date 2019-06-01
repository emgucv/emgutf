# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (WIN32 AND HAVE_WINDESKTOP )

PROJECT(Emgu.TF.Models.nuget)
ADD_CUSTOM_TARGET(
   ${PROJECT_NAME}
   COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
)
ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Models.Net45 Emgu.TF.Models.Netstandard Emgu.TF.nuget)

if(ENABLE_SOLUTION_FOLDERS)
  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
endif() 

ENDIF()

# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (WIN32 AND (HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64) )

PROJECT(Emgu.TF.Protobuf.nuget)
ADD_CUSTOM_TARGET(
   ${PROJECT_NAME}
   COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec  -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
)
ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Protobuf.Netstandard)

if(ENABLE_SOLUTION_FOLDERS)
  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
endif() 

ENDIF()

# --------------------------------------------------------
#  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_OSX)

PROJECT(Emgu.TF.runtime.macos.nuget)
IF(WIN32)
ADD_CUSTOM_TARGET(
   ${PROJECT_NAME} ALL
   COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
   WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
)
ELSE()
ADD_CUSTOM_TARGET(
   ${PROJECT_NAME} ALL
   COMMAND mono "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
   WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
)
ENDIF()

ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.nuget)

if(ENABLE_SOLUTION_FOLDERS)
  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
endif() 

ENDIF()

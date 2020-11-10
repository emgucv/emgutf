# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF ( HAVE_IOS_LITE )

PROJECT(Emgu.TF.Lite.runtime.ios.nuget)

ADD_CUSTOM_TARGET(
  ${PROJECT_NAME} ALL
  COMMAND ${DOTNET_EXECUTABLE} pack -c ${DEFAULT_CS_CONFIG} "${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF.Lite/Emgu.TF.Lite.Netstandard.IOS/Emgu.TF.Lite.Netstandard.IOS.csproj" -p:NuspecFile="${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" -o "${CMAKE_CURRENT_SOURCE_DIR}/../"
  WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
  )
ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.nuget)

IF(TARGET Emgu.TF.Lite.Runtime.iOS)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Runtime.iOS)
ENDIF()

if(ENABLE_SOLUTION_FOLDERS)
  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
endif() 

ENDIF()

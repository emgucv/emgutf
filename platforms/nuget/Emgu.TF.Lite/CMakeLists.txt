# --------------------------------------------------------
#  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF ( TARGET Emgu.TF.Lite.Netstandard )
  
  PROJECT(Emgu.TF.Lite.nuget)

    #ADD_CUSTOM_TARGET(
    #  ${PROJECT_NAME} ALL
    #  COMMAND ${DOTNET_EXECUTABLE} pack -c ${DEFAULT_CS_CONFIG} "${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF.Lite/Emgu.TF.Lite.Netstandard/Emgu.TF.Lite.Netstandard.csproj" -p:NuspecFile="${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" -o "${CMAKE_CURRENT_SOURCE_DIR}/../"
    #  WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    #  )

  BUILD_NUGET_PACKAGE(
    ${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF.Lite/Emgu.TF.Lite.Netstandard/Emgu.TF.Lite.Netstandard.csproj" #csproj_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/../" #output_dir 
	"${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
	)
    
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard)

  IF (TARGET Emgu.TF.Lite.Netstandard.IOS)
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Netstandard.IOS)
  ENDIF()
  
  if(ENABLE_SOLUTION_FOLDERS)
    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
  endif() 
  
ENDIF()

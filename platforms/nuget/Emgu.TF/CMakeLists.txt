# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

#IF (HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64 OR HAVE_ANDROID OR HAVE_OSX)
IF (TARGET Emgu.TF.Netstandard)
  PROJECT(Emgu.TF.nuget)
  #  IF (WIN32)
  #    ADD_CUSTOM_TARGET(
  #      ${PROJECT_NAME} ALL 
  #      COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
  #      )
  #  ELSE()
  #    ADD_CUSTOM_TARGET(
  #      ${PROJECT_NAME} ALL 
  #      COMMAND mono "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
  #      )
  #  ENDIF()
  
  ADD_CUSTOM_TARGET(
    ${PROJECT_NAME} ALL
    COMMAND ${DOTNET_EXECUTABLE} pack -c ${DEFAULT_CS_CONFIG} "${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF/Emgu.TF.Netstandard/Emgu.TF.Netstandard.csproj" -p:NuspecFile="${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" -o "${CMAKE_CURRENT_SOURCE_DIR}/../"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    )
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)
  
  if(ENABLE_SOLUTION_FOLDERS)
    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
  endif() 
  
  #IF (WIN32 AND (HAVE_WINDESKTOP_X64_CUDA) )
  #	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Dnn.nuget)
  #	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Cufft.nuget)
  #ENDIF()
ENDIF()

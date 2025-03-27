# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (WIN32 AND (HAVE_WINDESKTOP_X64 OR HAVE_WINDESKTOP_X64_LITE))

  PROJECT(Emgu.runtime.windows.msvc.rt.x64.nuget)

  get_filename_component(NUGET_OUTPUT_DIR ${CMAKE_CURRENT_SOURCE_DIR} DIRECTORY)

  BUILD_NUGET_PACKAGE(
    ${PROJECT_NAME} 
	"${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF/NetStandard/Emgu.TF.csproj"  #csproj_file 
	"${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" #nuspec_file 
	"${NUGET_OUTPUT_DIR}/" #output_dir 
	"${CMAKE_CURRENT_SOURCE_DIR}" #working_dir
	) 
  IF (EMGU_NUGET_SIGN_FOUND)
    EMGU_SIGN_NUGET(${PROJECT_NAME} "${NUGET_OUTPUT_DIR}/${EMGU_WINDOWS_MSVCRT_X64_NUGET_ID}.${EMGU_WINDOWS_MSVCRT_NUGET_VERSION}.nupkg")
  ENDIF()

  if(ENABLE_SOLUTION_FOLDERS)
    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
  endif() 

ENDIF()

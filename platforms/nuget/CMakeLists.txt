# --------------------------------------------------------
#  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (WIN32 AND (HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64) )

add_subdirectory(Dnn)
add_subdirectory(Cufft)

PROJECT(Emgu.TF.nuget)
ADD_CUSTOM_TARGET(
   ${PROJECT_NAME}
   COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/"
)

ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.World)

if(ENABLE_SOLUTION_FOLDERS)
  set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
endif() 

IF (WIN32 AND (HAVE_WINDESKTOP_X64_CUDA) )
	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.Dnn.nuget)
	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.Cufft.nuget)
ENDIF()

add_subdirectory(Models)
add_subdirectory(Protobuf)
ENDIF()

IF ( HAVE_ANDROID )
	add_subdirectory(Android)
ENDIF()

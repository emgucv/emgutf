# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF((HAVE_WINDESKTOP_X64 OR HAVE_LINUX) AND DOTNET_FOUND)
  PROJECT(Emgu.TF.CodeGen)
  
  IF(HAVE_WINDESKTOP_X64 AND WIN32)
    #Force to run in 64-bit for windows.
    BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj" "/p:Platform=x64" ALL)
  else()
    BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}.csproj" "" ALL)
  ENDIF()
  IF(HAVE_WINDESKTOP_X64 AND WIN32)
    ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
      POST_BUILD
      COMMAND ${DOTNET_EXECUTABLE} "${PROJECT_NAME}.dll"
      WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../Emgu.TF/")
  ELSEIF(APPLE)
    ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
      POST_BUILD
      COMMAND ${DOTNET_EXECUTABLE} "${PROJECT_NAME}.dll"
      WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../Emgu.TF/")  
  ELSEIF(HAVE_LINUX AND (NOT (WIN32 OR APPLE)))
    IF(HAVE_LINUX_ARM) #RASPBIAN
      SET(CODE_GEN_COMMAND LD_PRELOAD=/usr/lib/arm-linux-gnueabihf/libatomic.so.1 ${DOTNET_EXECUTABLE} "${PROJECT_NAME}.dll")
    ELSE()
      SET(CODE_GEN_COMMAND ${DOTNET_EXECUTABLE} "${PROJECT_NAME}.dll")
    ENDIF()  
    ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
      POST_BUILD
      COMMAND ${CODE_GEN_COMMAND}
      WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../Emgu.TF/")
  ENDIF()
  
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF Emgu.TF.Protobuf)
ENDIF()

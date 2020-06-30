# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_WINDESKTOP)
#  PROJECT(Example.CVInterop)
#  BUILD_CSPROJ_IN_SOLUTION(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/../../Solution/Windows.Desktop/Emgu.TF.Example.sln" "CvInterop" "" ALL)
#  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
#	PRE_BUILD
# 	COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../miscellaneous/nuget.exe" restore "Emgu.TF.Example.sln"
#	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../../Solution/Windows.Desktop/")

  #copy the test image for this example
  SET(EXAMPLE_INCEPTION_DATA_FILES
  "${CMAKE_CURRENT_SOURCE_DIR}/dog416.png"
  )
  
  FILE(GLOB SRC_Example_CVInterop RELATIVE ${CMAKE_CURRENT_SOURCE_DIR} "*.cs") 

  INSTALL(FILES
    ${SRC_Example_CVInterop}
	${EXAMPLE_INCEPTION_DATA_FILES}
    ${CMAKE_CURRENT_SOURCE_DIR}/CVInterop.NetCore.csproj
    #${CMAKE_CURRENT_SOURCE_DIR}/App.config
    DESTINATION Emgu.TF.Example/CVInterop COMPONENT emgutf_example_source)
  INSTALL(FILES
    ${CMAKE_CURRENT_SOURCE_DIR}/Properties/AssemblyInfo.cs
    DESTINATION Emgu.TF.Example/CVInterop/Properties COMPONENT emgutf_example_source)
  
  #ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Protobuf.Netstandard)
ENDIF()

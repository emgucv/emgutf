# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

#copy the test image for this example
SET(EXAMPLE_INCEPTION_DATA_FILES
  "${CMAKE_CURRENT_SOURCE_DIR}/dog416.png"
  )
INSTALL(FILES 
  ${EXAMPLE_INCEPTION_DATA_FILES}
  DESTINATION Emgu.TF.Example/CVInterop COMPONENT emgutf_example_source)

IF(HAVE_WINDESKTOP)
  PROJECT(Example.CVInterop)
  
  BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/CvInterop.csproj" "" ALL)
  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND ${MSBUILD_EXECUTABLE} /t:restore CvInterop.csproj
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")
  
  INSTALL(FILES
    ${SRC_Example_CVInterop}
    ${CMAKE_CURRENT_SOURCE_DIR}/CVInterop.csproj
    ${CMAKE_CURRENT_SOURCE_DIR}/App.config
    DESTINATION Emgu.TF.Example/CVInterop COMPONENT emgutf_example_source)
  INSTALL(FILES
    ${CMAKE_CURRENT_SOURCE_DIR}/Properties/AssemblyInfo.cs
    DESTINATION Emgu.TF.Example/CVInterop/Properties COMPONENT emgutf_example_source)
  
  #INSTALL(FILES 
  #${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.${CS_EXECUTABLE_EXTENSION} 
  #DESTINATION bin COMPONENT emgutf_example_binary)
ENDIF()

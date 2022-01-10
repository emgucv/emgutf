# --------------------------------------------------------
#  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_WINDESKTOP)
  FILE(GLOB SRC_Example_CVInterop RELATIVE ${CMAKE_CURRENT_SOURCE_DIR} "*.cs") 

  INSTALL(FILES
    ${SRC_Example_CVInterop}
	"${CMAKE_CURRENT_SOURCE_DIR}/dog416.png"
    ${CMAKE_CURRENT_SOURCE_DIR}/CVInterop.NetCore.csproj
    #${CMAKE_CURRENT_SOURCE_DIR}/App.config
    DESTINATION Emgu.TF.Example/CVInterop COMPONENT emgutf_example_source)

ENDIF()

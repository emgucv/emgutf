# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_WINDESKTOP_LITE)
  FILE(GLOB SRC_Example_CVInterop_Lite RELATIVE ${CMAKE_CURRENT_SOURCE_DIR} "*.cs") 

  INSTALL(FILES
    ${SRC_Example_CVInterop_Lite}
	"${CMAKE_CURRENT_SOURCE_DIR}/dog416.png"
    ${CMAKE_CURRENT_SOURCE_DIR}/CVInterop.Lite.Net.csproj
    #${CMAKE_CURRENT_SOURCE_DIR}/App.config
    DESTINATION Emgu.TF.Example/CVInterop.Lite COMPONENT emgutf_example_source)

ENDIF()

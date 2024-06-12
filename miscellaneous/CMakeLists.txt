# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

#IF(HAVE_WINDESKTOP OR HAVE_WINDESKTOP_LITE)
#  #MESSAGE(STATUS "MSBUILD_EXECUTABLE_40: ${MSBUILD_EXECUTABLE_40}")
#  IF(NOT ("${MSBUILD_EXECUTABLE_150}" STREQUAL ""))
#    SET(MSBUILD_EXECUTABLE ${MSBUILD_EXECUTABLE_150})
#  ELSE()
#    MESSAGE(STATUS "Not able to find MSBUILD 150, Documentation build may not produce the right output.")
#  ENDIF()
#ENDIF()

IF("${MSBUILD_EXECUTABLE}" STREQUAL "")
  MESSAGE(STATUS "MSBUILD_EXECUTABLE NOT FOUND, skipping documentation projects.")
ELSE()
  IF(HAVE_WINDESKTOP)  
    MESSAGE(STATUS "MSBUILD_EXECUTABLE for documentation building: ${MSBUILD_EXECUTABLE}")
    PROJECT(Emgu.TF.Document.Html)
    ADD_CUSTOM_TARGET(
       ${PROJECT_NAME}
       COMMAND ${MSBUILD_EXECUTABLE} /p:Configuration=Release ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.html.shfbproj /property:HelpFileVersion=${CPACK_PACKAGE_VERSION}
       COMMAND zip -r TF.Help.zip TF.Help
       SOURCES ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.html.shfbproj
       WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}/../
    )
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF Emgu.TF.Protobuf Emgu.TF.Models)
    
    PROJECT(Emgu.TF.Documentation.chm)
    ADD_CUSTOM_TARGET(
       ${PROJECT_NAME} ALL
       COMMAND ${MSBUILD_EXECUTABLE} /p:Configuration=Release ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.html.shfbproj /property:OutputPath=. /property:HelpFileFormat=HtmlHelp1 /property:HelpFileVersion=${CPACK_PACKAGE_VERSION}
       SOURCES ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.html.shfbproj
    )
    #Making sure documentation project is build one by one
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Document.Html)
  
    INSTALL(
      FILES
      "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Documentation.chm"
      DESTINATION .
      COMPONENT emgutf_document
      )
  ENDIF()
  
  IF(HAVE_WINDESKTOP_LITE)  
    PROJECT(Emgu.TF.Lite.Document.Html)
    ADD_CUSTOM_TARGET(
       ${PROJECT_NAME}
       COMMAND ${MSBUILD_EXECUTABLE} /p:Configuration=Release ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.html.shfbproj
       SOURCES ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.html.shfbproj
    )
    
	ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite Emgu.TF.Lite.Models)

	IF(TARGET Emgu.TF.Documentation.chm)
        #If Emgu.TF documentation is also build with the same solution
		#Make sure the document project is build one by one
		ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Documentation.chm)
	ENDIF()

    PROJECT(Emgu.TF.Lite.Documentation.chm)
    ADD_CUSTOM_TARGET(
       ${PROJECT_NAME} ALL
       COMMAND ${MSBUILD_EXECUTABLE} /p:Configuration=Release ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.html.shfbproj /property:OutputPath=. /property:HelpFileFormat=HtmlHelp1
       SOURCES ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.html.shfbproj
    )
    #Making sure documentation project is build one by one
    ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Lite.Document.Html)
    
    INSTALL(
      FILES
      "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Lite.Documentation.chm"
      DESTINATION .
      COMPONENT emgutf_document
      )
  ENDIF()
ENDIF()


# --------------------------------------------------------
#  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------
IF(HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64)
PROJECT(Example.InceptionObjectRecognition)

FILE(GLOB_RECURSE SRC_Example_InceptionObjectRecognition RELATIVE ${CMAKE_CURRENT_SOURCE_DIR} "*.cs")

SET(SRC_Example_InceptionObjectRecognition_ALL)
LIST(APPEND SRC_Example_InceptionObjectRecognition_ALL ${SRC_Example_InceptionObjectRecognition})
LIST(APPEND SRC_Example_MultiboxPeopleDetection_ALL ${CMAKE_CURRENT_SOURCE_DIR}/../../Emgu.Models/FileDownloadManager.cs)
LIST(APPEND SRC_Example_MultiboxPeopleDetection_ALL ${CMAKE_CURRENT_SOURCE_DIR}/../../Emgu.Models/DownloadableFile.cs)
LIST(APPEND SRC_Example_MultiboxPeopleDetection_ALL ${CMAKE_CURRENT_SOURCE_DIR}/../../Emgu.Models/NativeImageIO.cs)
LIST(APPEND SRC_Example_InceptionObjectRecognition_ALL ${CMAKE_CURRENT_SOURCE_DIR}/../../Emgu.TF.Models/ImageIO.cs)
LIST(APPEND SRC_Example_InceptionObjectRecognition_ALL ${CMAKE_CURRENT_SOURCE_DIR}/../../Emgu.TF.Models/Inception.cs)

#IF(TARGET_ARCH_64)
#  LIST(APPEND CS_FLAGS -platform:x64)
#ELSE()
#  LIST(APPEND CS_FLAGS -platform:x86)
#ENDIF()

LIST(APPEND CS_FLAGS ${EMGU_TF_REFERENCE})

#copy the test image for this example
SET(EXAMPLE_INCEPTION_DATA_FILES
  "${CMAKE_CURRENT_SOURCE_DIR}/space_shuttle.jpg"
  )
 
FOREACH(EXAMPLE_INCEPTION_DATA_FILE ${EXAMPLE_INCEPTION_DATA_FILES})
  ADD_CS_FILE_TO_DEPLOY(${EXAMPLE_INCEPTION_DATA_FILE})
ENDFOREACH()

COMPILE_CS(${PROJECT_NAME} "winexe" "${SRC_Example_InceptionObjectRecognition_ALL}" ALL)
set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "Emgu.TF.Examples")
ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.World)

MESSAGE(STATUS "EXAMPLE_INCEPTION_DATA_FILES: ${EXAMPLE_INCEPTION_DATA_FILES}")

INSTALL(FILES
  ${SRC_Example_InceptionObjectRecognition}
  ${CMAKE_CURRENT_SOURCE_DIR}/InceptionObjectRecognition.csproj
  ${CMAKE_CURRENT_SOURCE_DIR}/App.config
  DESTINATION Emgu.TF.Example/InceptionObjectRecognition COMPONENT emgutf_example_source)
INSTALL(FILES
  ${CMAKE_CURRENT_SOURCE_DIR}/Properties/AssemblyInfo.cs
  DESTINATION Emgu.TF.Example/InceptionObjectRecognition/Properties COMPONENT emgutf_example_source)
INSTALL(FILES 
  ${EXAMPLE_INCEPTION_DATA_FILES}
  DESTINATION Emgu.TF.Example/InceptionObjectRecognition COMPONENT emgutf_example_source)

INSTALL(FILES 
${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.${CS_EXECUTABLE_EXTENSION} 
DESTINATION bin COMPONENT emgutf_example_binary)
ENDIF()

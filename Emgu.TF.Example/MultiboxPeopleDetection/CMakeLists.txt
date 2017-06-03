# --------------------------------------------------------
#  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------
IF(HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64)
PROJECT(Example.MultiboxPeopleDetection)

FILE(GLOB_RECURSE SRC_Example_MultiboxPeopleDetection RELATIVE ${CMAKE_CURRENT_SOURCE_DIR} "*.cs")
LIST(APPEND SRC_Example_MultiboxPeopleDetection ${CMAKE_CURRENT_SOURCE_DIR}/../../CommonAssemblyInfo.cs)
LIST(APPEND SRC_Example_MultiboxPeopleDetection ${CMAKE_CURRENT_SOURCE_DIR}/Properties/CommonAssemblyInfo.cs)

SET(SRC_Example_MultiboxPeopleDetection_ALL)
LIST(APPEND SRC_Example_MultiboxPeopleDetection_ALL ${SRC_Example_MultiboxPeopleDetection})
LIST(APPEND SRC_Example_MultiboxPeopleDetection_ALL ${CMAKE_CURRENT_SOURCE_DIR}/../../Emgu.TF.Models/DownloadableModels.cs)
LIST(APPEND SRC_Example_MultiboxPeopleDetection_ALL ${CMAKE_CURRENT_SOURCE_DIR}/../../Emgu.TF.Models/ImageIO.cs)
LIST(APPEND SRC_Example_MultiboxPeopleDetection_ALL ${CMAKE_CURRENT_SOURCE_DIR}/../../Emgu.TF.Models/Multibox.cs)

#IF(TARGET_ARCH_64)
#  LIST(APPEND CS_FLAGS -platform:x64)
#ELSE()
#  LIST(APPEND CS_FLAGS -platform:x86)
#ENDIF()

LIST(APPEND CS_FLAGS ${EMGU_TF_REFERENCE})

#copy the test image for this example
SET(EXAMPLE_PEOPLE_DETECTION_DATA_FILES
  "${CMAKE_CURRENT_SOURCE_DIR}/../../tensorflow/tensorflow/examples/multibox_detector/data/surfers.jpg"
  )
 
FOREACH(EXAMPLE_PEOPLE_DETECTION_DATA_FILE ${EXAMPLE_PEOPLE_DETECTION_DATA_FILES})
  ADD_CS_FILE_TO_DEPLOY(${EXAMPLE_PEOPLE_DETECTION_DATA_FILE})
ENDFOREACH()


COMPILE_CS(${PROJECT_NAME} "winexe" "${SRC_Example_MultiboxPeopleDetection_ALL}" ALL)
set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "Emgu.TF.Examples")
ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.World)

MESSAGE(STATUS "EXAMPLE_PEOPLE_DETECTION_DATA_FILES: ${EXAMPLE_PEOPLE_DETECTION_DATA_FILES}")

INSTALL(FILES
  ${SRC_Example_MultiboxPeopleDetection}
  ${CMAKE_CURRENT_SOURCE_DIR}/MultiboxPeopleDetection.csproj
  ${CMAKE_CURRENT_SOURCE_DIR}/App.config
  ${CMAKE_CURRENT_SOURCE_DIR}/Properties/AssemblyInfo.cs
  DESTINATION Emgu.TF.Example/MultiboxPeopleDetection COMPONENT emgutf_example_source)
INSTALL(FILES 
  ${EXAMPLE_PEOPLE_DETECTION_DATA_FILES}
  DESTINATION tensorflow/tensorflow/examples/multibox_detector/data COMPONENT emgutf_example_source)

INSTALL(FILES 
${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.${CS_EXECUTABLE_EXTENSION} 
DESTINATION bin COMPONENT emgutf_example_binary)
ENDIF()

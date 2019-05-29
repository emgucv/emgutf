# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

PROJECT(Emgu.TF.Protobuf.Netstandard)

#MESSAGE("In protobug project now. HAVE_WINDESKTOP_X64: ${HAVE_WINDESKTOP_X64}")

INSTALL(
  DIRECTORY
  ${CMAKE_CURRENT_SOURCE_DIR}
  DESTINATION .
  COMPONENT emgutf_source
	FILES_MATCHING 
	PATTERN "*.cs"
	PATTERN "*.projitems"
	PATTERN "*.shproj"
	PATTERN "*.csproj"
	PATTERN ".svn" EXCLUDE
	PATTERN "obj" EXCLUDE
	PATTERN "CMakeFiles" EXCLUDE
	PATTERN "x64" EXCLUDE #exclude build artifacts from Visual Studio build process
	PATTERN "x86" EXCLUDE #exclude build artifacts from Visual Studio build process
	PATTERN "${PROJECT_NAME}.dir" EXCLUDE
)

#MESSAGE("Adding protobuf project")

BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)

#ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
#	PRE_BUILD
#	COMMAND ${DOTNET_EXECUTABLE} restore ${PROJECT_NAME}.csproj
#	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}")

IF(WIN32 OR APPLE)
  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND wget https://www.nuget.org/api/v2/package/Google.Protobuf.Tools/3.7.0 -O protobuf.tools.zip
	COMMAND unzip -u protobuf.tools.zip -d protobuf.tools
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")
ENDIF()

SET(PROTO_SOURCE)
FILE(GLOB PROTO_SOURCE_FILES_FRAME_WORK RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/framework/" "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/framework/*.proto")
FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_FRAME_WORK} )
	#MESSAGE(STATUS "File: ${PROTO_SOURCE_FILE}")
  IF(WIN32)
	LIST(APPEND PROTO_SOURCE "tensorflow\\core\\framework\\${PROTO_SOURCE_FILE}" )
  ELSE()
    LIST(APPEND PROTO_SOURCE "tensorflow/core/framework/${PROTO_SOURCE_FILE}" )
  ENDIF()
ENDFOREACH()

FILE(GLOB PROTO_SOURCE_FILES_PROTOBUF RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/protobuf/" "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/protobuf/*.proto")
LIST(REMOVE_ITEM PROTO_SOURCE_FILES_PROTOBUF "meta_graph.proto" "saved_model.proto" "worker.proto" "worker_service.proto")
FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_PROTOBUF} )
	#MESSAGE(STATUS "Protobuf File: ${PROTO_SOURCE_FILE}")
  IF(WIN32)
	LIST(APPEND PROTO_SOURCE "tensorflow\\core\\protobuf\\${PROTO_SOURCE_FILE}" )
  ELSE()
	LIST(APPEND PROTO_SOURCE "tensorflow/core/protobuf/${PROTO_SOURCE_FILE}" )
ENDIF()
ENDFOREACH()

FILE(GLOB PROTO_SOURCE_FILES_LIB_CORE RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/lib/core" "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/lib/core/*.proto")
FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_LIB_CORE} )
#MESSAGE(STATUS "File: ${PROTO_SOURCE_FILE}")
  IF(WIN32)
	LIST(APPEND PROTO_SOURCE "tensorflow\\core\\lib\\core\\${PROTO_SOURCE_FILE}" )
  ELSE()
	LIST(APPEND PROTO_SOURCE "tensorflow/core/lib/core/${PROTO_SOURCE_FILE}" )
ENDIF()
ENDFOREACH()

MESSAGE(STATUS "PROTO_SOURCE: ${PROTO_SOURCE}")

SET(PROTOBUF_TOOLS_PATH ${CMAKE_CURRENT_SOURCE_DIR}/protobuf.tools)
IF(WIN32)
  SET(PROTOC_EXE "${PROTOBUF_TOOLS_PATH}/tools/windows_x86/protoc.exe")
ELSEIF(APPLE)
  SET(PROTOC_EXE "${PROTOBUF_TOOLS_PATH}/tools/macosx_x64/protoc")
  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND chmod u+rx ${PROTOC_EXE}
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")
ELSE()
  SET(PROTOC_EXE "protoc")
ENDIF()

SET(PROTO_GEN_COMMAND ${PROTOC_EXE} "--proto_path=." "--csharp_out=\"${CMAKE_CURRENT_SOURCE_DIR}\"" "--csharp_opt=file_extension=.g.cs" ${PROTO_SOURCE})

#IF(NOT WIN32)
#  SET(PROTO_GEN_COMMAND mono ${PROTO_GEN_COMMAND})
#ENDIF()

#MESSAGE(STATUS "PROTO_GEN_COMMAND: ${PROTO_GEN_COMMAND}")

#ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
#	PRE_BUILD
#	COMMAND rm -f *.g.cs #remove all existing generated cs files
#	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
#	)

ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND ${PROTO_GEN_COMMAND}
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/"
	)

INSTALL(
FILES 
${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
COMPONENT emgutf_binary)

IF(HAVE_WINDESKTOP)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)
ENDIF()

IF(HAVE_OSX)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.World.Mac)
ENDIF()




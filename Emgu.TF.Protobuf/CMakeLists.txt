# --------------------------------------------------------
#  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.
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
PATTERN ".svn" EXCLUDE
PATTERN "obj" EXCLUDE
PATTERN "CMakeFiles" EXCLUDE
PATTERN "${PROJECT_NAME}.dir" EXCLUDE
)

IF(HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64)

#MESSAGE("Adding protobuf project")
BUILD_CSPROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Protobuf.Netstandard/Emgu.TF.Protobuf.Netstandard.csproj" "" ALL)

ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../miscellaneous/nuget.exe" restore "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Protobuf.Netstandard/Emgu.TF.Protobuf.Netstandard.csproj") 

SET(PROTO_SOURCE)
FILE(GLOB PROTO_SOURCE_FILES_FRAME_WORK RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/framework/" "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/framework/*.proto")
FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_FRAME_WORK} )
	#MESSAGE(STATUS "File: ${PROTO_SOURCE_FILE}")
	LIST(APPEND PROTO_SOURCE "tensorflow\\core\\framework\\${PROTO_SOURCE_FILE}" )
ENDFOREACH()

FILE(GLOB PROTO_SOURCE_FILES_PROTOBUF RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/protobuf/" "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/protobuf/*.proto")
LIST(REMOVE_ITEM PROTO_SOURCE_FILES_PROTOBUF "meta_graph.proto" "saved_model.proto" "worker.proto" "worker_service.proto")
FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_PROTOBUF} )
	#MESSAGE(STATUS "File: ${PROTO_SOURCE_FILE}")
	LIST(APPEND PROTO_SOURCE "tensorflow\\core\\protobuf\\${PROTO_SOURCE_FILE}" )
ENDFOREACH()

FILE(GLOB PROTO_SOURCE_FILES_LIB_CORE RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/lib/core" "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/lib/core/*.proto")
FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_LIB_CORE} )
	#MESSAGE(STATUS "File: ${PROTO_SOURCE_FILE}")
	LIST(APPEND PROTO_SOURCE "tensorflow\\core\\lib\\core\\${PROTO_SOURCE_FILE}" )
ENDFOREACH()

#MESSAGE(STATUS "PROTO_SOURCE: ${PROTO_SOURCE}")
#SET(PROTO_SOURCE 
#  "tensorflow\\core\\framework\\op_def.proto" 
#  "tensorflow\\core\\framework\\tensor.proto" 
#  "tensorflow\\core\\framework\\types.proto" 
#  "tensorflow\\core\\framework\\attr_value.proto" 
#  "tensorflow\\core\\framework\\tensor_shape.proto" 
#  "tensorflow\\core\\framework\\resource_handle.proto" 
#  "tensorflow\\core\\framework\\graph.proto" 
#  "tensorflow\\core\\framework\\step_stats.proto" 
#  "tensorflow\\core\\framework\\versions.proto" 
#  "tensorflow\\core\\framework\\node_def.proto" 
#  "tensorflow\\core\\framework\\tensor_description.proto" 
#  "tensorflow\\core\\framework\\allocation_description.proto" 
#  "tensorflow\\core\\framework\\cost_graph.proto" 
#  "tensorflow\\core\\framework\\function.proto" 
#  "tensorflow\\core\\protobuf\\config.proto" 
#  "tensorflow\\core\\protobuf\\cluster.proto" 
#  "tensorflow\\core\\protobuf\\debug.proto" 
#  "tensorflow\\core\\protobuf\\rewriter_config.proto" )

SET(PROTO_GEN_COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../nugetpackages/Google.Protobuf.Tools.3.5.1/tools/windows_x86/protoc.exe" "--proto_path=." "--csharp_out=\"${CMAKE_CURRENT_SOURCE_DIR}\"" "--csharp_opt=file_extension=.g.cs" ${PROTO_SOURCE})
#MESSAGE(STATUS "PROTO_GEN_COMMAND: ${PROTO_GEN_COMMAND}")

ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND rm "*.g.cs" #remove all existing generated cs files
	WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
	)

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

ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.World)

ENDIF()


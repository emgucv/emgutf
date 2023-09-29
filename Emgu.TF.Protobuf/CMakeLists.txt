# --------------------------------------------------------
#  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------



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
IF (DOTNET_FOUND AND (TARGET Emgu.TF.Netstandard))
  PROJECT(Emgu.TF.Protobuf.Netstandard)
  BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.csproj" "" ALL)
  
  SET(PROTOBUF_TOOLS_NUGET_URL https://www.nuget.org/api/v2/package/Google.Protobuf.Tools/${PROTOBUF_VERSION})
  IF(WIN32)
    SET(PROTOBUF_TOOLS_PATH ${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/bazel-bin/external/com_google_protobuf)
    FIND_PROGRAM (PROTOC_EXE 
      NAMES protoc
      PATHS
      ${PROTOBUF_TOOLS_PATH}
      )
    SET(PROTOBUF_INCLUDE_PATH ${PROTOBUF_TOOLS_PATH}/google/protobuf)
    
    if (("${PROTOC_EXE}" STREQUAL "PROTOC_EXE-NOTFOUND") OR NOT (EXISTS ${PROTOBUF_INCLUDE_PATH}))
      SET(PROTOBUF_TOOLS_PATH ${CMAKE_CURRENT_SOURCE_DIR}/protobuf.tools)
      SET(PROTOC_EXE "${PROTOBUF_TOOLS_PATH}/tools/windows_x64/protoc.exe")
      SET(PROTOBUF_INCLUDE_PATH ${PROTOBUF_TOOLS_PATH}/tools)
      ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	    PRE_BUILD
	    COMMAND wget ${PROTOBUF_TOOLS_NUGET_URL} -O protobuf.tools.zip
	    COMMAND unzip -uo protobuf.tools.zip -d protobuf.tools
        COMMENT "Downloading and extracting protobuf from ${PROTOBUF_TOOLS_NUGET_URL} to 'protobuf.tools' folder under ${CMAKE_CURRENT_SOURCE_DIR};"
	    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")
      ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	    PRE_BUILD
	    COMMAND icacls protoc.exe /grant %USERNAME%:RX
        COMMENT "Adding execute permission to protoc.exe;"
	    WORKING_DIRECTORY "${PROTOBUF_TOOLS_PATH}/tools/windows_x64/")
    ELSE()
      get_filename_component(PROTOC_EXEC_EXT "${PROTOC_EXE}" EXT)
      IF("${PROTOC_EXEC_EXT}" STREQUAL "")	
	    #FILE(COPY "${PROTOC_EXE}" "${PROTOC_EXE}.exe")
	    CONFIGURE_FILE("${PROTOC_EXE}" "${PROTOC_EXE}.exe" COPYONLY)
	    SET(PROTOC_EXE "${PROTOC_EXE}.exe")
	    #MESSAGE(STATUS "PROTOC_EXE EXTENSION: ${PROTOC_EXEC_EXT}")
      ENDIF()
      MESSAGE(STATUS "FOUND PROTOC_EXE: ${PROTOC_EXE}")
    endif()
  ELSEIF(APPLE)
    ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
      PRE_BUILD
      COMMAND wget ${PROTOBUF_TOOLS_NUGET_URL} -O protobuf.tools.zip
      COMMAND unzip -u protobuf.tools.zip -d protobuf.tools
      COMMENT "Downloading and extracting protobuf from ${PROTOBUF_TOOLS_NUGET_URL} to 'protobuf.tools' folder under ${CMAKE_CURRENT_SOURCE_DIR};"
      WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")
    SET(PROTOBUF_TOOLS_PATH ${CMAKE_CURRENT_SOURCE_DIR}/protobuf.tools)
    SET(PROTOC_EXE "${PROTOBUF_TOOLS_PATH}/tools/macosx_x64/protoc")
    SET(PROTOBUF_INCLUDE_PATH ${PROTOBUF_TOOLS_PATH}/tools)
    ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
      PRE_BUILD
      COMMAND chmod u+rx ${PROTOC_EXE}
      COMMENT "Adding execute permission to ${PROTOC_EXE};"
      WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}")
  ELSEIF(IS_UBUNTU)
    FIND_PROGRAM (PROTOC_EXE 
      NAMES protoc
      )
    SET(PROTOBUF_INCLUDE_PATH "/usr/include/google/protobuf")
  ELSE()
    SET(PROTOC_EXE "protoc")
  ENDIF()
  
  SET(PROTO_SOURCE)
  FILE(GLOB PROTO_SOURCE_FILES_FRAME_WORK RELATIVE 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/framework/" 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/framework/*.proto")
  FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_FRAME_WORK} )
    #MESSAGE(STATUS "File: ${PROTO_SOURCE_FILE}")
    LIST(APPEND PROTO_SOURCE "tensorflow/core/framework/${PROTO_SOURCE_FILE}" )
  ENDFOREACH()
  
  FILE(GLOB PROTO_SOURCE_FILES_PROTOBUF RELATIVE 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/protobuf/" 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/protobuf/*.proto")
  LIST(REMOVE_ITEM PROTO_SOURCE_FILES_PROTOBUF 
    "worker.proto" 
    "master.proto"
    "queue_runner.proto"
    "replay_log.proto"
    "worker_service.proto"
    "master_service.proto"
    #"error_codes.proto"
    #"rpc_options.proto"
    #"bfc_memory_map.proto"
    #"autotuning.proto"
  )
  FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_PROTOBUF} )
      LIST(APPEND PROTO_SOURCE "tensorflow/core/protobuf/${PROTO_SOURCE_FILE}" )
  ENDFOREACH()
  
  FILE(GLOB PROTO_SOURCE_FILES_PROTOBUF_TPU RELATIVE 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/protobuf/tpu" 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/protobuf/tpu/*.proto")
  FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_PROTOBUF_TPU} )
      LIST(APPEND PROTO_SOURCE "tensorflow/core/protobuf/tpu/${PROTO_SOURCE_FILE}" )
  ENDFOREACH()

  SET (PROTO_SOURCE_NATIVE_PATH)
  FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE})
    file(TO_NATIVE_PATH ${PROTO_SOURCE_FILE} PROTO_SOURCE_FILE)
    MESSAGE(STATUS "Adding proto source: ${PROTO_SOURCE_FILE}")
    LIST(APPEND PROTO_SOURCE_NATIVE_PATH ${PROTO_SOURCE_FILE})
  ENDFOREACH()

  SET(PROTO_GEN_COMMAND "${PROTOC_EXE}" "--experimental_allow_proto3_optional" "--proto_path=." "--proto_path=\"${PROTOBUF_INCLUDE_PATH}\""  "--csharp_out=\"${CMAKE_CURRENT_SOURCE_DIR}\"" "--csharp_opt=file_extension=.g.cs" ${PROTO_SOURCE_NATIVE_PATH})

  SET(PROTO_SOURCE_TSL)
  FILE(GLOB PROTO_SOURCE_FILES_PROTOBUF_TSL RELATIVE 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/tsl/protobuf"
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/tsl/protobuf/*.proto")
  FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_PROTOBUF_TSL} )
      LIST(APPEND PROTO_SOURCE_TSL "tensorflow/tsl/protobuf/${PROTO_SOURCE_FILE}" )
  ENDFOREACH()

  SET (PROTO_SOURCE_TSL_NATIVE_PATH)
  FOREACH(PROTO_SOURCE_TSL_FILE ${PROTO_SOURCE_TSL})
    file(TO_NATIVE_PATH ${PROTO_SOURCE_TSL_FILE} PROTO_SOURCE_TSL_FILE)
    MESSAGE(STATUS "Adding proto source: ${PROTO_SOURCE_TSL_FILE}")
    LIST(APPEND PROTO_SOURCE_TSL_NATIVE_PATH ${PROTO_SOURCE_TSL_FILE})
  ENDFOREACH()
  
  SET(PROTO_GEN_COMMAND_TSL "${PROTOC_EXE}" "--experimental_allow_proto3_optional" "--proto_path=." "--proto_path=\"${PROTOBUF_INCLUDE_PATH}\""  "--csharp_out=\"${CMAKE_CURRENT_SOURCE_DIR}\"" "--csharp_opt=file_extension=.tsl.g.cs" ${PROTO_SOURCE_TSL_NATIVE_PATH})

  SET(PROTO_SOURCE_XLA
    "tensorflow/compiler/xla/stream_executor/device_description.proto"
    "tensorflow/compiler/xla/stream_executor/dnn.proto"
    "tensorflow/compiler/xla/xla.proto"
    "tensorflow/compiler/xla/xla_data.proto"
    "tensorflow/compiler/xla/autotune_results.proto"
	"tensorflow/compiler/xla/autotuning.proto"
    "tensorflow/compiler/xla/service/hlo.proto"
    "tensorflow/compiler/xla/pjrt/distributed/protocol.proto"
  )
  SET (PROTO_SOURCE_XLA_NATIVE_PATH)
  FOREACH(PROTO_SOURCE_XLA_FILE ${PROTO_SOURCE_XLA})
    file(TO_NATIVE_PATH ${PROTO_SOURCE_XLA_FILE} PROTO_SOURCE_XLA_FILE)
    MESSAGE(STATUS "Adding proto source: ${PROTO_SOURCE_XLA_FILE}")
    LIST(APPEND PROTO_SOURCE_XLA_NATIVE_PATH ${PROTO_SOURCE_XLA_FILE})
  ENDFOREACH()
  
  SET(PROTO_GEN_COMMAND_XLA "${PROTOC_EXE}" "--experimental_allow_proto3_optional" "--proto_path=." "--proto_path=\"${PROTOBUF_INCLUDE_PATH}\""  "--csharp_out=\"${CMAKE_CURRENT_SOURCE_DIR}\"" "--csharp_opt=file_extension=.xla.g.cs" ${PROTO_SOURCE_XLA_NATIVE_PATH})
  
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
    COMMAND ${PROTO_GEN_COMMAND_TSL}
	COMMAND ${PROTO_GEN_COMMAND_XLA}
    COMMENT "PROTO_GEN_COMMAND: ${PROTO_GEN_COMMAND}"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/"
    )


#  LIST(APPEND PROTO_SOURCE_2 
#    "tensorflow/tsl/protobuf/error_codes.proto")
#  SET (PROTO_SOURCE_NATIVE_PATH_2 )
#  FOREACH(PROTO_SOURCE_FILE_2 ${PROTO_SOURCE_2})
#    file(TO_NATIVE_PATH ${PROTO_SOURCE_FILE_2} PROTO_SOURCE_FILE_2)
#    LIST(APPEND PROTO_SOURCE_NATIVE_PATH_2 ${PROTO_SOURCE_FILE_2})
#  ENDFOREACH()
#  SET(PROTO_GEN_COMMAND_2 "${PROTOC_EXE}" "--proto_path=." "--proto_path=\"${PROTOBUF_INCLUDE_PATH}\""  "--csharp_out=\"${CMAKE_CURRENT_SOURCE_DIR}\"" "--csharp_opt=file_extension=.2.g.cs" ${PROTO_SOURCE_NATIVE_PATH})
#  
#  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
#    PRE_BUILD
#    COMMAND ${PROTO_GEN_COMMAND_2}
#    COMMENT "PROTO_GEN_COMMAND_2: ${PROTO_GEN_COMMAND_2}"
#    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/"
#    )

  
  IF (EMGU_SIGN_FOUND)
    EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll )
  ENDIF()

  INSTALL(
    FILES 
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
    ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
  
  #IF(HAVE_WINDESKTOP OR HAVE_OSX)
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.Netstandard)
  #ENDIF()
  
ENDIF()

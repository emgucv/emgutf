# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
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
    SET(PROTOBUF_TOOLS_PATH ${CMAKE_CURRENT_SOURCE_DIR}/../protobuf_archive)
    FIND_PROGRAM (PROTOC_EXE 
      NAMES protoc
      PATHS
      ${PROTOBUF_TOOLS_PATH}
      )
    SET(PROTOBUF_INCLUDE_PATH ${PROTOBUF_TOOLS_PATH}/src)
    
    if (("${PROTOC_EXE}" STREQUAL "PROTOC_EXE-NOTFOUND") OR NOT (EXISTS ${PROTOBUF_INCLUDE_PATH}))
      SET(PROTOBUF_TOOLS_PATH ${CMAKE_CURRENT_SOURCE_DIR}/protobuf.tools)
      SET(PROTOC_EXE "${PROTOBUF_TOOLS_PATH}/tools/windows_x64/protoc.exe")
      SET(PROTOBUF_INCLUDE_PATH ${PROTOBUF_TOOLS_PATH}/tools)
      ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
	    PRE_BUILD
	    COMMAND wget ${PROTOBUF_TOOLS_NUGET_URL} -O protobuf.tools.zip
	    COMMAND unzip -u protobuf.tools.zip -d protobuf.tools
        COMMENT "Downloading and extractiong protobuf from ${PROTOBUF_TOOLS_NUGET_URL} to 'protobuf.tools' folder under ${CMAKE_CURRENT_SOURCE_DIR};"
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
      COMMENT "Downloading and extractiong protobuf from ${PROTOBUF_TOOLS_NUGET_URL} to 'protobuf.tools' folder under ${CMAKE_CURRENT_SOURCE_DIR};"
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
  #LIST(REMOVE_ITEM PROTO_SOURCE_FILES_PROTOBUF "meta_graph.proto" "saved_model.proto" "worker.proto" "worker_service.proto")
  #LIST(REMOVE_ITEM PROTO_SOURCE_FILES_PROTOBUF "saved_model.proto" "worker.proto" "worker_service.proto")
  LIST(REMOVE_ITEM PROTO_SOURCE_FILES_PROTOBUF "worker.proto" "worker_service.proto")
  FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_PROTOBUF} )
    #MESSAGE(STATUS "Protobuf File: ${PROTO_SOURCE_FILE}")
    IF(WIN32)
      LIST(APPEND PROTO_SOURCE "tensorflow\\core\\protobuf\\${PROTO_SOURCE_FILE}" )
    ELSE()
      LIST(APPEND PROTO_SOURCE "tensorflow/core/protobuf/${PROTO_SOURCE_FILE}" )
    ENDIF()
  ENDFOREACH()
  
  #FILE(GLOB PROTO_SOURCE_FILES_LIB_CORE RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/lib/core" "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/lib/core/*.proto")
  #FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_LIB_CORE} )
  #  IF(WIN32)
  #    LIST(APPEND PROTO_SOURCE "tensorflow\\core\\lib\\core\\${PROTO_SOURCE_FILE}" )
  #  ELSE()
  #    LIST(APPEND PROTO_SOURCE "tensorflow/core/lib/core/${PROTO_SOURCE_FILE}" )
  #  ENDIF()
  #ENDFOREACH()
  
  IF(WIN32)
    LIST(APPEND PROTO_SOURCE "tensorflow\\stream_executor\\dnn.proto" )
  ELSE()
    LIST(APPEND PROTO_SOURCE "tensorflow/stream_executor/dnn.proto" )
  ENDIF()
  
  SET(PROTO_GEN_COMMAND "${PROTOC_EXE}" "--proto_path=." "--proto_path=\"${PROTOBUF_INCLUDE_PATH}\""  "--csharp_out=\"${CMAKE_CURRENT_SOURCE_DIR}\"" "--csharp_opt=file_extension=.g.cs" ${PROTO_SOURCE})
  
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
    COMMENT "PROTO_GEN_COMMAND: ${PROTO_GEN_COMMAND}"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/"
    )
  
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

# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
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
  PATTERN "README.md"
  PATTERN "Directory.Build.props"
  PATTERN ".svn" EXCLUDE
  PATTERN "obj" EXCLUDE
  PATTERN "CMakeFiles" EXCLUDE
  PATTERN "x64" EXCLUDE #exclude build artifacts from Visual Studio build process
  PATTERN "x86" EXCLUDE #exclude build artifacts from Visual Studio build process
  PATTERN "${PROJECT_NAME}.dir" EXCLUDE
  )

#MESSAGE("Adding protobuf project")
IF (DOTNET_FOUND AND (TARGET Emgu.TF))
  PROJECT(Emgu.TF.Protobuf)
  BUILD_DOTNET_PROJ(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/NetStandard/Emgu.TF.Protobuf.csproj" "" ALL)
  
  SET(PROTOBUF_TOOLS_NUGET_URL https://www.nuget.org/api/v2/package/Google.Protobuf.Tools/${PROTOBUF_VERSION})
  IF(WIN32)
    FIND_PROGRAM (PROTOC_EXE 
      NAMES protoc protoc.exe
      PATHS
      ${CMAKE_CURRENT_SOURCE_DIR}/protobuf.tools/tools/windows_x64
      )
    
    if (("${PROTOC_EXE}" STREQUAL "PROTOC_EXE-NOTFOUND") OR NOT (EXISTS ${PROTOBUF_INCLUDE_PATH}))
      SET(PROTOBUF_TOOLS_PATH ${CMAKE_CURRENT_SOURCE_DIR}/protobuf.tools)
      SET(PROTOC_EXE "${PROTOBUF_TOOLS_PATH}/tools/windows_x64/protoc.exe")
      SET(PROTOBUF_INCLUDE_PATH ${PROTOBUF_TOOLS_PATH}/tools)
      # Download protobuf tools
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
      MESSAGE(STATUS "PROTOC_EXE: ${PROTOC_EXE} FOUND")
      SET(PROTOBUF_INCLUDE_PATH ${PROTOBUF_TOOLS_PATH}/google/protobuf)
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

  # Add .proto files from tensorflow/core/framework
  FILE(GLOB PROTO_SOURCE_FILES_FRAMEWORK RELATIVE 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/framework/" 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/framework/*.proto")
  FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_FRAMEWORK} )
    LIST(APPEND PROTO_SOURCE "tensorflow/core/framework/${PROTO_SOURCE_FILE}" )
  ENDFOREACH()
  
  SET(PROTOBUF_TSL_INCLUDE_PATH_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/third_party/xla/xla")
  SET(PROTOBUF_TSL_INCLUDE_PATH "${PROTOBUF_TSL_INCLUDE_PATH_ROOT}/tsl/protobuf")
  SET(PROTO_SOURCE_XLA_INCLUDE_PATH_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/third_party/xla")
  
  # Add .proto files from tensorflow/core/protobuf
  FILE(GLOB PROTO_SOURCE_FILES_PROTOBUF RELATIVE 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/protobuf/" 
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/core/protobuf/*.proto")
  LIST(REMOVE_ITEM PROTO_SOURCE_FILES_PROTOBUF 
    "conv_autotuning.proto"
    "autotuning.proto"
  )
  FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE_FILES_PROTOBUF} )
      LIST(APPEND PROTO_SOURCE "tensorflow/core/protobuf/${PROTO_SOURCE_FILE}" )
  ENDFOREACH()

  # Compute native paths for the proto files
  SET (PROTO_SOURCE_NATIVE_PATH)
  FOREACH(PROTO_SOURCE_FILE ${PROTO_SOURCE})
    file(TO_NATIVE_PATH ${PROTO_SOURCE_FILE} PROTO_SOURCE_FILE)
    MESSAGE(STATUS "Adding proto source: ${PROTO_SOURCE_FILE}")
    LIST(APPEND PROTO_SOURCE_NATIVE_PATH ${PROTO_SOURCE_FILE})
  ENDFOREACH()

  # Set the protobuf generation command
  SET(PROTO_GEN_COMMAND 
	"${PROTOC_EXE}" 
	"--experimental_allow_proto3_optional" 
	"--proto_path=." 
	"--proto_path=\"${PROTOBUF_INCLUDE_PATH}\"" 
	"--proto_path=\"${PROTOBUF_TSL_INCLUDE_PATH_ROOT}\"" 
	"--proto_path=\"${PROTO_SOURCE_XLA_INCLUDE_PATH_ROOT}\"" 
	"--csharp_out=\"${CMAKE_CURRENT_SOURCE_DIR}\"" 
	"--csharp_opt=file_extension=.g.cs"
    "--csharp_opt=base_namespace="	
	${PROTO_SOURCE_NATIVE_PATH})
  
  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
    PRE_BUILD
    COMMAND ${PROTO_GEN_COMMAND}
    COMMENT "PROTO_GEN_COMMAND: ${PROTO_GEN_COMMAND}"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/"
    )

  # Add .proto files from tsl
  SET(PROTO_SOURCE_TSL)
  FILE(GLOB PROTO_SOURCE_FILES_TSL RELATIVE 
    "${PROTO_SOURCE_XLA_INCLUDE_PATH_ROOT}" 
    "${PROTO_SOURCE_XLA_INCLUDE_PATH_ROOT}/xla/tsl/protobuf/*.proto")
  FOREACH(PROTO_SOURCE_FILE_TSL ${PROTO_SOURCE_FILES_TSL} )
    LIST(APPEND PROTO_SOURCE_TSL "${PROTO_SOURCE_FILE_TSL}" )
  ENDFOREACH()
  
  # Compute native paths for the proto files
  SET (PROTO_SOURCE_NATIVE_PATH_TSL )
  FOREACH(PROTO_SOURCE_FILE_TSL ${PROTO_SOURCE_TSL})
    file(TO_NATIVE_PATH ${PROTO_SOURCE_FILE_TSL} PROTO_SOURCE_FILE_TSL)
    MESSAGE(STATUS "Adding proto source: ${PROTO_SOURCE_FILE_TSL}")
    LIST(APPEND PROTO_SOURCE_NATIVE_PATH_TSL ${PROTO_SOURCE_FILE_TSL})
  ENDFOREACH()

  # Set the protobuf generation command for tsl
  SET(PROTO_GEN_COMMAND_TSL 
    "${PROTOC_EXE}" 
    "--proto_path=." 
    "--proto_path=\"${PROTOBUF_INCLUDE_PATH}\"" 
    "--proto_path=\"${PROTOBUF_TSL_INCLUDE_PATH_ROOT}\""  
    "--proto_path=\"${PROTO_SOURCE_XLA_INCLUDE_PATH_ROOT}\"" 
    "--csharp_out=\"${CMAKE_CURRENT_SOURCE_DIR}\"" 
    "--csharp_opt=file_extension=.tsl.g.cs" 
    "--csharp_opt=base_namespace="	
    ${PROTO_SOURCE_NATIVE_PATH_TSL})
  
  ADD_CUSTOM_COMMAND(TARGET ${PROJECT_NAME} 
    PRE_BUILD
    COMMAND ${PROTO_GEN_COMMAND_TSL}
    COMMENT "PROTO_GEN_COMMAND_TSL: ${PROTO_GEN_COMMAND_TSL}"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/third_party/xla"
    )

  INSTALL(
    DIRECTORY
    ${LIBRARY_OUTPUT_PATH}/Build/Emgu.TF.Protobuf
    DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
    COMPONENT emgutf_binary)
  
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF)
  
ENDIF()

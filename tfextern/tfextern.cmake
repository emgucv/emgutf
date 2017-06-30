
########################################################
# tfextern library
########################################################
set(tfextern_srcs
	"${tensorflow_source_dir}/../tfextern/tfextern.cc"
)
set(tfextern_hdrs
	"${tensorflow_source_dir}/../tfextern/tfextern.h"
)

add_library(tfextern SHARED
    ${tfextern_srcs} ${tfextern_hdrs}
	$<TARGET_OBJECTS:tf_c>
	$<TARGET_OBJECTS:tf_cc>
    $<TARGET_OBJECTS:tf_core_lib>
    $<TARGET_OBJECTS:tf_core_cpu>
    $<TARGET_OBJECTS:tf_core_framework>
    $<TARGET_OBJECTS:tf_core_kernels>
    $<TARGET_OBJECTS:tf_cc_framework>
    $<TARGET_OBJECTS:tf_cc_ops>
    $<TARGET_OBJECTS:tf_core_ops>
    $<TARGET_OBJECTS:tf_core_direct_session>
    $<$<BOOL:${tensorflow_ENABLE_GPU}>:$<TARGET_OBJECTS:tf_stream_executor>>
)

target_link_libraries(tfextern PUBLIC
    tf_protos_cc
    ${tf_core_gpu_kernels_lib}
    ${tensorflow_EXTERNAL_LIBRARIES}
)

SET(TF_LIB_SUBFOLDER "x86")
IF(APPLE)
  SET(TF_LIB_SUBFOLDER "osx")
ELSEIF("${CMAKE_SIZEOF_VOID_P}" STREQUAL "8")
  SET(TF_LIB_SUBFOLDER "x64")
endif()

set_target_properties(tfextern
    PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}"
    LIBRARY_OUTPUT_DIRECTORY "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}"
    RUNTIME_OUTPUT_DIRECTORY "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}"
	ARCHIVE_OUTPUT_DIRECTORY_DEBUG "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}"
    LIBRARY_OUTPUT_DIRECTORY_DEBUG "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}"
    RUNTIME_OUTPUT_DIRECTORY_DEBUG "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}"
	ARCHIVE_OUTPUT_DIRECTORY_RELEASE "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}"
    LIBRARY_OUTPUT_DIRECTORY_RELEASE "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}"
    RUNTIME_OUTPUT_DIRECTORY_RELEASE "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}"
)

SET(TFEXTERN_DEPENDENCY_DLLS)

############################### GPU START ##############################
MESSAGE(STATUS "tensorflow_ENABLE_GPU: ${tensorflow_ENABLE_GPU}")
IF(WIN32 AND tensorflow_ENABLE_GPU)
  SET(CUDA_TOOLKIT_ROOT_DIR "${CUDNN_HOME}")
  SET(CUDA_NPP_INCLUDES "${CUDA_TOOLKIT_ROOT_DIR}/include")
  MESSAGE("npp includes: ${CUDA_NPP_INCLUDES}")
  if(EXISTS ${CUDA_NPP_INCLUDES}/nppversion.h)
    MESSAGE("npp version header: ${CUDA_NPP_INCLUDES}/nppversion.h")
    file( STRINGS ${CUDA_NPP_INCLUDES}/nppversion.h npp_major REGEX "#define NPP_VERSION_MAJOR.*")
    file( STRINGS ${CUDA_NPP_INCLUDES}/nppversion.h npp_minor REGEX "#define NPP_VERSION_MINOR.*")
    file( STRINGS ${CUDA_NPP_INCLUDES}/nppversion.h npp_build REGEX "#define NPP_VERSION_BUILD.*")
    
    string( REGEX REPLACE "#define NPP_VERSION_MAJOR[ \t]+|//.*" "" npp_major ${npp_major})
    string( REGEX REPLACE "#define NPP_VERSION_MINOR[ \t]+|//.*" "" npp_minor ${npp_minor})
    string( REGEX REPLACE "#define NPP_VERSION_BUILD[ \t]+|//.*" "" npp_build ${npp_build})
    
    string( REGEX MATCH "[0-9]+" npp_major ${npp_major} ) 
    string( REGEX MATCH "[0-9]+" npp_minor ${npp_minor} ) 
    string( REGEX MATCH "[0-9]+" npp_build ${npp_build} ) 	
  endif()
  
  #SET(CUDA_NPP_LIBRARY_ROOT_DIR ${CUDA_TOOLKIT_ROOT_DIR})
  #replace any potential backslash in the path with slash
  #STRING(REGEX REPLACE "\\\\" "/" CUDA_NPP_LIBRARY_ROOT_DIR ${CUDA_NPP_LIBRARY_ROOT_DIR}) 
  
  SET(CUDA_POSTFIX 64)
  
  LIST(APPEND TFEXTERN_DEPENDENCY_DLLS 
	  #"${CUDA_NPP_LIBRARY_ROOT_DIR}/bin/nppc${CUDA_POSTFIX}_${npp_major}${npp_minor}.dll"
	  #"${CUDA_NPP_LIBRARY_ROOT_DIR}/bin/nppi${CUDA_POSTFIX}_${npp_major}${npp_minor}.dll"
	  #"${CUDA_NPP_LIBRARY_ROOT_DIR}/bin/npps${CUDA_POSTFIX}_${npp_major}${npp_minor}.dll"
	  "${CUDA_TOOLKIT_ROOT_DIR}/bin/cudart${CUDA_POSTFIX}_${npp_major}${npp_minor}.dll"
	  "${CUDA_TOOLKIT_ROOT_DIR}/bin/cufft${CUDA_POSTFIX}_${npp_major}${npp_minor}.dll"
	  "${CUDA_TOOLKIT_ROOT_DIR}/bin/cublas${CUDA_POSTFIX}_${npp_major}${npp_minor}.dll"
	  "${CUDA_TOOLKIT_ROOT_DIR}/bin/curand${CUDA_POSTFIX}_${npp_major}${npp_minor}.dll"
	  "${CUDA_TOOLKIT_ROOT_DIR}/bin/cusolver${CUDA_POSTFIX}_${npp_major}${npp_minor}.dll"
	  "${CUDA_TOOLKIT_ROOT_DIR}/bin/cudnn${CUDA_POSTFIX}_5.dll"
	  )
ENDIF()

############################### GPU END ################################


IF(WIN32 AND MSVC AND (NOT NETFX_CORE))
  # Add install rules for required system runtimes such as MSVCRxx.dll
  SET (CMAKE_INSTALL_SYSTEM_RUNTIME_LIBS_SKIP ON)
  INCLUDE(InstallRequiredSystemLibraries)
  LIST(APPEND TFEXTERN_DEPENDENCY_DLLS ${CMAKE_INSTALL_SYSTEM_RUNTIME_LIBS})   
ENDIF()
IF(DEFINED TFEXTERN_DEPENDENCY_DLLS)
  FOREACH(TFEXTERN_DEPENDENCY_DLL ${TFEXTERN_DEPENDENCY_DLLS})
    LIST(APPEND TFEXTERN_DEPENDENCY_DLL_DEPLOY_COMMAND COMMAND ${CMAKE_COMMAND} -E copy "${TFEXTERN_DEPENDENCY_DLL}" "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}")
    GET_FILENAME_COMPONENT(TFEXTERN_DEPENDENCT_DLL_NAME ${TFEXTERN_DEPENDENCY_DLL} NAME_WE)
    LIST(APPEND TFEXTERN_DEPENDENCY_DLL_NAMES ${TFEXTERN_DEPENDENCT_DLL_NAME})
  ENDFOREACH()
  
  #Promote this to parent scope such that cpack will know what dlls to be included in the package
  #SET(TFEXTERN_DEPENDENCY_DLL_NAMES ${TFEXTERN_DEPENDENCY_DLL_NAMES} PARENT_SCOPE)
  
  ADD_CUSTOM_COMMAND(
    TARGET tfextern
    POST_BUILD
    ${TFEXTERN_DEPENDENCY_DLL_DEPLOY_COMMAND}
    COMMENT "Copying ${TFEXTERN_DEPENDENCY_DLLS} to ${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}")
ENDIF()

IF (APPLE)
  set_target_properties(tfextern PROPERTIES MACOSX_RPATH ON)
ENDIF()

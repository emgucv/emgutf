# --------------------------------------------------------
# Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
# 
# Root CMake file for Emgu TF
#
# From the off-tree build directory, invoke:
#   $ cmake <PATH_TO_EMGUTF_ROOT>
#
# --------------------------------------------------------

PROJECT(emgutf)
CMAKE_MINIMUM_REQUIRED(VERSION 2.8.1)

# ----------------------------------------------------------------------------
#  Autodetect if we are in a GIT repository
# ----------------------------------------------------------------------------
FIND_PROGRAM(GIT_PATH git)
FIND_PROGRAM(WC_PATH wc)
MARK_AS_ADVANCED(force GIT_PATH WC_PATH)
IF(GIT_PATH AND WC_PATH)
  MESSAGE(STATUS "Extracting Emgu TF git version, please wait...")
  EXECUTE_PROCESS(
    WORKING_DIRECTORY  ${CMAKE_CURRENT_SOURCE_DIR}
    COMMAND ${GIT_PATH} log --oneline 
    COMMAND ${WC_PATH} -l
    OUTPUT_VARIABLE GITVERSION_RESULT)
  #MESSAGE(STATUS "COMMAND: ${GIT_PATH} log --oneline | ${WC_PATH} -l")
  STRING(REGEX REPLACE "^([0123456789]+).*" "\\1" EMGUTF_GITVERSION "${GITVERSION_RESULT}")
  STRING(STRIP "${EMGUTF_GITVERSION}" EMGUTF_GITVERSION)
  IF(EMGUTF_GITVERSION MATCHES "^$" OR EMGUTF_GITVERSION MATCHES "^-")
    # This is NOT a git repository:
    SET(EMGUTF_GITVERSION "0")
  ENDIF()
  MESSAGE(STATUS "Emgu TF GIT VERSION: ${EMGUTF_GITVERSION}")
ELSE()
  # We don't have git or wc:
  SET(EMGUTF_GITVERSION "0")
ENDIF()
SET(TENSORFLOW_SOURCE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/tensorflow")
SET(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${CMAKE_CURRENT_SOURCE_DIR}/cmake/modules")

SET(CSC_PREFERRED_VERSION "16.0")
SET(CSC_TARGET_FRAMEWORK "" CACHE STRING "C# target framework")

#SET(CSC_PREFERRED_VERSION "4.0")
#IF((NOT NETFX_CORE) AND MSVC) 
#  #SET CSC 3.5 as the prefered compiler for EMGU TF
#  SET(CSC_TARGET_FRAMEWORK "3.5" CACHE STRING "C#  target framework")
#ELSE()
#  #SET CSC 4.5 (in the .Net framework 4.0 folder) as the prefered compiler for EMGU TF
#  SET(CSC_TARGET_FRAMEWORK "" CACHE STRING "C# target framework")
#ENDIF()

FIND_PACKAGE(CSharp REQUIRED)
MESSAGE(STATUS "DOTNET_FOUND: ${DOTNET_FOUND}")
INCLUDE (CheckPlatform)
IF ( (IS_UBUNTU OR IS_RASPBIAN) AND NOT (DOTNET_FOUND))
  MESSAGE(FATAL_ERROR "dotnet core is required to build Emgu TF on Ubuntu, please follow the instructions to install dotnet core on Ubuntu: https://dotnet.microsoft.com/download/linux-package-manager/ubuntu18-04/sdk-current")  
ENDIF()
#MESSAGE(STATUS "IS_UBUNTU: ${IS_UBUNTU}")
INCLUDE (CSharpMacros)
INCLUDE (TensorflowVersion)
MESSAGE(STATUS "TENSORFLOW_VERSION: ${TENSORFLOW_VERSION} ")

SET(LICENSE_FILE_NAME "LICENSE")
MACRO(CHECK_LICENSE_TYPE)
  MESSAGE(STATUS "LICENSE FILE: ${CMAKE_CURRENT_SOURCE_DIR}/${LICENSE_FILE_NAME}")
  IF (WIN32)
    EXECUTE_PROCESS(COMMAND "findstr" "gpl" "${LICENSE_FILE_NAME}" WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} OUTPUT_VARIABLE TMP)
  ELSE()	
    EXECUTE_PROCESS(COMMAND "grep" "gpl" "${LICENSE_FILE_NAME}" WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} OUTPUT_VARIABLE TMP)
  ENDIF()
  
  STRING(STRIP "${TMP}" TMP)
  STRING(LENGTH "${TMP}" GPL_LICENSE_LENGTH)
  SET (LICENSE_TYPE "Dual License")
  #SET (EMGUTF_LICENSE_FILE ${LICENSE_FILE_NAME})
  #SET (EMGUTF_LICENSE_URL "https://www.gnu.org/licenses/gpl-3.0.txt")
  IF(${GPL_LICENSE_LENGTH} EQUAL 0)
    SET(LICENSE_TYPE "Commercial")
    #	SET (EMGUTF_LICENSE_URL "http://www.emgu.com/wiki/files/Emgu.TF.Commercial.License.1.0.pdf")
  ENDIF()
  
  MESSAGE(STATUS "LICENSE_TYPE: ${LICENSE_TYPE}")
ENDMACRO()

CHECK_LICENSE_TYPE()
SET(EMGUTF_MAIN_WEB_URL "http://www.emgu.com/wiki/index.php/Emgu_TF")


IF(MSVC)
  set(ENABLE_SOLUTION_FOLDERS ON CACHE BOOL "Solution folder in Visual Studio")
ENDIF()

set_property(GLOBAL PROPERTY USE_FOLDERS ${ENABLE_SOLUTION_FOLDERS})

# ----------------------------------------------------------------------------
#  Check if 32bit or 64-bit
# ----------------------------------------------------------------------------
INCLUDE (CheckTargetArch)
CHECK_TARGET_ARCH()

IF (WIN32)
  MESSAGE(STATUS "CMAKE_SYSTEM_NAME: ${CMAKE_SYSTEM_NAME}")
  IF(TARGET_ARCH_64)
    SET(EMGUTF_ARCH win64)
  ELSE()
    #ELSEIF ( ("${CMAKE_SYSTEM_NAME}" STREQUAL "WindowsPhone") OR ("${CMAKE_SYSTEM_NAME}" STREQUAL "WindowsStore") )
    MESSAGE(STATUS "CMAKE_GENERATOR: ${CMAKE_GENERATOR}")
    STRING(FIND "${CMAKE_GENERATOR}" "ARM" IS_ARM)
    IF(IS_ARM GREATER -1)
      SET(EMGUTF_ARCH arm)
    ELSE()
      SET(EMGUTF_ARCH win32)
    ENDIF()
  ENDIF()
ELSEIF(IS_UBUNTU)
  IF(TARGET_ARCH_64)
    SET(EMGUTF_ARCH "ubuntu_x64")
  ELSE()
    SET(EMGUTF_ARCH "ubuntu_x86")
  ENDIF()
ELSEIF(IS_RASPBIAN)
  SET(EMGUTF_ARCH "raspbian_arm")
ELSE()
  SET(EMGUTF_ARCH ${CMAKE_SYSTEM_NAME})
ENDIF()

MESSAGE(STATUS "Building for ${EMGUTF_ARCH}")

SET(LIBRARY_OUTPUT_PATH ${CMAKE_SOURCE_DIR}/lib)
IF(WIN32)
  IF ("${EMGUTF_ARCH}" STREQUAL "win64")
    IF("${CMAKE_SYSTEM_NAME}" STREQUAL "WindowsStore")
      IF(CMAKE_SYSTEM_VERSION MATCHES 8.1)
	SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/store8_1_x64")
      ELSE()
	SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/UWP_x64")
      ENDIF()
    ELSE()
      SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/x64")
    ENDIF()
  ELSEIF("${EMGUTF_ARCH}" STREQUAL "arm")
    IF("${CMAKE_SYSTEM_NAME}" STREQUAL "WindowsPhone")
      SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/phone8_1_arm")
    ELSE()
      IF(CMAKE_SYSTEM_VERSION MATCHES 10.0)
	SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/UWP_arm")
      ELSE()
	SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/arm")
      ENDIF()
    ENDIF()
  ELSE()
    IF("${CMAKE_SYSTEM_NAME}" STREQUAL "WindowsPhone")
      SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/phone8_1_x86")
    ELSEIF("${CMAKE_SYSTEM_NAME}" STREQUAL "WindowsStore")
      IF(CMAKE_SYSTEM_VERSION MATCHES 10.0)
	SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/UWP_x86")
      ELSE()
	SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/store8_1_x86")
      ENDIF()
    ELSE()
      SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/x86")
    ENDIF()
  ENDIF()
  #ELSEIF(APPLE)
  #  SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "/lib")
ELSEIF()
  SET(UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER "")
ENDIF()

SET(UNMANAGED_LIBRARY_OUTPUT_PATH "${LIBRARY_OUTPUT_PATH}${UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER}")
FILE(MAKE_DIRECTORY ${UNMANAGED_LIBRARY_OUTPUT_PATH})

SET(UNMANAGED_DLL_EXTRA)

MACRO(CHECK_BINARY_EXIST folder patterns property_name second_check)
  SET(${property_name} ON)
  #MESSAGE(STATUS "PATTERNS: ${patterns}")
  #separate_arguments(patterns)
  foreach(pattern ${patterns})
    #MESSAGE(STATUS "PATTERN: ${pattern}")
    FILE(GLOB DLL_LIST "${CMAKE_CURRENT_SOURCE_DIR}/${folder}/${pattern}")
    LIST(LENGTH DLL_LIST DLL_LIST_LENGTH)
    IF(DLL_LIST_LENGTH GREATER 0)
      LIST(APPEND UNMANAGED_DLL_EXTRA ${DLL_LIST})
      MESSAGE(STATUS "Including the following files in package: ${DLL_LIST}")
      INSTALL(
        FILES
        ${DLL_LIST}
        DESTINATION ${folder}
        COMPONENT emgutf_binary
        )  
      #SET(${property_name} ON)
    ELSE()
      SET(${property_name} ${second_check})
    ENDIF()
    #ENDIF()
    #MESSAGE(STATUS "Property - ${property_name}: ${${property_name}}")
  endforeach()
  IF(${${property_name}})
    MESSAGE(STATUS "${property_name}: ${${property_name}}")
  ENDIF()
ENDMACRO()


#CHECK_BINARY_EXIST("lib/ios" "libtfextern.a" HAVE_IOS OFF)
CHECK_BINARY_EXIST("lib/ios" "libtfliteextern.a" HAVE_IOS_LITE OFF)
#CHECK_BINARY_EXIST("lib/osx" "{libtfextern|libtensorflow_framework}.dylib" HAVE_OSX OFF)
CHECK_BINARY_EXIST("lib/osx" "libtensorflow_framework.dylib;libtfextern.dylib" HAVE_OSX OFF)
#CHECK_BINARY_EXIST("lib/osx" "*.dylib" HAVE_OSX OFF)
#CHECK_BINARY_EXIST("lib/osx" "libtfextern.dylib" HAVE_OSX OFF)
CHECK_BINARY_EXIST("lib/osx" "libtfliteextern.dylib" HAVE_OSX_LITE OFF)
CHECK_BINARY_EXIST("lib/x64" "libtfextern.so" HAVE_LINUX_X86_64 OFF)
CHECK_BINARY_EXIST("lib/x64" "libtfliteextern.so" HAVE_LINUX_X86_64_LITE OFF)
CHECK_BINARY_EXIST("lib/arm" "libtfextern.so" HAVE_LINUX_ARM OFF)
CHECK_BINARY_EXIST("lib/arm" "libtfliteextern.so" HAVE_LINUX_ARM_LITE OFF)
CHECK_BINARY_EXIST("lib/android/arm64-v8a" "libtfextern.so" HAVE_ANDROID_ARM64_V8A OFF)
CHECK_BINARY_EXIST("lib/android/arm64-v8a" "libtfliteextern.so" HAVE_ANDROID_ARM64_V8A_LITE OFF)
#CHECK_BINARY_EXIST("lib/android/armeabi" "libtfextern.so" HAVE_ANDROID_ARMEABI OFF)
#CHECK_BINARY_EXIST("lib/android/armeabi" "libtfliteextern.so" HAVE_ANDROID_ARMEABI_LITE OFF)
CHECK_BINARY_EXIST("lib/android/armeabi-v7a" "libtfextern.so" HAVE_ANDROID_ARMEABI_V7A OFF)
CHECK_BINARY_EXIST("lib/android/armeabi-v7a" "libtfliteextern.so" HAVE_ANDROID_ARMEABI_V7A_LITE OFF)
CHECK_BINARY_EXIST("lib/android/x86" "libtfextern.so" HAVE_ANDROID_X86 OFF)
CHECK_BINARY_EXIST("lib/android/x86" "libtfliteextern.so" HAVE_ANDROID_X86_LITE OFF)
CHECK_BINARY_EXIST("lib/android/x86_64" "libtfextern.so" HAVE_ANDROID_X86_64 OFF)
CHECK_BINARY_EXIST("lib/android/x86_64" "libtfliteextern.so" HAVE_ANDROID_X86_64_LITE OFF)

SET(HAVE_LINUX OFF)
IF(HAVE_LINUX_X86_64 OR HAVE_LINUX_ARM)
  SET(HAVE_LINUX ON)
  MESSAGE(STATUS "HAVE_LINUX: ${HAVE_LINUX}")
ENDIF()

SET(HAVE_LINUX_LITE OFF)
IF(HAVE_LINUX_X86_64_LITE OR HAVE_LINUX_ARM_LITE)
  SET(HAVE_LINUX_LITE ON)
  MESSAGE(STATUS "HAVE_LINUX_LITE: ${HAVE_LINUX_LITE}")
ENDIF()

SET(HAVE_ANDROID OFF)
#IF(HAVE_ANDROID_ARM64_V8A OR HAVE_ANDROID_ARMEABI OR HAVE_ANDROID_ARMEABI_V7A OR HAVE_ANDROID_X86 OR HAVE_ANDROID_X86_64)
IF(HAVE_ANDROID_ARM64_V8A OR HAVE_ANDROID_ARMEABI_V7A OR HAVE_ANDROID_X86 OR HAVE_ANDROID_X86_64)
  SET(HAVE_ANDROID ON)
  MESSAGE(STATUS "HAVE_ANDROID: ${HAVE_ANDROID}")
ENDIF()

SET(HAVE_ANDROID_LITE OFF)
#IF(HAVE_ANDROID_ARM64_V8A_LITE OR HAVE_ANDROID_ARMEABI_LITE OR HAVE_ANDROID_ARMEABI_V7A_LITE OR HAVE_ANDROID_X86_LITE OR HAVE_ANDROID_X86_64_LITE)
IF(HAVE_ANDROID_ARM64_V8A_LITE OR HAVE_ANDROID_ARMEABI_V7A_LITE OR HAVE_ANDROID_X86_LITE OR HAVE_ANDROID_X86_64_LITE)
  SET(HAVE_ANDROID_LITE ON) 
ENDIF()
IF(HAVE_ANDROID_LITE)
  MESSAGE(STATUS "HAVE_ANDROID_LITE: ${HAVE_ANDROID_LITE}")
ENDIF()

#CHECK_BINARY_EXIST("lib/x86" "tfextern.dll" HAVE_WINDESKTOP_X86 OFF)
CHECK_BINARY_EXIST("lib/x64" "tfextern.dll" HAVE_WINDESKTOP_X64 OFF)
#CHECK_BINARY_EXIST("lib/x86" "tfliteextern.dll" HAVE_WINDESKTOP_X86_LITE OFF)
CHECK_BINARY_EXIST("lib/x64" "tfliteextern.dll" HAVE_WINDESKTOP_X64_LITE OFF)
CHECK_BINARY_EXIST("lib/x64" "cudart64*.dll" HAVE_WINDESKTOP_X64_CUDA OFF)

IF(HAVE_WINDESKTOP_X64_CUDA)
  SET(WINDESKTOP_LABEL "-windesktop_x64-cuda")
ELSEIF(HAVE_WINDESKTOP_X64)	
  SET(WINDESKTOP_LABEL "-windesktop_x64")
ELSE() 
  SET(WINDESKTOP_LABEL "")
ENDIF()

SET(HAVE_WINDESKTOP OFF)
IF(HAVE_WINDESKTOP_X64 OR HAVE_WINDESKTOP_X64_CUDA)
  SET(HAVE_WINDESKTOP ON)
  MESSAGE(STATUS "HAVE_WINDESKTOP: ${HAVE_WINDESKTOP}")
ENDIF()


IF (HAVE_WINDESKTOP_X64_LITE)
  SET(WINDESKTOP_LITE_LABEL "-windesktop_x64")
ENDIF()
SET(HAVE_WINDESKTOP_LITE OFF)
IF(HAVE_WINDESKTOP_X64_LITE)
  SET(HAVE_WINDESKTOP_LITE ON) 
  MESSAGE(STATUS "HAVE_WINDESKTOP_LITE: ${HAVE_WINDESKTOP_LITE}")
ENDIF()

SET(WINDOWS_LABEL "${WINDESKTOP_LABEL}")
IF ("${WINDOWS_LABEL}" STREQUAL "-UWP-windesktop")
  SET(WINDOWS_LABEL "-windows")
ENDIF()

#SET(IOS_LABEL "")
#IF(HAVE_IOS)
#  #MESSAGE(STATUS "HAVE_IOS: ${HAVE_IOS}")
#  SET(IOS_LABEL "-ios")
#ENDIF()

SET(IOS_LITE_LABEL "")
IF(HAVE_IOS_LITE)
  #MESSAGE(STATUS "HAVE_IOS_LITE: ${HAVE_IOS_LITE}")
  SET(IOS_LITE_LABEL "-ios")
ENDIF()

SET(OSX_LABEL "")
IF(HAVE_OSX)
  #MESSAGE(STATUS "HAVE_OSX: ${HAVE_OSX}")
  SET(OSX_LABEL "-osx")
ENDIF()	

SET(OSX_LITE_LABEL "")
IF(HAVE_OSX_LITE)
  #MESSAGE(STATUS "HAVE_OSX_LITE: ${HAVE_OSX_LITE}")
  SET(OSX_LITE_LABEL "-osx")
ENDIF()

SET(ANDROID_LABEL "")
IF(HAVE_ANDROID_ARM64_V8A)
  SET(ANDROID_LABEL "${ANDROID_LABEL}-arm64-v8a")
ENDIF()
#IF(HAVE_ANDROID_ARMEABI)
#  SET(ANDROID_LABEL "${ANDROID_LABEL}-armeabi")
#ENDIF()
IF(HAVE_ANDROID_ARMEABI_V7A)	
  SET(ANDROID_LABEL "${ANDROID_LABEL}-armeabi-v7a")
ENDIF()
IF(HAVE_ANDROID_X86)
  SET(ANDROID_LABEL "${ANDROID_LABEL}-x86")
ENDIF()
IF(HAVE_ANDROID_X86_64)
  SET(ANDROID_LABEL "${ANDROID_LABEL}-x86_64")
ENDIF()

SET(ANDROID_LITE_LABEL "")
IF(HAVE_ANDROID_ARM64_V8A_LITE)
  SET(ANDROID_LITE_LABEL "${ANDROID_LITE_LABEL}-arm64-v8a")
ENDIF()
#IF(HAVE_ANDROID_ARMEABI_LITE)
#  SET(ANDROID_LITE_LABEL "${ANDROID_LITE_LABEL}-armeabi")
#ENDIF()
IF(HAVE_ANDROID_ARMEABI_V7A_LITE)	
  SET(ANDROID_LITE_LABEL "${ANDROID_LITE_LABEL}-armeabi-v7a")
ENDIF()
IF(HAVE_ANDROID_X86_LITE)
  SET(ANDROID_LITE_LABEL "${ANDROID_LITE_LABEL}-x86")
ENDIF()
IF(HAVE_ANDROID_X86_64_LITE)
  SET(ANDROID_LITE_LABEL "${ANDROID_LITE_LABEL}-x86_64")
ENDIF()

CHECK_BINARY_EXIST("bin" "Emgu.TF.Runtime.*" HAVE_PREBUILD_EMGU_TF_WORLD OFF)

#IF("${ANDROID_LABEL}" STREQUAL "-arm64-v8a-armeabi-armeabi-v7a-x86-x86_64")
IF("${ANDROID_LABEL}" STREQUAL "-arm64-v8a-armeabi-v7a-x86-x86_64")
  SET(ANDROID_LABEL "-android")
ELSEIF(NOT ("${ANDROID_LABEL}" STREQUAL ""))
  SET(ANDROID_LABEL "-android${ANDROID_LABEL}")
ENDIF()

#IF("${ANDROID_LITE_LABEL}" STREQUAL "-arm64-v8a-armeabi-armeabi-v7a-x86-x86_64")
IF("${ANDROID_LITE_LABEL}" STREQUAL "-arm64-v8a-armeabi-v7a-x86-x86_64")
  SET(ANDROID_LITE_LABEL "-android")
ELSEIF(NOT ("${ANDROID_LITE_LABEL}" STREQUAL ""))
  SET(ANDROID_LITE_LABEL "-android${ANDROID_LITE_LABEL}")
ENDIF()

SET(HAVE_FULL_BUILD OFF)
IF(HAVE_ANDROID OR HAVE_OSX OR HAVE_WINDESKTOP_X64 OR HAVE_WINDESKTOP_X64_CUDA OR HAVE_LINUX)
  SET(HAVE_FULL_BUILD ON)
  MESSAGE(STATUS "Have Tensorflow full build: ${HAVE_FULL_BUILD}")
ENDIF()

SET(HAVE_LITE_BUILD OFF)
IF(HAVE_ANDROID_LITE OR HAVE_IOS_LITE OR HAVE_OSX_LITE OR HAVE_WINDESKTOP_LITE OR HAVE_LINUX_LITE)
  SET(HAVE_LITE_BUILD ON)
  MESSAGE(STATUS "Have Tensorflow Lite build: ${HAVE_LITE_BUILD}")
ENDIF()

if ("${LICENSE_TYPE}" STREQUAL "OpenSource")
  SET(LICENSE_LABEL "")
ELSE()
  SET(LICENSE_LABEL "-cr")
ENDIF()
	
# ----------------------------------------------------------------------------
#  Setup Package information
# ----------------------------------------------------------------------------
SET(PACKAGE "${PROJECT_NAME}")
SET(CPACK_PACKAGE_CONTACT "Emgu TF SUPPORT <support@emgu.com>")
SET(PACKAGE_BUGREPORT "${CPACK_PACKAGE_CONTACT}")
SET(PACKAGE_NAME "${PROJECT_NAME}")

SET(CPACK_PACKAGE_DESCRIPTION_SUMMARY "Emgu TF is a cross platform .Net wrapper to the Google Tensorflow library.")
SET(FULL_BUILD_LABEL "")
IF(HAVE_FULL_BUILD)
  SET(FULL_BUILD_LABEL "${ANDROID_LABEL}${WINDOWS_LABEL}${OSX_LABEL}")
  IF("${FULL_BUILD_LABEL}" STREQUAL "-android-windesktop_x64-osx")
    SET(FULL_BUILD_LABEL "-pro")
  ENDIF()
  SET(FULL_BUILD_LABEL "tf${FULL_BUILD_LABEL}")
ENDIF()
SET(LITE_BUILD_LABEL "")
IF(HAVE_LITE_BUILD)
  SET(LITE_BUILD_LABEL "${IOS_LITE_LABEL}${ANDROID_LITE_LABEL}${WINDESKTOP_LITE_LABEL}${OSX_LITE_LABEL}")
  IF("${LITE_BUILD_LABEL}" STREQUAL "-ios-android-windesktop_x64-osx")
    SET(LITE_BUILD_LABEL "-pro")
  ENDIF()
  SET(LITE_BUILD_LABEL "tflite${LITE_BUILD_LABEL}")
ENDIF()

IF(HAVE_FULL_BUILD AND HAVE_LITE_BUILD)
  SET(CPACK_PACKAGE_NAME "emgu${FULL_BUILD_LABEL}-${LITE_BUILD_LABEL}")
ELSE()
  SET(CPACK_PACKAGE_NAME "emgu${FULL_BUILD_LABEL}${LITE_BUILD_LABEL}")
ENDIF()

#SET(CPACK_PACKAGE_NAME "${PACKAGE_NAME}")

#MESSAGE(STATUS "TENSORFLOW_VERSION_MAJOR: ${TENSORFLOW_VERSION_MAJOR}")
SET(CPACK_PACKAGE_VENDOR "Emgu Corporation")
SET(CPACK_PACKAGE_VERSION_MAJOR ${TENSORFLOW_VERSION_MAJOR})
SET(CPACK_PACKAGE_VERSION_MINOR ${TENSORFLOW_VERSION_MINOR})
SET(CPACK_PACKAGE_VERSION_PATCH ${TENSORFLOW_VERSION_PATCH})
SET(CPACK_PACKAGE_VERSION "${CPACK_PACKAGE_VERSION_MAJOR}.${CPACK_PACKAGE_VERSION_MINOR}.${CPACK_PACKAGE_VERSION_PATCH}.${EMGUTF_GITVERSION}")
#MESSAGE(STATUS "CPACK_PACKAGE_VERSION: ${CPACK_PACKAGE_VERSION}")

SET(CPACK_PACKAGE_FILE_NAME "lib${CPACK_PACKAGE_NAME}-${CPACK_PACKAGE_VERSION}${LICENSE_LABEL}")

MESSAGE(STATUS "CPACK_PACKAGE_FILE_NAME is ${CPACK_PACKAGE_FILE_NAME}")
FILE(WRITE "emgutf.version.txt" "${CPACK_PACKAGE_VERSION}${LICENSE_LABEL}")

SET(CPACK_RESOURCE_FILE_LICENSE "${CMAKE_CURRENT_SOURCE_DIR}/${LICENSE_FILE_NAME}")

IF (WIN32 OR APPLE)
  SET(CPACK_PACKAGE_CLI_FOLDER bin)
ELSE ()
  SET(CPACK_PACKAGE_CLI_FOLDER lib/cli/${PACKAGE_NAME}-${CPACK_PACKAGE_VERSION_MAJOR}.${CPACK_PACKAGE_VERSION_MINOR})
ENDIF()


SET(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${LIBRARY_OUTPUT_PATH}" )
SET(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELEASE "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}")
SET(CMAKE_RUNTIME_OUTPUT_DIRECTORY_DEBUG "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}")

SET(EMGU_TF_BUILD ON CACHE BOOL "BUILD Emgu TF")
SET(EMGU_TF_EXAMPLE_BUILD ON CACHE BOOL "Build Emgu TF Example")

# ----------------------------------------------------------------------------
#  Generate required common assembly file for Emgu TF project
# ----------------------------------------------------------------------------
INCLUDE(Today)
TODAY() #set the variable CURRENT_YEAR

CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/CommonAssemblyInfo.cs.in ${CMAKE_CURRENT_SOURCE_DIR}/CommonAssemblyInfo.cs)

CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Directory.Build.props.in ${CMAKE_CURRENT_SOURCE_DIR}/Directory.Build.props)

SET (EMGUTF_LICENSE_FILE "${LICENSE_FILE_NAME}.txt")
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/${LICENSE_FILE_NAME} ${CMAKE_CURRENT_SOURCE_DIR}/${EMGUTF_LICENSE_FILE} COPYONLY)

# ----------------------------------------------------------------------------
#  Generate xamarin component configuration file
# ----------------------------------------------------------------------------
#CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/component.yaml.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/xamarin/emgutf_v1/component/component.yaml)

SET(EMGUTF_NUGET_FILE_LIST "")

SET(EMGUTF_NUGET_BASE_ID "Emgu.TF")
SET(EMGUTF_NUGET_ID "${EMGUTF_NUGET_BASE_ID}")
SET(EMGUTF_MODEL_NUGET_ID "${EMGUTF_NUGET_BASE_ID}.Models")
SET(EMGUTF_PROTOBUF_NUGET_ID "${EMGUTF_NUGET_BASE_ID}.Protobuf")

SET(EMGUTF_NUGET_TITLE_ARCH "")
IF(HAVE_WINDESKTOP_X86)
 SET(EMGUTF_NUGET_TITLE_ARCH "${EMGUTF_NUGET_TITLE_ARCH} x86")
ENDIF()
IF(HAVE_WINDESKTOP_X64)
 SET(EMGUTF_NUGET_TITLE_ARCH "${EMGUTF_NUGET_TITLE_ARCH} x64")
ENDIF()
STRING(STRIP "${EMGUTF_NUGET_TITLE_ARCH}" EMGUTF_NUGET_TITLE_ARCH)
STRING(REPLACE "x86 x64" "" EMGUTF_NUGET_TITLE_ARCH "${EMGUTF_NUGET_TITLE_ARCH}")
 
SET(EMGUTF_NUGET_TITLE "Emgu TF ${EMGUTF_NUGET_TITLE_ARCH}")
STRING(STRIP "${EMGUTF_NUGET_TITLE}" EMGUTF_NUGET_TITLE)

SET(EMGUTF_MODEL_NUGET_TITLE "Emgu TF Models")
SET(EMGUTF_PROTOBUF_NUGET_TITLE "Emgu TF Protobuf")

IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
  SET(EMGUTF_NUGET_ID "${EMGUTF_NUGET_ID}")
  SET(EMGUTF_MODEL_NUGET_ID "${EMGUTF_MODEL_NUGET_ID}")
  SET(EMGUTF_PROTOBUF_NUGET_ID "${EMGUTF_PROTOBUF_NUGET_ID}")
   
  #SET(EMGUTF_NUGET_TITLE "${EMGUTF_NUGET_TITLE} Commercial Release")
  #SET(EMGUTF_MODEL_NUGET_TITLE "${EMGUTF_MODEL_NUGET_TITLE} Commercial Release")
  #SET(EMGUTF_PROTOBUF_NUGET_TITLE "${EMGUTF_PROTOBUF_NUGET_TITLE} Commercial Release")
ENDIF()

SET(EMGUTF_WINDOWS_NUGET_FILE_LIST "")

IF(HAVE_WINDESKTOP)
  IF(HAVE_WINDESKTOP_X64_CUDA)
    SET(EMGUTF_WINDOWS_NUGET_ID "${EMGUTF_NUGET_BASE_ID}.runtime.windows.cuda")  
    SET(EMGUTF_WINDOWS_NUGET_TITLE "Native runtime of ${EMGUTF_NUGET_TITLE} for Windows with CUDA support")	
  ELSE()
    SET(EMGUTF_WINDOWS_NUGET_ID "${EMGUTF_NUGET_BASE_ID}.runtime.windows")  
    SET(EMGUTF_WINDOWS_NUGET_TITLE "Native runtime of ${EMGUTF_NUGET_TITLE} for Windows")	
  ENDIF()

  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_WINDOWS_NUGET_ID "${EMGUTF_WINDOWS_NUGET_ID}-CR")
  ENDIF()
  SET(EMGUTF_WINDOWS_NUGET_FILE_LIST "${EMGUTF_WINDOWS_NUGET_FILE_LIST}  
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"Emgu.TF.runtime.windows.targets\" target=\"build\\${EMGUTF_WINDOWS_NUGET_ID}.targets\" />")
  SET(EMGUTF_WINDOWS_NUGET_DEPENDENCY "${EMGUTF_WINDOWS_NUGET_DEPENDENCY}
		<dependency id=\"${EMGUTF_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />") 
ENDIF()


IF(HAVE_ANDROID)
  SET(EMGUTF_ANDROID_NUGET_ID "${EMGUTF_NUGET_BASE_ID}.runtime.android")
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_ANDROID_NUGET_ID "${EMGUTF_ANDROID_NUGET_ID}-CR")  
  ENDIF()
  SET(EMGUTF_ANDROID_NUGET_TITLE "Native runtime of ${EMGUTF_NUGET_TITLE} for Android")	
  SET(EMGUTF_ANDROID_NUGET_FILE_LIST " 
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Runtime.Android.dll\" target=\"lib\\MonoAndroid50\" />
	<file src=\"..\\..\\..\\lib\\Emgu.TF.Runtime.Android.xml\" target=\"lib\\MonoAndroid50\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />")
  SET(EMGUTF_ANDROID_NUGET_DEPENDENCY "${EMGUTF_ANDROID_NUGET_DEPENDENCY}
    <dependencies>
		<dependency id=\"${EMGUTF_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
    </dependencies>") 
ENDIF()

IF(HAVE_OSX)
  SET(EMGUTF_MACOS_NUGET_ID "${EMGUTF_NUGET_BASE_ID}.runtime.mac")
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_MACOS_NUGET_ID "${EMGUTF_MACOS_NUGET_ID}-CR")  
  ENDIF()
  SET(EMGUTF_MACOS_NUGET_TITLE "Native runtime of ${EMGUTF_NUGET_TITLE} for MacOS")	
  SET(EMGUTF_MACOS_NUGET_FILE_LIST " 
    <file src=\"..\\..\\..\\lib\\osx\\*.dylib\" target=\"build\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />
	<file src=\"Emgu.TF.runtime.macos.targets\" target=\"build\\${EMGUTF_MACOS_NUGET_ID}.targets\" />")
  SET(EMGUTF_MACOS_NUGET_DEPENDENCY "${EMGUTF_MACOS_NUGET_DEPENDENCY}
    <dependencies>
		<dependency id=\"${EMGUTF_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
    </dependencies>") 
ENDIF()

IF(HAVE_LINUX_X86_64)
  SET(EMGUTF_UBUNTU_NUGET_ID "${EMGUTF_NUGET_BASE_ID}.runtime.ubuntu")
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_UBUNTU_NUGET_ID "${EMGUTF_UBUNTU_NUGET_ID}-CR")  
  ENDIF()
  SET(EMGUTF_UBUNTU_NUGET_TITLE "Native runtime of ${EMGUTF_NUGET_TITLE} for Ubuntu")
  SET(EMGUTF_UBUNTU_NUGET_FILE_LIST " 
    <file src=\"..\\..\\..\\lib\\x64\\*.so\" target=\"build\\x64\" />
    <file src=\"..\\..\\..\\lib\\x64\\*.so.1\" target=\"build\\x64\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />
	<file src=\"Emgu.TF.runtime.ubuntu.targets\" target=\"build\\${EMGUTF_UBUNTU_NUGET_ID}.targets\" />")
  SET(EMGUTF_UBUNTU_NUGET_DEPENDENCY "${EMGUTF_UBUNTU_NUGET_DEPENDENCY}
    <dependencies>
		<dependency id=\"${EMGUTF_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
    </dependencies>")   
ENDIF()

IF(HAVE_LINUX_ARM)
  SET(EMGUTF_RASPBIAN_NUGET_ID "${EMGUTF_NUGET_BASE_ID}.runtime.raspbian")
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_RASPBIAN_NUGET_ID "${EMGUTF_RASPBIAN_NUGET_ID}-CR")  
  ENDIF()
  SET(EMGUTF_RASPBIAN_NUGET_TITLE "Native runtime of ${EMGUTF_NUGET_TITLE} for Raspian")		
  SET(EMGUTF_RASPBIAN_NUGET_FILE_LIST " 
    <file src=\"..\\..\\..\\lib\\arm\\*.so\" target=\"build\\arm\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />
	<file src=\"Emgu.TF.runtime.raspbian.targets\" target=\"build\\${EMGUTF_RASPBIAN_NUGET_ID}.targets\" />")
  SET(EMGUTF_RASPBIAN_NUGET_DEPENDENCY "${EMGUTF_RASPBIAN_NUGET_DEPENDENCY}
    <dependencies>
		<dependency id=\"${EMGUTF_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
    </dependencies>")
ENDIF()

IF(HAVE_LITE_BUILD)
  SET(EMGUTF_LITE_NUGET_ID "Emgu.TF.Lite")
  SET(EMGUTF_LITE_MODEL_NUGET_ID "${EMGUTF_LITE_NUGET_ID}.Models")
  
  
  SET(EMGUTF_LITE_WINDOWS_NUGET_ID "${EMGUTF_LITE_NUGET_ID}.runtime.windows")  
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_LITE_WINDOWS_NUGET_ID "${EMGUTF_LITE_WINDOWS_NUGET_ID}-CR")  
  ENDIF()
  SET(EMGUTF_LITE_WINDOWS_NUGET_TITLE "Native runtime of ${EMGUTF_LITE_NUGET_TITLE} for Windows")	
  
  SET(EMGUTF_LITE_MACOS_NUGET_ID "${EMGUTF_LITE_NUGET_ID}.runtime.mac")
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_LITE_MACOS_NUGET_ID "${EMGUTF_LITE_MACOS_NUGET_ID}-CR")  
  ENDIF()
  SET(EMGUTF_LITE_MACOS_NUGET_TITLE "Native runtime of ${EMGUTF_LITE_NUGET_TITLE} for MacOS")	
  
  SET(EMGUTF_LITE_UBUNTU_NUGET_ID "${EMGUTF_LITE_NUGET_ID}.runtime.ubuntu") 
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_LITE_UBUNTU_NUGET_ID "${EMGUTF_LITE_UBUNTU_NUGET_ID}-CR")  
  ENDIF()  
  SET(EMGUTF_LITE_UBUNTU_NUGET_TITLE "Native runtime of ${EMGUTF_LITE_NUGET_TITLE} for Ubuntu")	
  
  SET(EMGUTF_LITE_RASPBIAN_NUGET_ID "${EMGUTF_LITE_NUGET_ID}.runtime.raspbian")
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_LITE_RASPBIAN_NUGET_ID "${EMGUTF_LITE_RASPBIAN_NUGET_ID}-CR")  
  ENDIF()
  SET(EMGUTF_LITE_RASPBIAN_NUGET_TITLE "Native runtime of ${EMGUTF_LITE_NUGET_TITLE} for Raspian")	

  SET(EMGUTF_LITE_ANDROID_NUGET_ID "${EMGUTF_LITE_NUGET_ID}.runtime.android")  
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_LITE_ANDROID_NUGET_ID "${EMGUTF_LITE_ANDROID_NUGET_ID}-CR")  
  ENDIF()
  SET(EMGUTF_LITE_ANDROID_NUGET_TITLE "Native runtime of ${EMGUTF_LITE_NUGET_TITLE} for Android")	
  
  SET(EMGUTF_LITE_IOS_NUGET_ID "${EMGUTF_LITE_NUGET_ID}.runtime.ios")  
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    SET(EMGUTF_LITE_IOS_NUGET_ID "${EMGUTF_LITE_IOS_NUGET_ID}-CR")  
  ENDIF()
  SET(EMGUTF_LITE_IOS_NUGET_TITLE "Native runtime of ${EMGUTF_LITE_NUGET_TITLE} for iOS")	

  SET(EMGUTF_LITE_NUGET_TITLE "Emgu TF Lite")
  SET(EMGUTF_LITE_MODEL_NUGET_TITLE "Emgu TF Lite Models")
  #MESSAGE(STATUS ">>> HAVE_WINDESKTOP_LITE: ${HAVE_WINDESKTOP_LITE}")
  
  IF ("${LICENSE_TYPE}" STREQUAL "Commercial")
    #MESSAGE(STATUS ">>> TF Lite Commercial license")
    SET(EMGUTF_LITE_NUGET_ID "${EMGUTF_LITE_NUGET_ID}-CR")
    SET(EMGUTF_LITE_MODEL_NUGET_ID "${EMGUTF_LITE_MODEL_NUGET_ID}-CR")
    
    SET(EMGUTF_LITE_NUGET_TITLE "${EMGUTF_LITE_NUGET_TITLE} Commercial Release")
    SET(EMGUTF_LITE_MODEL_NUGET_TITLE "${EMGUTF_LITE_MODEL_NUGET_TITLE} Commercial Release")
  ENDIF()  
  
  SET(EMGUTF_LITE_NUGET_FILE_LIST "${EMGUTF_LITE_NUGET_FILE_LIST} 
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Netstandard.dll\" target=\"lib\\netstandard2.0\" />
	<file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Netstandard.xml\" target=\"lib\\netstandard2.0\" />")
  SET(EMGUTF_LITE_NUGET_FILE_LIST "${EMGUTF_LITE_NUGET_FILE_LIST}
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />")
  
  SET(EMGUTF_LITE_MODEL_NUGET_FILE_LIST "${EMGUTF_LITE_MODEL_NUGET_FILE_LIST} 
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Models.Netstandard.dll\" target=\"lib\\netstandard2.0\" />
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Models.Netstandard.xml\" target=\"lib\\netstandard2.0\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
    <file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />")
  SET(EMGUTF_LITE_MODEL_NUGET_DEPENDENCY "${EMGUTF_LITE_MODEL_NUGET_DEPENDENCY}
    <dependencies>
		<dependency id=\"${EMGUTF_LITE_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
    </dependencies>")
ENDIF()

IF( HAVE_WINDESKTOP_LITE )
  SET(EMGUTF_LITE_WINDOWS_NUGET_FILE_LIST "${EMGUTF_LITE_WINDOWS_NUGET_FILE_LIST} 
    <file src=\"..\\..\\..\\lib\\x64\\*.dll\" target=\"build\\x64\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"Emgu.TF.Lite.runtime.windows.targets\" target=\"build\\${EMGUTF_LITE_WINDOWS_NUGET_ID}.targets\" />")
  SET(EMGUTF_LITE_WINDOWS_NUGET_DEPENDENCY "${EMGUTF_LITE_WINDOWS_NUGET_DEPENDENCY}
    <dependencies>
		<dependency id=\"${EMGUTF_LITE_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
    </dependencies>")
ENDIF()

IF( HAVE_OSX_LITE ) 
  SET(EMGUTF_LITE_MODEL_NUGET_FILE_LIST "${EMGUTF_LITE_MODEL_NUGET_FILE_LIST}
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Models.Mac.dll\" target=\"lib\\xamarinmac20\" />
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Models.Mac.xml\" target=\"lib\\xamarinmac20\" />")
  SET(EMGUTF_LITE_MACOS_NUGET_FILE_LIST "${EMGUTF_LITE_MACOS_NUGET_FILE_LIST} 
    <file src=\"..\\..\\..\\lib\\osx\\*.dylib\" target=\"build\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"Emgu.TF.Lite.runtime.macos.targets\" target=\"build\\${EMGUTF_LITE_MACOS_NUGET_ID}.targets\" />")
  SET(EMGUTF_LITE_MACOS_NUGET_DEPENDENCY "${EMGUTF_LITE_MACOS_NUGET_DEPENDENCY}
    <dependencies>
		<dependency id=\"${EMGUTF_LITE_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
    </dependencies>")	
ENDIF()
IF (HAVE_IOS_LITE)
  SET(EMGUTF_LITE_IOS_NUGET_FILE_LIST "${EMGUTF_LITE_IOS_NUGET_FILE_LIST}
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Runtime.iOS.dll\" target=\"lib\\Xamarin.iOS10\" />
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Runtime.iOS.xml\" target=\"lib\\Xamarin.iOS10\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />")
ENDIF()
IF(HAVE_ANDROID_LITE)
  SET(EMGUTF_LITE_MODEL_NUGET_FILE_LIST "${EMGUTF_LITE_MODEL_NUGET_FILE_LIST}
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Models.Android.dll\" target=\"lib\\MonoAndroid90\" />
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Models.Android.xml\" target=\"lib\\MonoAndroid90\" />")
  SET(EMGUTF_LITE_ANDROID_NUGET_FILE_LIST "${EMGUTF_LITE_ANDROID_NUGET_FILE_LIST}
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Runtime.Android.dll\" target=\"lib\\MonoAndroid90\" />
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Lite.Runtime.Android.xml\" target=\"lib\\MonoAndroid90\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />")
  SET(EMGUTF_LITE_ANDROID_NUGET_DEPENDENCY "${EMGUTF_LITE_ANDROID_NUGET_DEPENDENCY}
    <dependencies>
		<dependency id=\"${EMGUTF_LITE_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
    </dependencies>")
ENDIF()
IF (HAVE_LINUX_X86_64_LITE)
  SET(EMGUTF_LITE_UBUNTU_NUGET_FILE_LIST "${EMGUTF_LITE_UBUNTU_NUGET_FILE_LIST} 
    <file src=\"..\\..\\..\\lib\\x64\\*.so\" target=\"build\\x64\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />
	<file src=\"Emgu.TF.Lite.runtime.ubuntu.targets\" target=\"build\\${EMGUTF_LITE_UBUNTU_NUGET_ID}.targets\" />")
  SET(EMGUTF_LITE_UBUNTU_NUGET_DEPENDENCY "${EMGUTF_LITE_UBUNTU_NUGET_DEPENDENCY}
    <dependencies>
		<dependency id=\"${EMGUTF_LITE_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
    </dependencies>")	
ENDIF()
IF (HAVE_LINUX_ARM_LITE)
  SET(EMGUTF_LITE_RASPBIAN_NUGET_FILE_LIST "${EMGUTF_LITE_RASPBIAN_NUGET_FILE_LIST} 
    <file src=\"..\\..\\..\\lib\\arm\\*.so\" target=\"build\\arm\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"Emgu.TF.Lite.runtime.raspbian.targets\" target=\"build\\${EMGUTF_LITE_RASPBIAN_NUGET_ID}.targets\" />")
  SET(EMGUTF_LITE_RASPBIAN_NUGET_DEPENDENCY "${EMGUTF_LITE_RASPBIAN_NUGET_DEPENDENCY}
    <dependencies>
		<dependency id=\"${EMGUTF_LITE_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
    </dependencies>")	
ENDIF()

SET(EMGUTF_NUGET_DEPENDENCY "")
SET(EMGUTF_PROTOBUF_NUGET_DEPENDENCY "")

IF(HAVE_FULL_BUILD)
  SET(EMGUTF_NUGET_FILE_LIST "${EMGUTF_NUGET_FILE_LIST} 
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Netstandard.dll\" target=\"lib\\netstandard2.0\" />
	<file src=\"..\\..\\..\\lib\\Emgu.TF.Netstandard.xml\" target=\"lib\\netstandard2.0\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
	<file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />")
ENDIF()

IF(HAVE_WINDESKTOP)
  #MESSAGE(STATUS "HAVE_WINDESKTOP: ${HAVE_WINDESKTOP}")
  SET(BAZEL_CUDA_TOOLKIT_PATH_FILE "${CMAKE_CURRENT_SOURCE_DIR}/tensorflow/tensorflow/tools/CUDA_TOOLKIT_PATH.txt")
  
  SET(TFEXTERN_DEPENDENCY_DLLS)
  
  #IF(EXISTS ${BAZEL_GENERATED_CUFFT_VERSION_FILE})
  #  SET(BAZEL_GENERATED_CUFFT_VERSION_FILE_FOUND ON)
  #ENDIF()
  #MESSAGE(STATUS ">>>>>>>>> BAZEL_GENERATED_CUFFT_VERSION_FILE ${BAZEL_GENERATED_CUFFT_VERSION_FILE_FOUND}: ${BAZEL_GENERATED_CUFFT_VERSION_FILE}")
  
  IF(EXISTS ${BAZEL_CUDA_TOOLKIT_PATH_FILE})
    SET(CUDA_POSTFIX 64)
    SET(CMAKE_GENERATED_CUFFT_VERSION_FILE "${CMAKE_CURRENT_SOURCE_DIR}/tensorflow/tensorflow/contrib/cmake/emgu.tf.cufft.version.txt")	
    IF(EXISTS ${CMAKE_GENERATED_CUFFT_VERSION_FILE})
      #The native binary is built with CMAKE
      MESSAGE(STATUS "Native tensorflow is build with cuda and cmake")
      FILE(STRINGS "${CMAKE_GENERATED_CUFFT_VERSION_FILE}" EMGU_TF_CUFFT_VERSION)
      STRING(REPLACE "." "" EMGU_TF_CUFFT_FILE_VERSION "${EMGU_TF_CUFFT_VERSION}")
    ELSE()	
      #The native binary is built with bazel
      MESSAGE(STATUS "Native tensorflow is build with cuda and bazel")
      FILE(STRINGS "${BAZEL_CUDA_TOOLKIT_PATH_FILE}" BAZEL_CUDA_TOOLKIT_PATH_RAW)
      #STRING(REGEX REPLACE "\n" "" BAZEL_CUDA_TOOLKIT_PATH "${BAZEL_CUDA_TOOLKIT_PATH}")
      STRING(STRIP "${BAZEL_CUDA_TOOLKIT_PATH_RAW}" BAZEL_CUDA_TOOLKIT_PATH)
      #MESSAGE(STATUS "The Cuda path is: '${BAZEL_CUDA_TOOLKIT_PATH}'")
	  SET(BAZEL_GENERATED_DNN_VERSION_FILE "${BAZEL_CUDA_TOOLKIT_PATH}/include/cudnn.h")
	  
      
      IF(EXISTS "${BAZEL_CUDA_TOOLKIT_PATH}/include/nppversion.h")
	  SET(BAZEL_GENERATED_CUFFT_VERSION_FILE "${BAZEL_CUDA_TOOLKIT_PATH}/include/nppversion.h")
	  file( STRINGS "${BAZEL_GENERATED_CUFFT_VERSION_FILE}" npp_major REGEX "#define NPP_VERSION_MAJOR.*")
	  file( STRINGS "${BAZEL_GENERATED_CUFFT_VERSION_FILE}" npp_minor REGEX "#define NPP_VERSION_MINOR.*")
	  file( STRINGS "${BAZEL_GENERATED_CUFFT_VERSION_FILE}" npp_build REGEX "#define NPP_VERSION_BUILD.*")
	  
	  string( REGEX REPLACE "#define NPP_VERSION_MAJOR[ \t]+|//.*" "" npp_major ${npp_major})
	  string( REGEX REPLACE "#define NPP_VERSION_MINOR[ \t]+|//.*" "" npp_minor ${npp_minor})
	  string( REGEX REPLACE "#define NPP_VERSION_BUILD[ \t]+|//.*" "" npp_build ${npp_build})
	  
	  string( REGEX MATCH "[0-9]+" npp_major ${npp_major} ) 
	  string( REGEX MATCH "[0-9]+" npp_minor ${npp_minor} ) 
	  string( REGEX MATCH "[0-9]+" npp_build ${npp_build} )
	  ELSEIF(EXISTS "${BAZEL_CUDA_TOOLKIT_PATH}/include/npp.h")
	  SET(BAZEL_GENERATED_CUFFT_VERSION_FILE "${BAZEL_CUDA_TOOLKIT_PATH}/include/npp.h")
	  file( STRINGS "${BAZEL_GENERATED_CUFFT_VERSION_FILE}" npp_major REGEX "#define NPP_VER_MAJOR.*")
	  file( STRINGS "${BAZEL_GENERATED_CUFFT_VERSION_FILE}" npp_minor REGEX "#define NPP_VER_MINOR.*")
	  file( STRINGS "${BAZEL_GENERATED_CUFFT_VERSION_FILE}" npp_build REGEX "#define NPP_VER_BUILD.*")
	  
	  string( REGEX REPLACE "#define NPP_VERSION_MAJOR[ \t]+|//.*" "" npp_major ${npp_major})
	  string( REGEX REPLACE "#define NPP_VERSION_MINOR[ \t]+|//.*" "" npp_minor ${npp_minor})
	  string( REGEX REPLACE "#define NPP_VERSION_BUILD[ \t]+|//.*" "" npp_build ${npp_build})
	  
	  string( REGEX MATCH "[0-9]+" npp_major ${npp_major} ) 
	  string( REGEX MATCH "[0-9]+" npp_minor ${npp_minor} ) 
	  string( REGEX MATCH "[0-9]+" npp_build ${npp_build} )
	  ELSE()
	  MESSAGE(FATAL_ERROR "Not able to find NPP header")
	  ENDIF()
      
	  SET(EMGU_TF_CUDA_VERSION "${npp_major}.${npp_minor}.${npp_build}") 
      SET(EMGU_TF_CUFFT_VERSION "${npp_major}.${npp_minor}.${npp_build}") 
      #SET(CUDA_TOOLKIT_ROOT_DIR "C:/Program Files/NVIDIA GPU Computing Toolkit/CUDA/v${npp_major}.${npp_minor}")
      SET(CUDA_TOOLKIT_ROOT_DIR "${BAZEL_CUDA_TOOLKIT_PATH}")
      
      IF((${npp_major} GREATER 10) OR (("${npp_major}" STREQUAL "10") AND (${npp_minor} GREATER 0)))
	    SET(NPP_POSTFIX "${npp_major}")
	    LIST(APPEND TFEXTERN_DEPENDENCY_DLLS 
	      "${CUDA_TOOLKIT_ROOT_DIR}/bin/cublasLt${CUDA_POSTFIX}_${NPP_POSTFIX}.dll")
      ELSE()
	    SET(NPP_POSTFIX "${npp_major}${npp_minor}")
      ENDIF()
	  
      FILE(GLOB CUDART_FILES "${CUDA_TOOLKIT_ROOT_DIR}/bin/cudart${CUDA_POSTFIX}_*.dll")
      LIST(APPEND TFEXTERN_DEPENDENCY_DLLS 
	    ${CUDART_FILES}
	    "${CUDA_TOOLKIT_ROOT_DIR}/bin/cufft${CUDA_POSTFIX}_${NPP_POSTFIX}.dll"
	    "${CUDA_TOOLKIT_ROOT_DIR}/bin/cublas${CUDA_POSTFIX}_${NPP_POSTFIX}.dll"
	    "${CUDA_TOOLKIT_ROOT_DIR}/bin/curand${CUDA_POSTFIX}_${NPP_POSTFIX}.dll"
	    "${CUDA_TOOLKIT_ROOT_DIR}/bin/cusolver${CUDA_POSTFIX}_${NPP_POSTFIX}.dll"
		"${CUDA_TOOLKIT_ROOT_DIR}/bin/cusparse${CUDA_POSTFIX}_${NPP_POSTFIX}.dll"
	  )
      SET(EMGU_TF_CUFFT_FILE_VERSION "${NPP_POSTFIX}")
#    ELSE()
#      MESSAGE(FATAL_ERROR "-- Unable to find CUFFT version")
    ENDIF()
    
    MESSAGE(STATUS "CUDA release using CUDA runtime: ${EMGU_TF_CUDA_VERSION}; File Version: ${EMGU_TF_CUFFT_FILE_VERSION}")	
    
    SET(CMAKE_GENERATED_DNN_VERSION_FILE "${CMAKE_CURRENT_SOURCE_DIR}/tensorflow/tensorflow/contrib/cmake/emgu.tf.dnn.version.txt")
    
    IF(EXISTS ${CMAKE_GENERATED_DNN_VERSION_FILE})
      #The native binary is built with CMake
      MESSAGE(STATUS "-- Native tensorflow is build with cudnn and cmake")
      FILE(STRINGS "${CMAKE_CURRENT_SOURCE_DIR}/tensorflow/tensorflow/contrib/cmake/emgu.tf.dnn.version.txt" EMGU_TF_DNN_VERSION)
    ELSEIF(EXISTS ${BAZEL_GENERATED_DNN_VERSION_FILE})
      #The native binary is built with bazel
      MESSAGE(STATUS "-- Native tensorflow is build with cudnn and bazel")
      file( STRINGS "${BAZEL_GENERATED_DNN_VERSION_FILE}" dnn_major REGEX "#define CUDNN_MAJOR.*")
      file( STRINGS "${BAZEL_GENERATED_DNN_VERSION_FILE}" dnn_minor REGEX "#define CUDNN_MINOR.*")
      file( STRINGS "${BAZEL_GENERATED_DNN_VERSION_FILE}" dnn_patchlevel REGEX "#define CUDNN_PATCHLEVEL.*")
      
      string( REGEX REPLACE "#define CUDNN_MAJOR[ \t]+|//.*" "" dnn_major ${dnn_major})
      string( REGEX REPLACE "#define CUDNN_MINOR[ \t]+|//.*" "" dnn_minor ${dnn_minor})
      string( REGEX REPLACE "#define CUDNN_PATCHLEVEL[ \t]+|//.*" "" dnn_patchlevel ${dnn_patchlevel})
      
      string( REGEX MATCH "[0-9]+" dnn_major ${dnn_major} ) 
      string( REGEX MATCH "[0-9]+" dnn_minor ${dnn_minor} ) 
      string( REGEX MATCH "[0-9]+" dnn_patchlevel ${dnn_patchlevel} ) 	
      SET(EMGU_TF_DNN_VERSION "${dnn_major}.${dnn_minor}.${dnn_patchlevel}")
      SET(CUDA_TOOLKIT_ROOT_DIR "C:/Program Files/NVIDIA GPU Computing Toolkit/CUDA/v${npp_major}.${npp_minor}")
      LIST(APPEND TFEXTERN_DEPENDENCY_DLLS "${CUDA_TOOLKIT_ROOT_DIR}/bin/cudnn${CUDA_POSTFIX}_${dnn_major}.dll")
    ENDIF()
    
    STRING(REPLACE "." ";" EMGU_TF_DNN_VERSION_LIST ${EMGU_TF_DNN_VERSION})
    #list(LENGTH EMGU_TF_DNN_VERSION_LIST len)
    LIST(GET EMGU_TF_DNN_VERSION_LIST 0 EMGU_TF_DNN_FILE_VERSION)
    MESSAGE(STATUS "CUDA release using DNN: ${EMGU_TF_DNN_VERSION}; File Version: ${EMGU_TF_DNN_FILE_VERSION}")
    
    IF(DEFINED TFEXTERN_DEPENDENCY_DLLS)
      FOREACH(TFEXTERN_DEPENDENCY_DLL ${TFEXTERN_DEPENDENCY_DLLS})
	#LIST(APPEND TFEXTERN_DEPENDENCY_DLL_DEPLOY_COMMAND COMMAND ${CMAKE_COMMAND} -E copy "${TFEXTERN_DEPENDENCY_DLL}" "${tensorflow_source_dir}/../lib/${TF_LIB_SUBFOLDER}")
	GET_FILENAME_COMPONENT(TFEXTERN_DEPENDENCT_DLL_NAME ${TFEXTERN_DEPENDENCY_DLL} NAME_WE)
	file(COPY "${TFEXTERN_DEPENDENCY_DLL}" DESTINATION "${UNMANAGED_LIBRARY_OUTPUT_PATH}")
	MESSAGE(STATUS "Copying ${TFEXTERN_DEPENDENCY_DLL} to ${UNMANAGED_LIBRARY_OUTPUT_PATH}")
	file(COPY "${TFEXTERN_DEPENDENCY_DLL}" DESTINATION "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF${UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER}")
	MESSAGE(STATUS "Copying ${TFEXTERN_DEPENDENCY_DLL} to ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF${UNMANAGED_LIBRARY_OUTPUT_SUBFOLDER}")
	#LIST(APPEND TFEXTERN_DEPENDENCY_DLL_NAMES ${TFEXTERN_DEPENDENCT_DLL_NAME})
      ENDFOREACH()
    ENDIF()
    
    SET(EMGU_TF_DNN_ID "Emgu.TF.runtime.windows.cuda.dnn")
    SET(EMGU_TF_CUFFT_ID "Emgu.TF.runtime.windows.cuda.cufft")
	SET(EMGU_TF_CUDART_ID "Emgu.TF.runtime.windows.cuda.rt")

	SET(EMGUTF_CUDART_NUGET_DEPENDENCY "${EMGUTF_CUDART_NUGET_DEPENDENCY}
        <dependencies>
			<dependency id=\"${EMGU_TF_DNN_ID}\" version=\"[${EMGU_TF_DNN_VERSION}]\" />
			<dependency id=\"${EMGU_TF_CUFFT_ID}\" version=\"[${EMGU_TF_CUFFT_VERSION}]\" />
        </dependencies>")
    
    CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/${EMGU_TF_DNN_ID}.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/${EMGU_TF_DNN_ID}/Package.nuspec)
    CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/${EMGU_TF_DNN_ID}.targets.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/${EMGU_TF_DNN_ID}/${EMGU_TF_DNN_ID}.targets)
    CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/${EMGU_TF_CUFFT_ID}.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/${EMGU_TF_CUFFT_ID}/Package.nuspec)
    CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/${EMGU_TF_CUFFT_ID}.targets.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/${EMGU_TF_CUFFT_ID}/${EMGU_TF_CUFFT_ID}.targets)
	CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/${EMGU_TF_CUDART_ID}.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/${EMGU_TF_CUDART_ID}/Package.nuspec)
	CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/${EMGU_TF_CUDART_ID}.targets.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/${EMGU_TF_CUDART_ID}/${EMGU_TF_CUDART_ID}.targets)
	
    SET(EMGUTF_WINDOWS_NUGET_DEPENDENCY "${EMGUTF_WINDOWS_NUGET_DEPENDENCY}
			<dependency id=\"${EMGU_TF_CUDART_ID}\" version=\"[${EMGU_TF_CUDA_VERSION}]\" />")
    SET(EMGUTF_WINDOWS_NUGET_FILE_LIST "${EMGUTF_WINDOWS_NUGET_FILE_LIST} 
	<file src=\"..\\..\\..\\lib\\x64\\*.dll\" exclude=\"..\\..\\..\\lib\\x64\\cu*.dll;\" target=\"build\\x64\" />")
  ELSE()
    SET(EMGUTF_WINDOWS_NUGET_FILE_LIST "${EMGUTF_WINDOWS_NUGET_FILE_LIST} 
	<file src=\"..\\..\\..\\lib\\x64\\*.dll\" target=\"build\\x64\" />")
  ENDIF()
  
ENDIF()

SET(EMGUTF_MODEL_NUGET_FILE_LIST "
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Models.Netstandard.dll\" target=\"lib\\netstandard2.0\" />
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Models.Netstandard.xml\" target=\"lib\\netstandard2.0\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
    <file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />")
	
SET(EMGUTF_MODEL_NUGET_DEPENDENCY "${EMGUTF_MODEL_NUGET_DEPENDENCY}
    <group targetFramework=\".NETStandard2.0\">
		<dependency id=\"${EMGUTF_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
		<dependency id=\"System.Drawing.Common\" version=\"4.7.0\" exclude=\"Build,Analyzers\" />
	</group>")

IF( HAVE_OSX ) 
  SET(EMGUTF_MODEL_NUGET_FILE_LIST "${EMGUTF_MODEL_NUGET_FILE_LIST}
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Models.Mac.dll\" target=\"lib\\xamarinmac20\" />
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Models.Mac.xml\" target=\"lib\\xamarinmac20\" />")
  SET(EMGUTF_MODEL_NUGET_DEPENDENCY "${EMGUTF_MODEL_NUGET_DEPENDENCY}
    <group targetFramework=\"Xamarin.Mac2.0\">
		<dependency id=\"${EMGUTF_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
	</group>")
ENDIF()

IF(HAVE_ANDROID)
  SET(EMGUTF_MODEL_NUGET_FILE_LIST "${EMGUTF_MODEL_NUGET_FILE_LIST}
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Models.Android.dll\" target=\"lib\\MonoAndroid90\" />
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Models.Android.xml\" target=\"lib\\MonoAndroid90\" />")
  SET(EMGUTF_MODEL_NUGET_DEPENDENCY "${EMGUTF_MODEL_NUGET_DEPENDENCY}
    <group targetFramework=\"MonoAndroid1.0\">
		<dependency id=\"${EMGUTF_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
	</group>")
ENDIF()

SET(EMGUTF_MODEL_NUGET_DEPENDENCY "
    <dependencies>
		${EMGUTF_MODEL_NUGET_DEPENDENCY}
    </dependencies>")

SET(EMGUTF_PROTOBUF_NUGET_FILE_LIST "
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Protobuf.Netstandard.dll\" target=\"lib\\netstandard2.0\" />
    <file src=\"..\\..\\..\\lib\\Emgu.TF.Protobuf.Netstandard.xml\" target=\"lib\\netstandard2.0\" />
	<file src=\"..\\..\\..\\miscellaneous\\nuget_icon.png\" target=\"icon.png\" />
    <file src=\"..\\..\\..\\${EMGUTF_LICENSE_FILE}\" target=\"\" />")

SET(EMGUTF_PROTOBUF_PROJECT_FILE "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Protobuf/Emgu.TF.Protobuf.Netstandard/Emgu.TF.Protobuf.Netstandard.csproj")
file(STRINGS "${EMGUTF_PROTOBUF_PROJECT_FILE}" EMGUTF_PROTOBUF_PROJECT_PARTS REGEX "Google.Protobuf" )
string(REGEX REPLACE ".+Version=\"([0-9]+.[0-9]+.[0-9]+)\".*" "\\1" PROTOBUF_VERSION "${EMGUTF_PROTOBUF_PROJECT_PARTS}")
#MESSAGE(STATUS "EMGUTF_PROTOBUF_PROJECT_PARTS: ${EMGUTF_PROTOBUF_PROJECT_PARTS}" )	
#MESSAGE(STATUS "PROTOBUF_VERSION: ${PROTOBUF_VERSION}" )	
	
SET(EMGUTF_PROTOBUF_NUGET_DEPENDENCY "${EMGUTF_PROTOBUF_NUGET_DEPENDENCY}
    <dependencies>
		<group targetFramework=\".NETStandard2.0\">
			<dependency id=\"${EMGUTF_NUGET_ID}\" version=\"[${CPACK_PACKAGE_VERSION}${TENSORFLOW_VERSION_SUFFIX}]\" />
			<dependency id=\"Google.Protobuf\" version=\"[${PROTOBUF_VERSION}]\" />
		</group>
    </dependencies>") 

SET(EMGUTF_WINDOWS_NUGET_DEPENDENCY "
	<dependencies>
		${EMGUTF_WINDOWS_NUGET_DEPENDENCY}
	</dependencies>")

CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Models.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.Models/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Protobuf.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.Protobuf/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/XamarinForms.Android.AndroidManifest.xml.in ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Example/XamarinForms/XamarinForms.Android/Properties/AndroidManifest.xml @ONLY)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/XamarinForms.Lite.Android.AndroidManifest.xml.in ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.Example/XamarinForms/XamarinForms.Lite.Android/Properties/AndroidManifest.xml @ONLY)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.runtime.android.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.runtime.android/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.runtime.windows.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.runtime.windows/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.runtime.macos.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.runtime.macos/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Lite.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.Lite/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Lite.Models.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.Lite.Models/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Lite.runtime.windows.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.Lite.runtime.windows/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Lite.runtime.macos.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.Lite.runtime.macos/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Lite.runtime.android.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.Lite.runtime.android/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Lite.runtime.ios.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.Lite.runtime.ios/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Lite.runtime.ubuntu.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.Lite.runtime.ubuntu/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.runtime.ubuntu.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.runtime.ubuntu/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.Lite.runtime.raspbian.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.Lite.runtime.raspbian/Package.nuspec)
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Emgu.TF.runtime.raspbian.Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/platforms/nuget/Emgu.TF.runtime.raspbian/Package.nuspec)

# ----------------------------------------------------------------------------
#  Generate readme file for Emgu TF project
# ----------------------------------------------------------------------------
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/README.txt.in ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF/README.txt)
INSTALL(FILES "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF/README.txt" DESTINATION "." COMPONENT emgutf_binary)

IF(TARGET_ARCH_64)
  SET(UNMANAGED_CODE_BITNESS "64")
ELSE()
  SET(UNMANAGED_CODE_BITNESS "32")
ENDIF()


# ----------------------------------------------------------------------------
#  Build the managed dlls of Emgu TF
# ----------------------------------------------------------------------------

ADD_SUBDIRECTORY(Emgu.TF.Util)
IF(HAVE_FULL_BUILD)
  ADD_SUBDIRECTORY(Emgu.TF)
ENDIF()
IF(HAVE_LITE_BUILD)
  ADD_SUBDIRECTORY(Emgu.TF.Lite)
ENDIF()
ADD_SUBDIRECTORY(Emgu.TF.Example)
#ADD_SUBDIRECTORY(Emgu.TF.Test.Core)
#ADD_SUBDIRECTORY(Emgu.TF.Test)
ADD_SUBDIRECTORY(Emgu.TF.Runtime)
ADD_SUBDIRECTORY(Emgu.Models)
IF(HAVE_FULL_BUILD)
  ADD_SUBDIRECTORY(Emgu.TF.Protobuf)
  ADD_SUBDIRECTORY(Emgu.TF.Models)
  ADD_SUBDIRECTORY(Emgu.TF.CodeGen)
ENDIF()
IF(HAVE_LITE_BUILD)
  ADD_SUBDIRECTORY(Emgu.TF.Lite.Models)
ENDIF()
IF (HAVE_FULL_BUILD OR HAVE_LITE_BUILD)
  ADD_SUBDIRECTORY(platforms)
ENDIF()

INSTALL(
  FILES
  "tensorflow/LICENSE"
  DESTINATION "tensorflow"
  COMPONENT emgutf_source
  ) 

IF(HAVE_ANDROID)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/Android
    DESTINATION Solution
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Android.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
ENDIF()
IF(HAVE_ANDROID_LITE)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/Android
    DESTINATION Solution
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Lite.Android.sln"
	PATTERN "Emgu.TF.Lite.Example.Android.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
ENDIF()

#IF(HAVE_IOS)
#  INSTALL(
#    DIRECTORY
#    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/iOS
#    DESTINATION Solution
#    COMPONENT emgutf_source
#    FILES_MATCHING 
#    PATTERN "Emgu.TF.Example.iOS.sln"
#    PATTERN ".git" EXCLUDE
#    PATTERN "bin" EXCLUDE
#    )
#ENDIF()

IF(HAVE_IOS_LITE)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/iOS
    DESTINATION Solution
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Lite.Example.iOS.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
ENDIF()

IF(HAVE_OSX)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/macos
    DESTINATION Solution
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Mac.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
ENDIF()

IF(HAVE_OSX_LITE)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/macos
    DESTINATION Solution
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Lite.Mac.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
ENDIF()

#WIN32 and not NETFX_CORE solution files
IF (HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/Windows.Desktop
    DESTINATION Solution
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/Windows.Desktop
    DESTINATION Solution
    COMPONENT emgutf_example_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Example.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
ENDIF()

IF(HAVE_WINDESKTOP_LITE)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/Windows.Desktop
    DESTINATION Solution
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Lite.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/Windows.Desktop
    DESTINATION Solution
    COMPONENT emgutf_example_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Lite.Example.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
ENDIF()

INSTALL(
  FILES
  "${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.snk"
  "${CMAKE_CURRENT_SOURCE_DIR}/CommonAssemblyInfo.cs"
  "${CMAKE_CURRENT_SOURCE_DIR}/${LICENSE_FILE_NAME}"
  "${CMAKE_CURRENT_SOURCE_DIR}/nuget.config"
  "${CMAKE_CURRENT_SOURCE_DIR}/components.config"
  DESTINATION .
  COMPONENT emgutf_source
  )


SET(CPACK_GENERATOR ZIP)
IF(WIN32 AND (NOT NETFX_CORE))
  SET(CPACK_GENERATOR NSIS;ZIP)
ENDIF()
SET(CPACK_ARCHIVE_COMPONENT_INSTALL ON) #enable components install for zip
IF(CPACK_GENERATOR MATCHES "ZIP")
  SET(CPACK_COMPONENTS_ALL_IN_ONE_PACKAGE 1)
ENDIF()
  
# ----------------------------------------------------------------------------
#  The binary files
# ----------------------------------------------------------------------------
INSTALL(
  DIRECTORY
  ${CMAKE_CURRENT_SOURCE_DIR}/lib
  DESTINATION .
  COMPONENT emgutf_binary
  FILES_MATCHING 
  PATTERN "*.dll"
  PATTERN "*.dylib"
  PATTERN "*.so"
  PATTERN "*.txt"
  PATTERN "*.xml"
  PATTERN ".git" EXCLUDE
  PATTERN "obj" EXCLUDE
  PATTERN "CMake*" EXCLUDE
  PATTERN "Release" EXCLUDE
  PATTERN "Debug" EXCLUDE
  PATTERN "Build" EXCLUDE
  PATTERN "${PROJECT_NAME}.dir" EXCLUDE
  ) 

set(CPACK_COMPONENTS_ALL 
  emgutf_binary 
  emgutf_source
  emgutf_example_source
  )

IF (WIN32)
  SET(CPACK_NSIS_MODIFY_PATH OFF)
  SET(CPACK_NSIS_INSTALL_ROOT "C:\\\\Emgu")
  
  # ----------------------------------------------------------------------------
  #  Build the documents of Emgu TF
  # ----------------------------------------------------------------------------
  SET(EMGU_TF_DOCUMENTATION_BUILD OFF CACHE BOOL "Build Emgu TF Documentation")
  IF(EMGU_TF_DOCUMENTATION_BUILD)
    ADD_SUBDIRECTORY(miscellaneous)
  ENDIF()
  
  # ----------------------------------------------------------------------------
  #  Build the package
  # ----------------------------------------------------------------------------
    
  set(CPACK_PACKAGE_EXECUTABLES "" "") #http://public.kitware.com/Bug/view.php?id=7828
  
  SET(CPACK_NSIS_CONTACT "support@emgu.com")
  
  # Define MUI_TEMP that will be used for uninstalling menulinks
  SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\n !insertmacro MUI_STARTMENU_GETFOLDER Application $MUI_TEMP")
  
  # ----------------------------------------------------------------------------
  #  Add menu link for documentations
  # ----------------------------------------------------------------------------
  
  
  IF(EMGU_TF_DOCUMENTATION_BUILD)
    SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateDirectory \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Documentation\\\" ")
    
    LIST(APPEND CPACK_COMPONENTS_ALL emgutf_document)
    set(CPACK_COMPONENT_EMGUTF_DOCUMENT_DISPLAY_NAME "Emgu TF Documentation")
    set(CPACK_COMPONENT_EMGUTF_DOCUMENT_DEPENDS emgutf_binary)
    IF (HAVE_WINDESKTOP)
      SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Documentation\\\\Emgu TF Documentation.lnk\\\"  \\\"$INSTDIR\\\\Emgu.TF.Documentation.chm\\\" ")	
      SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\n Delete \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Documentation\\\\Emgu TF Documentation.lnk\\\" ")
    ENDIF()
    IF (HAVE_WINDESKTOP_LITE)
      SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Documentation\\\\Emgu TF Lite Documentation.lnk\\\"  \\\"$INSTDIR\\\\Emgu.TF.Lite.Documentation.chm\\\" ")	
      SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\n Delete \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Documentation\\\\Emgu TF Lite Documentation.lnk\\\" ")
    ENDIF()
    
    SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\nRMDir  \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Documentation\\\" ")
  ENDIF()
  
  # ----------------------------------------------------------------------------
  #  Add menu link for web sites
  # ----------------------------------------------------------------------------
  LIST(APPEND CPACK_NSIS_MENU_LINKS "${EMGUTF_MAIN_WEB_URL}" "Emgu TF wiki")
  #LIST(APPEND CPACK_NSIS_MENU_LINKS "http://www.emgu.com/forum" "Emgu TF Discussion Forums")
  LIST(APPEND CPACK_NSIS_MENU_LINKS "https://github.com/emgucv/emgutf/issues" "Emgu TF Bug Tracking")
  
  # ----------------------------------------------------------------------------
  #  Add menu link for Visual Studio solutions 
  # ----------------------------------------------------------------------------
  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateDirectory \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Visual Studio Solution\\\" ")
  IF (HAVE_WINDESKTOP)
    SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Visual Studio Solution\\\\Emgu TF Visual Studio Examples.lnk\\\"  \\\"$INSTDIR\\\\Solution\\\\Windows.Desktop\\\\Emgu.TF.Example.sln\\\" ")	
    SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\n Delete \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Visual Studio Solution\\\\Emgu TF Visual Studio Examples.lnk\\\" ")
  ENDIF()
  IF (HAVE_WINDESKTOP_LITE)
    SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Visual Studio Solution\\\\Emgu TF Lite Visual Studio Examples.lnk\\\"  \\\"$INSTDIR\\\\Solution\\\\Windows.Desktop\\\\Emgu.TF.Lite.Example.sln\\\" ")	
    SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\n Delete \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Visual Studio Solution\\\\Emgu TF Lite Visual Studio Examples.lnk\\\" ")
  ENDIF()
  
  SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\nRMDir  \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Visual Studio Solution\\\" ")
  
  # ----------------------------------------------------------------------------
  #  Add menu link for Licenses 
  # ----------------------------------------------------------------------------
  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateDirectory \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\License\\\" ")
  
  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\License\\\\Emgu TF License.lnk\\\"  \\\"$INSTDIR\\\\${LICENSE_FILE_NAME}\\\" ")	
  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\License\\\\Tensorflow License.lnk\\\"  \\\"$INSTDIR\\\\tensorflow\\\\LICENSE\\\" ")
  
  SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\n Delete \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\License\\\\Emgu TF License.lnk\\\" ")
  SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\n Delete \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\License\\\\Tensorflow License.lnk\\\" ")
  
  SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\nRMDir  \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\License\\\" ")
  
  #IF(EMGU_TF_EXAMPLE_BUILD AND NOT NETFX_CORE)
  #  LIST(APPEND CPACK_COMPONENTS_ALL emgutf_example_binary)
  #  SET(CPACK_COMPONENT_EMGUTF_EXAMPLE_BINARY_DISPLAY_NAME "Emgu TF Examples (Binary)")
  #  SET(CPACK_COMPONENT_EMGUTF_EXAMPLE_BINARY_DEPENDS emgutf_binary)
  #ENDIF()
  
  set(CPACK_COMPONENT_MAIN_DISPLAY_NAME "Tensorflow Native Binary")
  set(CPACK_COMPONENT_MAIN_REQUIRED ON)
  set(CPACK_COMPONENT_EMGUTF_BINARY_DISPLAY_NAME "Emgu TF (Binary)")
  set(CPACK_COMPONENT_EMGUTF_BINARY_REQUIRED ON)
  #set(CPACK_COMPONENT_EMGUTF_BINARY_DEPENDS libs)
  set(CPACK_COMPONENT_EMGUTF_SOURCE_DISPLAY_NAME "Emgu TF (Source)")
  set(CPACK_COMPONENT_EMGUTF_SOURCE_DEPENDS emgutf_binary)
  set(CPACK_COMPONENT_EMGUTF_EXAMPLE_SOURCE_DISPLAY_NAME "Emgu TF Examples (Source)")
  set(CPACK_COMPONENT_EMGUTF_EXAMPLE_SOURCE_DEPENDS emgutf_source)
  
ELSEIF (APPLE)
ELSE() #LINUX
ENDIF()  

MESSAGE(STATUS "CPACK_GENERATOR: ${CPACK_GENERATOR}")
INCLUDE (CPack) 

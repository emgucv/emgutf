# --------------------------------------------------------
# Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.
# 
# Root CMake file for Emgu TF
#
# From the off-tree build directory, invoke:
#   $ cmake <PATH_TO_EMGUCV_ROOT>
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

SET(CSC_PREFERRED_VERSION "4.0")
IF((NOT NETFX_CORE) AND MSVC) 
  #SET CSC 3.5 as the prefered compiler for EMGU CV
  SET(CSC_TARGET_FRAMEWORK "3.5" CACHE STRING "C#  target framework")
ELSE()
  #SET CSC 4.5 (in the .Net framework 4.0 folder) as the prefered compiler for EMGU CV
  SET(CSC_TARGET_FRAMEWORK "" CACHE STRING "C# target framework")
ENDIF()

FIND_PACKAGE(CSharp REQUIRED)
INCLUDE (CSharpMacros)
INCLUDE (TensorflowVersion)
MESSAGE(STATUS "TENSORFLOW_VERSION: ${TENSORFLOW_VERSION} ")

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
    #ELSE()
    #  SET(EMGUCV_ARCH win32)
  ENDIF()
ELSE(WIN32)
  SET(EMGUTF_ARCH ${CMAKE_SYSTEM_NAME})
ENDIF(WIN32)

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

MACRO(CHECK_BINARY_EXIST folder pattern property_name second_check)
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
    SET(${property_name} ON)
  ELSE()
    SET(${property_name} ${second_check})
  ENDIF()
  #ENDIF()
  #MESSAGE(STATUS "Property - ${property_name}: ${${property_name}}")
  IF(${${property_name}})
    MESSAGE(STATUS "${property_name}: ${${property_name}}")
  ENDIF()
ENDMACRO()


CHECK_BINARY_EXIST("lib/ios" "libtfextern.a" HAVE_IOS OFF)
CHECK_BINARY_EXIST("lib/osx" "libtfextern.dylib" HAVE_OSX OFF)
CHECK_BINARY_EXIST("lib/android/arm64-v8a" "*.so" HAVE_ANDROID_ARM64_V8A OFF)
CHECK_BINARY_EXIST("lib/android/armeabi" "*.so" HAVE_ANDROID_ARMEABI OFF)
CHECK_BINARY_EXIST("lib/android/armeabi-v7a" "*.so" HAVE_ANDROID_ARMEABI_V7A OFF)
CHECK_BINARY_EXIST("lib/android/x86" "*.so" HAVE_ANDROID_X86 OFF)
CHECK_BINARY_EXIST("lib/android/x86_64" "*.so" HAVE_ANDROID_X86_64 OFF)
SET(HAVE_ANDROID OFF)
IF(HAVE_ANDROID_ARM64_V8A OR HAVE_ANDROID_ARMEABI OR HAVE_ANDROID_ARMEABI_V7A OR HAVE_ANDROID_X86 OR HAVE_ANDROID_X86_64)
  SET(HAVE_ANDROID ON) 
ENDIF()
IF(HAVE_ANDROID)
  MESSAGE(STATUS "HAVE_ANDROID: ${HAVE_ANDROID}")
ENDIF()

CHECK_BINARY_EXIST("lib/x86" "*.dll" HAVE_WINDESKTOP_X86 OFF)
CHECK_BINARY_EXIST("lib/x64" "*.dll" HAVE_WINDESKTOP_X64 OFF)
IF(HAVE_WINDESKTOP_X86 AND HAVE_WINDESKTOP_X64)
  SET(WINDESKTOP_LABEL "-windesktop") 
ELSEIF(HAVE_WINDESKTOP_X86)
  SET(WINDESKTOP_LABEL "-windesktop_x86")
ELSEIF(HAVE_WINDESKTOP_X64)	
  SET(WINDESKTOP_LABEL "-windesktop_x64")
ELSE() 
  SET(WINDESKTOP_LABEL "")
ENDIF()
SET(WINDOWS_LABEL "${WINDESKTOP_LABEL}")
IF ("${WINDOWS_LABEL}" STREQUAL "-UWP-windesktop")
  SET(WINDOWS_LABEL "-windows")
ENDIF()

SET(IOS_LABEL "")
IF(HAVE_IOS)
  SET(IOS_LABEL "-ios")
ENDIF()

SET(OSX_LABEL "")
IF(HAVE_OSX)
  SET(OSX_LABEL "-osx")
ENDIF()	


SET(ANDROID_LABEL "")
IF(HAVE_ANDROID_ARM64_V8A)
  SET(ANDROID_LABEL "${ANDROID_LABEL}-arm64-v8a")
ENDIF()
IF(HAVE_ANDROID_ARMEABI)
  SET(ANDROID_LABEL "${ANDROID_LABEL}-armeabi")
ENDIF()
IF(HAVE_ANDROID_ARMEABI_V7A)	
  SET(ANDROID_LABEL "${ANDROID_LABEL}-armeabi-v7a")
ENDIF()
IF(HAVE_ANDROID_X86)
  SET(ANDROID_LABEL "${ANDROID_LABEL}-x86")
ENDIF()
IF(HAVE_ANDROID_X86_64)
  SET(ANDROID_LABEL "${ANDROID_LABEL}-x86_64")
ENDIF()

CHECK_BINARY_EXIST("bin" "Emgu.TF.World*" HAVE_PREBUILD_EMGU_TF_WORLD OFF)

IF("${ANDROID_LABEL}" STREQUAL "-arm64-v8a-armeabi-armeabi-v7a-x86-x86_64")
  SET(ANDROID_LABEL "-android")
ELSEIF(NOT ("${ANDROID_LABEL}" STREQUAL ""))
  SET(ANDROID_LABEL "-android${ANDROID_LABEL}")
ENDIF()

# ----------------------------------------------------------------------------
#  Setup Package information
# ----------------------------------------------------------------------------
SET(PACKAGE "${PROJECT_NAME}")
SET(CPACK_PACKAGE_CONTACT "Emgu TF SUPPORT <support@emgu.com>")
SET(PACKAGE_BUGREPORT "${CPACK_PACKAGE_CONTACT}")
SET(PACKAGE_NAME "${PROJECT_NAME}")

SET(CPACK_PACKAGE_DESCRIPTION_SUMMARY "Emgu TF is a cross platform .Net wrapper to the Google Tensorflow library.")

SET(CPACK_PACKAGE_NAME "${PACKAGE_NAME}${IOS_LABEL}${ANDROID_LABEL}${WINDOWS_LABEL}${OSX_LABEL}")
IF("${CPACK_PACKAGE_NAME}" STREQUAL "${PACKAGE_NAME}-ios-android-windows-osx")
  SET(CPACK_PACKAGE_NAME "${PACKAGE_NAME}-pro")
ENDIF()
#MESSAGE(STATUS "TENSORFLOW_VERSION_MAJOR: ${TENSORFLOW_VERSION_MAJOR}")
SET(CPACK_PACKAGE_VENDOR "Emgu Corporation")
SET(CPACK_PACKAGE_VERSION_MAJOR ${TENSORFLOW_VERSION_MAJOR})
SET(CPACK_PACKAGE_VERSION_MINOR ${TENSORFLOW_VERSION_MINOR})
SET(CPACK_PACKAGE_VERSION_PATCH ${TENSORFLOW_VERSION_PATCH})
SET(CPACK_PACKAGE_VERSION "${CPACK_PACKAGE_VERSION_MAJOR}.${CPACK_PACKAGE_VERSION_MINOR}.${CPACK_PACKAGE_VERSION_PATCH}.${EMGUTF_GITVERSION}")
#MESSAGE(STATUS "CPACK_PACKAGE_VERSION: ${CPACK_PACKAGE_VERSION}")

SET(CPACK_PACKAGE_FILE_NAME "lib${CPACK_PACKAGE_NAME}-${CPACK_PACKAGE_VERSION}")

MESSAGE(STATUS "CPACK_PACKAGE_FILE_NAME is ${CPACK_PACKAGE_FILE_NAME}")
FILE(WRITE "emgutf.version.txt" "${CPACK_PACKAGE_VERSION}")

SET(LICENSE_FILE_NAME "Emgu.TF.License.txt")

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
CONFIGURE_FILE(${CMAKE_CURRENT_SOURCE_DIR}/cmake/Package.nuspec.in ${CMAKE_CURRENT_SOURCE_DIR}/Emgu.TF.World/Package.nuspec.in)

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
ADD_SUBDIRECTORY(Emgu.TF)
ADD_SUBDIRECTORY(Emgu.TF.Example)
#ADD_SUBDIRECTORY(Emgu.TF.Test.Core)
#ADD_SUBDIRECTORY(Emgu.TF.Test)
ADD_SUBDIRECTORY(Emgu.TF.World)
ADD_SUBDIRECTORY(Emgu.TF.Models)

IF(HAVE_ANDROID)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/Android
    DESTINATION Solution
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "*.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
ENDIF()

IF(HAVE_IOS)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/iOS
    DESTINATION Solution
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "*.sln"
    PATTERN ".git" EXCLUDE
    PATTERN "bin" EXCLUDE
    )
ENDIF()

IF(HAVE_OSX)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Solution/OSX
    DESTINATION Solution
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "*.sln"
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
  PATTERN "*.txt"
  PATTERN "*.xml"
  PATTERN ".git" EXCLUDE
  PATTERN "obj" EXCLUDE
  PATTERN "CMake*" EXCLUDE
  PATTERN "Release" EXCLUDE
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
  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateDirectory \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Documentation\\\" ")
  
  #SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Documentation\\\\Open CV Documentation.lnk\\\"  \\\"$INSTDIR\\\\doc\\\\opencv2refman.pdf\\\" ")	
  #SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\n Delete \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Documentation\\\\Open CV Documentation.lnk\\\" ")
  
  #IF(EMGU_TF_DOCUMENTATION_BUILD)
  #  LIST(APPEND CPACK_COMPONENTS_ALL emgutf_document)
  #  set(CPACK_COMPONENT_EMGUTF_DOCUMENT_DISPLAY_NAME "Emgu TF Documentation")
  #  set(CPACK_COMPONENT_EMGUTF_DOCUMENT_DEPENDS emgutf_binary)
  #  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Documentation\\\\Emgu TF Documentation.lnk\\\"  \\\"$INSTDIR\\\\Emgu.TF.Documentation.chm\\\" ")	
  #  SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\n Delete \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Documentation\\\\Emgu TF Documentation.lnk\\\" ")
  #ENDIF()
  
  SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\nRMDir  \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Documentation\\\" ")
  
  # ----------------------------------------------------------------------------
  #  Add menu link for web sites
  # ----------------------------------------------------------------------------
  LIST(APPEND CPACK_NSIS_MENU_LINKS "http://www.emgu.com" "Emgu TF wiki")
  LIST(APPEND CPACK_NSIS_MENU_LINKS "http://www.emgu.com/forum" "Emgu TF Discussion Forums")
  LIST(APPEND CPACK_NSIS_MENU_LINKS "http://www.emgu.com/bugs/buglist.cgi?query_format=advanced&short_desc_type=allwordssubstr&short_desc=&product=Emgu+CV&long_desc_type=substring&long_desc=&bug_file_loc_type=allwordssubstr&bug_file_loc=&deadlinefrom=&deadlineto=&bug_status=UNCONFIRMED&bug_status=NEW&bug_status=ASSIGNED&bug_status=REOPENED&bug_status=RESOLVED&bug_status=VERIFIED&bug_status=CLOSED&emailassigned_to1=1&emailtype1=substring&email1=&emailassigned_to2=1&emailreporter2=1&emailcc2=1&emailtype2=substring&email2=&bugidtype=include&bug_id=&chfieldfrom=&chfieldto=Now&chfieldvalue=&cmdtype=doit&order=Reuse+same+sort+as+last+time&field0-0-0=noop&type0-0-0=noop&value0-0-0=" "Emgu CV Bug Tracking")
  
  # ----------------------------------------------------------------------------
  #  Add menu link for Visual Studio solutions 
  # ----------------------------------------------------------------------------
  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateDirectory \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Visual Studio Solution\\\" ")
  
  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Visual Studio Solution\\\\Visual Studio 2013 - 2017 Examples.lnk\\\"  \\\"$INSTDIR\\\\Solution\\\\Windows.Desktop\\\\Emgu.CV.Example.sln\\\" ")	
  
  SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\n Delete \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Visual Studio Solution\\\\Visual Studio 2013 - 2017 Examples.lnk\\\" ")
  
  SET(CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS "${CPACK_NSIS_EXTRA_UNINSTALL_COMMANDS}\nRMDir  \\\"$SMPROGRAMS\\\\$MUI_TEMP\\\\Visual Studio Solution\\\" ")
  
  # ----------------------------------------------------------------------------
  #  Add menu link for Licenses 
  # ----------------------------------------------------------------------------
  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateDirectory \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\License\\\" ")
  
  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\License\\\\Emgu TF License.lnk\\\"  \\\"$INSTDIR\\\\${LICENSE_FILE_NAME}\\\" ")	
  SET(CPACK_NSIS_EXTRA_INSTALL_COMMANDS "${CPACK_NSIS_EXTRA_INSTALL_COMMANDS}\nCreateShortCut \\\"$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\License\\\\Tensorflow License.lnk\\\"  \\\"$INSTDIR\\\\lib\\\\tensorflow.license.txt\\\" ")
  
  
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
  #set(CPACK_COMPONENT_EMGUTF_SOURCE_DEPENDS libs)
  set(CPACK_COMPONENT_EMGUTF_EXAMPLE_SOURCE_DISPLAY_NAME "Emgu TF Examples (Source)")
  set(CPACK_COMPONENT_EMGUTF_EXAMPLE_SOURCE_DEPENDS emgutf_source)
  
ELSEIF (APPLE)
ELSE() #LINUX
ENDIF()  

MESSAGE(STATUS "CPACK_GENERATOR: ${CPACK_GENERATOR}")
INCLUDE (CPack) 

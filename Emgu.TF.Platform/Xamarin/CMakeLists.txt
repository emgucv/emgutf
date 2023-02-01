# --------------------------------------------------------
#  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_ANDROID_LITE)
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Android
    DESTINATION Emgu.TF.Platform/Xamarin
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Lite.Platform.Xamarin.Android.csproj"
    PATTERN "TfLite*.cs"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  IF (WIN32 OR APPLE)
    PROJECT(Emgu.TF.Lite.Platform.Xamarin.Android)
    MSBUILD_CSPROJ_IN_SOLUTION(
      ${PROJECT_NAME} 
      "${CMAKE_CURRENT_SOURCE_DIR}/../../Solution/Android/Emgu.TF.Lite.Android.sln" 
      ${PROJECT_NAME}
      "${EXTRA_BUILD_FLAGS}" 
      ALL)
    IF (EMGU_SIGN_FOUND AND WIN32)
      EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll )
    ENDIF()
    INSTALL(
      FILES 
      ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
      ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
      DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
      COMPONENT emgutf_binary)
  ENDIF()
ENDIF()


IF(HAVE_ANDROID)
  INSTALL(
    DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}/Android
    DESTINATION Emgu.TF.Platform/Xamarin
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Platform.Xamarin.Android.csproj"
    PATTERN "TfInvoke*.cs"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  IF(APPLE OR WIN32)
    PROJECT(Emgu.TF.Platform.Xamarin.Android)
    MSBUILD_CSPROJ_IN_SOLUTION(
      ${PROJECT_NAME} 
      "${CMAKE_CURRENT_SOURCE_DIR}/../../Solution/Android/Emgu.TF.Android.sln" 
      ${PROJECT_NAME}
      "${EXTRA_BUILD_FLAGS}" 
      ALL)
    IF (EMGU_SIGN_FOUND AND WIN32)
      EMGU_SIGN_BINARY(${PROJECT_NAME} ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll )
    ENDIF()

    INSTALL(
      FILES 
      ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
      ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
      DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
      COMPONENT emgutf_binary)
  ENDIF()
ENDIF()

IF(HAVE_IOS_LITE)
  #APPLE IOS
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/iOS
    DESTINATION Emgu.TF.Platform/Xamarin
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Lite.Platform.Xamarin.iOS.csproj"
    PATTERN "*.cs"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    )
  IF(APPLE)
    PROJECT(Emgu.TF.Lite.Platform.Xamarin.iOS)
    MSBUILD_CSPROJ_IN_SOLUTION(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}/../../Solution/iOS/Emgu.TF.Lite.Example.iOS.sln" "${PROJECT_NAME}" "" ALL)
    INSTALL(
      FILES 
      ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.xml
      ${LIBRARY_OUTPUT_PATH}/${PROJECT_NAME}.dll 
      DESTINATION ${CPACK_PACKAGE_CLI_FOLDER}
      COMPONENT emgutf_binary)
  ENDIF()	
ENDIF()

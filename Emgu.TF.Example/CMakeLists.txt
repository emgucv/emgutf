# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------


IF (HAVE_ANDROID OR HAVE_IOS OR HAVE_OSX OR HAVE_WINDESKTOP_X64 OR HAVE_ANDROID_LITE OR HAVE_IOS_LITE OR HAVE_OSX_LITE OR HAVE_WINDESKTOP_LITE) 	
  INSTALL(FILES
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/examples/multibox_detector/data/surfers.jpg"
    DESTINATION tensorflow/tensorflow/examples/multibox_detector/data 
	COMPONENT emgutf_example_source)
	
  INSTALL(FILES
    "${CMAKE_CURRENT_SOURCE_DIR}/CVInterop/dog416.png"
    DESTINATION Emgu.TF.Example/CVInterop
	COMPONENT emgutf_example_source)
ENDIF()


IF(HAVE_ANDROID_LITE OR HAVE_IOS_LITE OR HAVE_WINDESKTOP_LITE)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/Maui.Lite"
    DESTINATION Emgu.TF.Example/Maui.Lite
    COMPONENT emgutf_example_source
    FILES_MATCHING 
    PATTERN "*.cs"
    PATTERN "*.csproj"
    PATTERN "*.config"
    PATTERN "*.shproj"
    PATTERN "*.projitems"
    PATTERN "*.axml"
    PATTERN "*.xaml"
    PATTERN "*.xml"
    PATTERN "*.txt"
    PATTERN "*.png"
    PATTERN "*.dll"
    PATTERN "*.json"
    PATTERN ".git" EXCLUDE
    PATTERN "Debug" EXCLUDE
    PATTERN "Release" EXCLUDE  
    )
ENDIF()
  
IF(HAVE_IOS OR HAVE_WINDESKTOP_X64)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/Maui"
    DESTINATION Emgu.TF.Example/Maui/
    COMPONENT emgutf_example_source
    FILES_MATCHING 
    PATTERN "*.cs"
    PATTERN "*.csproj"
    PATTERN "*.config"
    PATTERN "*.shproj"
    PATTERN "*.projitems"
    PATTERN "*.axml"
    PATTERN "*.xaml"
    PATTERN "*.xml"
    PATTERN "*.txt"
    PATTERN "*.png"
    PATTERN "*.dll"
    PATTERN "*.json"
    PATTERN "*.plist"
    PATTERN "iTunesArtwork*"
    PATTERN "*.storyboard"
    PATTERN ".git" EXCLUDE
    PATTERN "Debug" EXCLUDE
    PATTERN "Release" EXCLUDE  
    )  
ENDIF()

add_subdirectory(Inception.Console.Lite)
add_subdirectory(Inception.Console)

add_subdirectory(CVInterop)
add_subdirectory(CVInterop.Lite)


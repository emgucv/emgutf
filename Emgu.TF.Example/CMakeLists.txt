# --------------------------------------------------------
#  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------


#XamarinForms
IF (HAVE_ANDROID OR HAVE_IOS OR HAVE_OSX) 
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms/
    COMPONENT emgutf_example_source
    FILES_MATCHING 
    PATTERN "*.cs"
    PATTERN "*.csproj"
    PATTERN "*.config"
    PATTERN "*.shproj"
    PATTERN "*.Xamarin"
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
	
  INSTALL(FILES
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/examples/multibox_detector/data/surfers.jpg"
    DESTINATION tensorflow/tensorflow/examples/multibox_detector/data 
	COMPONENT emgutf_example_source)
	
  INSTALL(FILES
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/examples/label_image/data/grace_hopper.jpg"
    DESTINATION tensorflow/tensorflow/examples/label_image/data 
	COMPONENT emgutf_example_source)
ENDIF()

IF(HAVE_ANDROID)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms.Android/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms.Android/
    COMPONENT emgutf_example_source
    FILES_MATCHING 
    PATTERN "*.cs"
    PATTERN "*.csproj"
    PATTERN "*.config"
    PATTERN "*.shproj"
    PATTERN "*.Xamarin"
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
  
IF(HAVE_IOS)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms.iOS/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms.iOS/
    COMPONENT emgutf_example_source
    FILES_MATCHING 
    PATTERN "*.cs"
    PATTERN "*.csproj"
    PATTERN "*.config"
    PATTERN "*.shproj"
    PATTERN "*.Xamarin"
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

IF(HAVE_OSX)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms.OSX/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms.OSX/
    COMPONENT emgutf_example_source
    FILES_MATCHING
    PATTERN "*.cs"
    PATTERN "*.csproj"
    PATTERN "*.config"
    PATTERN "*.Xamarin"
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

IF(HAVE_WINSTORE_10_X86 OR HAVE_WINSTORE_10_X64 OR HAVE_WINSTORE_10_ARM)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms.UWP/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms.UWP/
    COMPONENT emgutf_example_source
    FILES_MATCHING 
    PATTERN "*.cs"
    PATTERN "Properties/*"
    PATTERN "*.csproj"
    PATTERN "*.appxmanifest"
    PATTERN "*.xaml"
    PATTERN "*.json"
    PATTERN "*.pfx"
    PATTERN "*.config"
    PATTERN "Assets/*.png"  
    PATTERN ".git" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMake*" EXCLUDE
    PATTERN "Release" EXCLUDE
    PATTERN "Debug" EXCLUDE
    PATTERN "RelWithDebInfo" EXCLUDE
    PATTERN "*.dir" EXCLUDE
    PATTERN "bin" EXCLUDE
    )  
ENDIF()


IF(NOT (NETFX_CORE OR ANDROID OR (APPLE AND (NOT IOS))))
  SET(EMGU_TF_REFERENCE 
    -r:System.Xml.dll 
    -r:System.Drawing.dll 
    -r:System.Data.dll 
    -r:System.Windows.Forms.dll
    -r:System.ServiceModel.dll
    -r:"${LIBRARY_OUTPUT_PATH}/Emgu.TF.World.dll" )
ENDIF()

add_subdirectory(MultiboxPeopleDetection)


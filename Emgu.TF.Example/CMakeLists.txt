# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------


#XamarinForms

IF (HAVE_ANDROID OR HAVE_IOS OR HAVE_OSX OR HAVE_WINDESKTOP_X64) 
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
    PATTERN "*.dll"
    PATTERN "*.json"
    PATTERN ".git" EXCLUDE
    PATTERN "Debug" EXCLUDE
    PATTERN "Release" EXCLUDE  
    )
ENDIF()

IF (HAVE_ANDROID OR HAVE_IOS OR HAVE_OSX OR HAVE_WINDESKTOP_X64 OR HAVE_ANDROID_LITE OR HAVE_IOS_LITE OR HAVE_OSX_LITE OR HAVE_WINDESKTOP_LITE) 	
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms/
    COMPONENT emgutf_example_source
    FILES_MATCHING 
    PATTERN "*.png"
	PATTERN "*.jpg"
    PATTERN ".git" EXCLUDE
    PATTERN "Debug" EXCLUDE
    PATTERN "Release" EXCLUDE  
    )
  INSTALL(FILES
    "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/examples/multibox_detector/data/surfers.jpg"
    DESTINATION tensorflow/tensorflow/examples/multibox_detector/data 
	COMPONENT emgutf_example_source)
	
  #INSTALL(FILES
  #  "${CMAKE_CURRENT_SOURCE_DIR}/InceptionObjectRecognition/space_shuttle.jpg"
  #  DESTINATION Emgu.TF.Example/InceptionObjectRecognition
  #  COMPONENT emgutf_example_source)
	
  #INSTALL(FILES
  #  "${CMAKE_CURRENT_SOURCE_DIR}/../tensorflow/tensorflow/examples/label_image/data/grace_hopper.jpg"
  #  DESTINATION tensorflow/tensorflow/examples/label_image/data 
  #	COMPONENT emgutf_example_source)
ENDIF()

IF (HAVE_ANDROID_LITE OR HAVE_IOS_LITE OR HAVE_OSX_LITE OR HAVE_WINDESKTOP_LITE OR HAVE_WINDESKTOP_X64) 
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms.Lite/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms.Lite/
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

IF(HAVE_ANDROID_LITE)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms.Lite.Android/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms.Lite.Android/
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

IF(HAVE_IOS_LITE)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms.Lite.iOS/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms.Lite.iOS/
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
	
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/Mac/"
    DESTINATION Emgu.TF.Example/Mac/
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

IF(HAVE_OSX_LITE)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms.Lite.OSX/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms.Lite.OSX/
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

IF(HAVE_WINDESKTOP_LITE)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms.Lite.WPF/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms.Lite.WPF/
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
	PATTERN "*.resx"
	PATTERN "*.settings"
    PATTERN ".git" EXCLUDE
    PATTERN "Debug" EXCLUDE
    PATTERN "Release" EXCLUDE  
    )
ENDIF()

IF(HAVE_WINDESKTOP_X64)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/XamarinForms/XamarinForms.WPF/"
    DESTINATION Emgu.TF.Example/XamarinForms/XamarinForms.WPF/
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

IF(HAS_LITE_BUILD)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/Inception.Console.Lite.NetCore/"
    DESTINATION Emgu.TF.Example/Inception.Console.Lite.NetCore/
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
  add_subdirectory(Inception.Console.Lite.NetCore)
ENDIF()

IF(HAS_FULL_BUILD)
  INSTALL(
    DIRECTORY
    "${CMAKE_CURRENT_SOURCE_DIR}/Inception.Console.NetCore/"
    DESTINATION Emgu.TF.Example/Inception.Console.NetCore/
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
  add_subdirectory(Inception.Console.NetCore)
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

#add_subdirectory(MultiboxPeopleDetection)
#add_subdirectory(InceptionObjectRecognition)
add_subdirectory(CVInterop)


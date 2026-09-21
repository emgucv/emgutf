# --------------------------------------------------------
#  Copyright (C) 2004-2026 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(HAVE_FULL_BUILD)
  #MacOS
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Mac
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Runtime.Mac.shproj"
    PATTERN "Emgu.TF.Runtime.Mac.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )

  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Windows
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Runtime.Windows.shproj"
    PATTERN "Emgu.TF.Runtime.Windows.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
  
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Ubuntu
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Runtime.Ubuntu.shproj"
    PATTERN "Emgu.TF.Runtime.Ubuntu.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
	
  INSTALL(
    DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}/Debian
    DESTINATION Emgu.TF.Runtime
    COMPONENT emgutf_source
    FILES_MATCHING 
    PATTERN "Emgu.TF.Runtime.Debian.shproj"
    PATTERN "Emgu.TF.Runtime.Debian.projitems"
    PATTERN ".svn" EXCLUDE
    PATTERN "obj" EXCLUDE
    PATTERN "CMakeFiles" EXCLUDE
    PATTERN "${PROJECT_NAME}.dir" EXCLUDE
    )
ENDIF()

ADD_SUBDIRECTORY(Maui)
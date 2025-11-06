# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------
IF(HAVE_FULL_BUILD)

INSTALL(
  DIRECTORY
  ${CMAKE_CURRENT_SOURCE_DIR}
  DESTINATION .
  COMPONENT emgutf_source
  FILES_MATCHING 
  PATTERN "*.cs"
  PATTERN "*.shproj"
  PATTERN "*.csproj"
  PATTERN "*.projitems"
  PATTERN "*.resx"
  PATTERN "README.md"
  PATTERN "Directory.Build.props"
  PATTERN ".svn" EXCLUDE
  PATTERN "obj" EXCLUDE
  PATTERN "CMakeFiles" EXCLUDE
  PATTERN "${PROJECT_NAME}.dir" EXCLUDE
  PATTERN "*Unity*" EXCLUDE
  )

ADD_SUBDIRECTORY(NetStandard)

ENDIF()

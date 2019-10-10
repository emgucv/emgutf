# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (HAVE_FULL_BUILD)
  add_subdirectory(Emgu.TF)
  add_subdirectory(Emgu.TF.Models)
  add_subdirectory(Emgu.TF.Protobuf)
ENDIF()

IF (WIN32 AND HAVE_WINDESKTOP )
  add_subdirectory(runtime.windows.cuda.dnn.Emgu.TF)
  add_subdirectory(runtime.windows.cuda.cufft.Emgu.TF)
  add_subdirectory(runtime.windows.Emgu.TF)
ENDIF()

IF ( HAVE_ANDROID )
  add_subdirectory(runtime.android.Emgu.TF)
ENDIF()

#MESSAGE(STATUS "-------HAVE_OSX_LITE: ${HAVE_OSX_LITE}")
#MESSAGE(STATUS "-------HAVE_LITE_BUILD: ${HAVE_LITE_BUILD}")

IF (HAVE_LITE_BUILD)
  add_subdirectory(Emgu.TF.Lite)
  add_subdirectory(Emgu.TF.Lite.Models)
ENDIF()

IF (HAVE_WINDESKTOP_X64_LITE)
  add_subdirectory(runtime.windows.Emgu.TF.Lite)
ENDIF()

IF(HAVE_OSX_LITE)
  add_subdirectory(runtime.macos.Emgu.TF.Lite)
ENDIF()

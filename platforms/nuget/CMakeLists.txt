# --------------------------------------------------------
#  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

add_subdirectory(Emgu.TF)
add_subdirectory(Emgu.TF.Models)
add_subdirectory(Emgu.TF.Protobuf)

IF (WIN32 AND HAVE_WINDESKTOP)
  add_subdirectory(Emgu.TF.runtime.windows.cuda.rt)
  add_subdirectory(Emgu.TF.runtime.windows.cuda.dnn)
  add_subdirectory(Emgu.TF.runtime.windows.cuda.cufft)
  add_subdirectory(Emgu.TF.runtime.windows)
ENDIF()

IF (HAVE_ANDROID)
  add_subdirectory(Emgu.TF.runtime.android)
ENDIF()

IF(HAVE_OSX)
  add_subdirectory(Emgu.TF.runtime.macos)
ENDIF()

IF(HAVE_LINUX_X86_64)
  add_subdirectory(Emgu.TF.runtime.ubuntu)
ENDIF()
  

#MESSAGE(STATUS "-------HAVE_OSX_LITE: ${HAVE_OSX_LITE}")
#MESSAGE(STATUS "-------HAVE_LITE_BUILD: ${HAVE_LITE_BUILD}")

add_subdirectory(Emgu.TF.Lite)
add_subdirectory(Emgu.TF.Lite.Models)

IF (HAVE_WINDESKTOP_X64_LITE)
  add_subdirectory(Emgu.TF.Lite.runtime.windows)
ENDIF()

IF(HAVE_OSX_LITE)
  add_subdirectory(Emgu.TF.Lite.runtime.macos)
ENDIF()

IF ( HAVE_ANDROID_LITE )
  add_subdirectory(Emgu.TF.Lite.runtime.android)
ENDIF()

IF ( HAVE_IOS_LITE )
  add_subdirectory(Emgu.TF.Lite.runtime.ios)
ENDIF()

IF (HAVE_LINUX_X86_64_LITE)
  add_subdirectory(Emgu.TF.Lite.runtime.ubuntu)
ENDIF()

IF (HAVE_LINUX_ARM_LITE)
  add_subdirectory(Emgu.TF.Lite.runtime.raspbian)
ENDIF()

# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

add_subdirectory(Emgu.TF)
add_subdirectory(Emgu.TF.Models)
add_subdirectory(Emgu.TF.Protobuf)

IF (WIN32 AND HAVE_WINDESKTOP)
  add_subdirectory(Emgu.TF.runtime.windows.cuda.dnn)
  add_subdirectory(Emgu.TF.runtime.windows.cuda.cufft)
  add_subdirectory(runtime.windows.Emgu.TF)
ENDIF()

IF (HAVE_ANDROID)
  add_subdirectory(runtime.android.Emgu.TF)
ENDIF()

IF(HAVE_OSX)
  add_subdirectory(runtime.macos.Emgu.TF)
ENDIF()

IF(HAVE_LINUX_X86_64)
  add_subdirectory(runtime.ubuntu.Emgu.TF)
ENDIF()
  

#MESSAGE(STATUS "-------HAVE_OSX_LITE: ${HAVE_OSX_LITE}")
#MESSAGE(STATUS "-------HAVE_LITE_BUILD: ${HAVE_LITE_BUILD}")

add_subdirectory(Emgu.TF.Lite)
add_subdirectory(Emgu.TF.Lite.Models)

IF (HAVE_WINDESKTOP_X64_LITE)
  add_subdirectory(runtime.windows.Emgu.TF.Lite)
ENDIF()

IF(HAVE_OSX_LITE)
  add_subdirectory(runtime.macos.Emgu.TF.Lite)
ENDIF()

IF ( HAVE_ANDROID_LITE )
  add_subdirectory(runtime.android.Emgu.TF.Lite)
ENDIF()

IF ( HAVE_IOS_LITE )
  add_subdirectory(runtime.ios.Emgu.TF.Lite)
ENDIF()

IF (HAVE_LINUX_X86_64_LITE)
  add_subdirectory(runtime.ubuntu.Emgu.TF.Lite)
ENDIF()

IF (HAVE_LINUX_ARM_LITE)
  add_subdirectory(runtime.raspbian.Emgu.TF.Lite)
ENDIF()

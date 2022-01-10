# --------------------------------------------------------
#  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

add_subdirectory(Emgu.TF)
add_subdirectory(Emgu.TF.Models)
add_subdirectory(Emgu.TF.Protobuf)

IF (WIN32 AND HAVE_WINDESKTOP)
  add_subdirectory(Emgu.runtime.windows.cuda.rt)
  add_subdirectory(Emgu.runtime.windows.cuda.dnn)
  add_subdirectory(Emgu.runtime.windows.cuda.dnn.adv)
  add_subdirectory(Emgu.runtime.windows.cuda.dnn.cnn.train)
  add_subdirectory(Emgu.runtime.windows.cuda.dnn.cnn.infer)
  add_subdirectory(Emgu.runtime.windows.cuda.dnn.ops)
  add_subdirectory(Emgu.runtime.windows.cuda.fft)
  add_subdirectory(Emgu.runtime.windows.cuda.sparse)
  add_subdirectory(Emgu.runtime.windows.cuda.blas)
  add_subdirectory(Emgu.runtime.windows.cuda.solver)
  add_subdirectory(Emgu.runtime.windows.cuda.solver.mg)
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
  
IF(HAVE_LINUX_ARM)
  add_subdirectory(Emgu.TF.runtime.linux-arm)
ENDIF()

#MESSAGE(STATUS "-------HAVE_OSX_LITE: ${HAVE_OSX_LITE}")
#MESSAGE(STATUS "-------HAVE_LITE_BUILD: ${HAVE_LITE_BUILD}")

add_subdirectory(Emgu.TF.Lite)
add_subdirectory(Emgu.TF.Lite.Models)

IF (HAVE_WINDESKTOP_LITE)
  add_subdirectory(Emgu.TF.Lite.runtime.windows)
ENDIF()

IF (HAVE_WINDESKTOP_X64_LITE OR HAVE_WINDESKTOP)
  ADD_SUBDIRECTORY(Emgu.runtime.windows.msvc.rt.x64)
ENDIF()

IF (HAVE_WINDESKTOP_X86_LITE)
  ADD_SUBDIRECTORY(Emgu.runtime.windows.msvc.rt.x86)
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

add_subdirectory(Emgu.TF.Lite.runtime.ubuntu)
add_subdirectory(Emgu.TF.Lite.runtime.linux-arm)


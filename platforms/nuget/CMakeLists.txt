# --------------------------------------------------------
#  Copyright (C) 2004-2026 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

#add_subdirectory(Emgu.TF)
#add_subdirectory(Emgu.TF.Models)
#add_subdirectory(Emgu.TF.Protobuf)

IF (HAVE_WINDESKTOP)
  ADD_SUBDIRECTORY(Emgu.runtime.windows.msvc.rt.x64)
ENDIF()

IF (HAVE_WINDESKTOP_X86)
  ADD_SUBDIRECTORY(Emgu.runtime.windows.msvc.rt.x86)
ENDIF()

IF (WIN32 AND HAVE_WINDESKTOP)
  add_subdirectory(Emgu.runtime.windows.zlibwapi.x64)
  
  add_subdirectory(Emgu.runtime.windows.cuda.rt)
  add_subdirectory(Emgu.runtime.windows.cuda.dnn)
  add_subdirectory(Emgu.runtime.windows.cuda.dnn.adv)
  add_subdirectory(Emgu.runtime.windows.cuda.dnn.cnn.train)
  add_subdirectory(Emgu.runtime.windows.cuda.dnn.cnn.infer)
  add_subdirectory(Emgu.runtime.windows.cuda.dnn.ops)
  add_subdirectory(Emgu.runtime.windows.cuda.fft)
  add_subdirectory(Emgu.runtime.windows.cuda.sparse)
  add_subdirectory(Emgu.runtime.windows.cuda.blas)
  add_subdirectory(Emgu.runtime.windows.cuda.blas.lt)
  add_subdirectory(Emgu.runtime.windows.cuda.solver)
  add_subdirectory(Emgu.runtime.windows.cuda.solver.mg)
  
  add_subdirectory(Emgu.TF.runtime.windows)
ENDIF()

#IF (HAVE_ANDROID)
#  add_subdirectory(Emgu.TF.runtime.xamarin.android)
#ENDIF()

IF(HAVE_OSX)
  add_subdirectory(Emgu.TF.runtime.macos)
ENDIF()

add_subdirectory(Emgu.TF.runtime.ubuntu)
  
IF(HAVE_LINUX_ARM)
  add_subdirectory(Emgu.TF.runtime.debian-arm)
ENDIF()

IF(HAVE_LINUX_ARM64)
  add_subdirectory(Emgu.TF.runtime.debian-arm64)
ENDIF()

#add_subdirectory(Emgu.TF.runtime.maui)
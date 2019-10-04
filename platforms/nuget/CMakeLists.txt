# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF (HAVE_WINDESKTOP OR HAVE_ANDROID OR HAVE_OSX)
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

IF (HAVE_WINDESKTOP_X64_LITE)
	add_subdirectory(runtime.windows.Emgu.TF.Lite)
	add_subdirectory(Emgu.TF.Lite)
	add_subdirectory(Emgu.TF.Lite.Models)
ENDIF()

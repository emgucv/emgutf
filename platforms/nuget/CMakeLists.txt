# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

add_subdirectory(Emgu.TF)

IF (WIN32 AND (HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64) )
	add_subdirectory(runtime.windows.cuda.dnn.Emgu.TF)
	add_subdirectory(runtime.windows.cuda.cufft.Emgu.TF)
	add_subdirectory(Models)
	add_subdirectory(Emgu.TF.Protobuf)
	add_subdirectory(runtime.windows.Emgu.TF)
ENDIF()

IF ( HAVE_ANDROID )
	add_subdirectory(runtime.android.Emgu.TF)
ENDIF()

IF (HAVE_WINDESKTOP_X64_LITE)
	add_subdirectory(Lite)
	add_subdirectory(Lite.Models)
ENDIF()

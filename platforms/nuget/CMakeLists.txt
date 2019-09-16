# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

add_subdirectory(Emgu.TF)

IF (WIN32 AND (HAVE_WINDESKTOP_X86 OR HAVE_WINDESKTOP_X64) )
	add_subdirectory(Dnn)
	add_subdirectory(Cufft)
	add_subdirectory(Models)
	add_subdirectory(Protobuf)
ENDIF()

IF ( HAVE_ANDROID )
	add_subdirectory(Android)
ENDIF()

IF (HAVE_WINDESKTOP_X64_LITE)
	add_subdirectory(Lite)
	add_subdirectory(Lite.Models)
ENDIF()

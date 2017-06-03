package(default_visibility = ["//visibility:public"])

load(
    "//tensorflow:tensorflow.bzl",
    "tf_copts",
    "tf_opts_nortti_if_android",
	"if_android",
)

cc_binary(
	name = "libtfextern.so",
	srcs = ["tfextern.cc",
		"tfextern.h"
	],
   copts = tf_copts() + [	
        "-ffunction-sections",
        "-fdata-sections",
    ],
    linkopts = if_android([
        "-landroid",
        "-llog",
        "-lm",
	"-z defs",
        "-s",
        "-Wl,--gc-sections",
    ]),
    linkshared = 1,
    linkstatic = 1,
    tags = [
        "manual",
        "notap",
    ],
    deps = [
	"//tensorflow/c:c_api",
        #"//tensorflow/core:core_cpu",
	"//tensorflow/core:android_tensorflow_lib"
    ],
)
						    
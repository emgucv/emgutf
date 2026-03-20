# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Emgu TF is a cross-platform .NET wrapper for the Google TensorFlow library, enabling TF functions to be called from C#, VB, VC++, and IronPython. The repo contains two main wrappers:
- **Emgu TF** — full TensorFlow model loading/running (Windows, macOS, Linux, Android)
- **Emgu TF Lite** — TensorFlow Lite model loading/running (Windows, macOS, Linux, Android, iOS)

The native C++ layer is exposed through P/Invoke via two extern libraries: `tfextern` (full TF) and `tfliteextern` (TF Lite).

## Architecture

### Layer structure
1. **Native layer** — C/C++ wrapper DLLs built from the `tensorflow/` submodule using CMake or Bazel. The DLLs (`tfextern`, `tfliteextern`) must be present in `lib/runtimes/<rid>/native/` before building .NET code.
2. **P/Invoke layer** — `TfInvoke` (in `Emgu.TF/`) and `TfLiteInvoke` (in `Emgu.TF.Lite/`) are partial static classes that expose the native DLL entry points via `[DllImport]`.
3. **Managed wrappers** — `Graph`, `Session`, `Tensor`, `Interpreter`, etc. inherit `UnmanagedObject` (from `Emgu.TF.Util/`) and wrap native handles with proper lifetime management.
4. **Models layer** — `Emgu.TF.Models/` and `Emgu.TF.Lite.Models/` provide high-level pre-built model helpers (Inception, MobileNet, COCO SSD, etc.) that download weights and run inference.
5. **Platform runtime packages** — `Emgu.TF.Runtime/` contains shared project items (`.shproj`/`.projitems`) that pull in the correct native binaries for each platform (Windows, macOS, Debian, Ubuntu, Maui).
6. **Unity integration** — `Emgu.TF.Unity/` and `Emgu.TF.Lite.Unity/` are Unity projects that mirror the managed API for use in game engines.

### Code generation
`Graph.g.cs` and similar `*.g.cs` files are **auto-generated** by `Emgu.TF.CodeGen/`. After changing TF version or modifying the codegen project, regenerate by building `Emgu.TF.CodeGen` — CMake runs the generator as a post-build step automatically.

### Shared projects
`Emgu.TF.Shared.shproj`, `Emgu.TF.Lite.Shared.shproj`, etc. share source files across multiple target frameworks (NetStandard, Android, iOS, Unity). The `.projitems` files define which `.cs` files are included.

## Building

### Prerequisites
- .NET SDK
- Visual Studio 2022 or VS 2026 (detected automatically by `vswhere.exe`)
- CMake 3.16+
- TF Lite native DLL already built and placed under `lib/runtimes/`

### Build native TF Lite (Windows x64)
```bat
cd platforms/windows
cmake_build_tflite_x86_64.bat
```

### Build the full .NET solution (Windows)
```bat
cd platforms/windows
build_emgutf.bat
```
Optional args: `doc` (build docs), `nuget` (build NuGet packages), `package` (build zip packages).

### Build with CMake (cross-platform)
```bash
mkdir build && cd build
cmake <PATH_TO_EMGUTF_ROOT>
cmake --build . --config Release
```

### Visual Studio solutions
Platform-specific `.sln` files live under `Solution/`:
- `Solution/Windows.Desktop/` — `Emgu.TF.sln`, `Emgu.TF.Lite.sln`, test/example solutions
- `Solution/Android/`, `Solution/iOS/`, `Solution/CrossPlatform/`, `Solution/macos/`

## Running Tests

Tests use either NUnit or MSTest (selected at compile-time via the `VS_TEST` preprocessor symbol).

### TF full — run all tests
```bash
dotnet test Emgu.TF.Test/Emgu.TF.Test.Net/Emgu.TF.Test.Net.csproj
```

### TF Lite — run all tests
```bash
dotnet test Emgu.TF.Test/Emgu.TF.Lite.Test/Emgu.TF.Lite.Test.Net/Emgu.TF.Lite.Test.Net.csproj
```

### Run a single test
```bash
dotnet test <project.csproj> --filter "FullyQualifiedName~TestGetVersion"
```

Test assets (e.g., `grace_hopper.jpg`) must be present in the working directory when tests run.

## Key Conventions

- **Extern library names**: `tfextern` for full TF, `tfliteextern` for TF Lite. On iOS/Unity these resolve to `__Internal`.
- **`*.g.cs` files**: auto-generated — do not edit manually.
- **`UnmanagedObject`** (in `Emgu.TF.Util/`): base class for all objects wrapping a native pointer; handles `Dispose`/finalizer pattern.
- **Calling convention**: `CallingConvention.Cdecl` throughout.
- **Boolean marshaling**: `UnmanagedType.U1` for `bool`, `UnmanagedType.Bool` for `int`-as-bool.
- **Error handling**: native errors are redirected via a callback delegate (`TfLiteErrorCallback`) and thrown as managed exceptions.
- The `tensorflow/` directory is a submodule pinned to the version declared in `cmake/modules/TensorflowVersion.cmake`. Do not modify files inside it directly.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Emgu TF is a cross-platform .NET wrapper for the Google TensorFlow library, enabling TF functions to be called from C#, VB, VC++, and IronPython. It provides full TensorFlow model loading/running on Windows, macOS, Linux, and Android.

The native C++ layer is exposed through P/Invoke via the `tfextern` extern library.

## Architecture

### Layer structure
1. **Native layer** — C/C++ wrapper DLLs built from the `tensorflow/` submodule using CMake or Bazel. The DLL (`tfextern`) must be present in `lib/runtimes/<rid>/native/` before building .NET code.
2. **P/Invoke layer** — `TfInvoke` (in `Emgu.TF/`) is a partial static class that exposes the native DLL entry points via `[DllImport]`.
3. **Managed wrappers** — `Graph`, `Session`, `Tensor`, etc. inherit `UnmanagedObject` (from `Emgu.TF.Util/`) and wrap native handles with proper lifetime management.
4. **Models layer** — `Emgu.TF.Models/` provides high-level pre-built model helpers (Inception, MobileNet, COCO SSD, etc.) that download weights and run inference.
5. **Platform runtime packages** — `Emgu.TF.Runtime/` contains shared project items (`.shproj`/`.projitems`) that pull in the correct native binaries for each platform (Windows, macOS, Debian, Ubuntu, Maui).
6. **Unity integration** — `Emgu.TF.Unity/` is a Unity project that mirrors the managed API for use in game engines.

### Code generation
`Graph.g.cs` and similar `*.g.cs` files are **auto-generated** by `Emgu.TF.CodeGen/`. After changing TF version or modifying the codegen project, regenerate by building `Emgu.TF.CodeGen` — CMake runs the generator as a post-build step automatically.

### Shared projects
`Emgu.TF.Shared.shproj`, etc. share source files across multiple target frameworks (NetStandard, Android, Unity). The `.projitems` files define which `.cs` files are included.

## Building

### Prerequisites
- .NET SDK
- Visual Studio 2022 or VS 2026 (detected automatically by `vswhere.exe`)
- CMake 3.16+
- TF native DLL already built and placed under `lib/runtimes/`

### Build native TF full (Windows x64)

The full TensorFlow native binary (`tfextern.dll`) is built with **Bazel** via a helper script:

```powershell
cd platforms/windows
./bazel_build_tf_x86_64.ps1
```

The scripts are PowerShell scripts and work with Windows PowerShell 5.1 and PowerShell 7. If script execution is
blocked, run them as `powershell -ExecutionPolicy Bypass -File ./bazel_build_tf_x86_64.ps1`.

This calls `bazel_build_tf.ps1`, which:
1. Detects the newest installed Visual Studio (2019/2022/2026) and sets `BAZEL_VS`/`BAZEL_VC`.
2. Detects Python at `C:\Python312` or `C:\python-virt\python312` (falls back to `python` on PATH).
3. Runs Bazel (`bazel.exe` or `C:\msys64\usr\bin\bazel.exe`) to build `//tensorflow/tfextern:libtfextern.so` with `--config=win_clang`. The script fails if Bazel fails.
4. Copies the result to `lib/runtimes/win-x64/native/tfextern.dll` and deploys MSVC runtime DLLs alongside it.

**Prerequisites for the Bazel build:**
- Bazel (on PATH or at `C:\msys64\usr\bin\bazel.exe`)
- MSYS64 at `C:\msys64` (provides `bash.exe` used by the TF build scripts)
- Python 3.12 (ideally at `C:\Python312` or `C:\python-virt\python312`)
- Visual Studio 2019, 2022, or 2026 with C++ workload

**Optional switches of `bazel_build_tf.ps1`:**
- `-Avx2` — add `/arch:AVX2` to compiler flags for better CPU performance
- `-Fast` — low optimization, no debug info, no warnings: much faster to compile, but the binary is slower. For CI and development checks, not for binaries you ship. `-FastOptLevel Od|O1` selects the optimization level (default `Od`, about 25x faster to compile than the normal `/O2` on the slowest kernels; the DLL is much larger).
- `-Docker` — build inside a docker container
- `-DiskCache <dir>` — use Bazel's disk cache
- `-DryRun` — print the Bazel command and environment without running it

### Build the full .NET solution (Windows)
```powershell
cd platforms/windows
./build_emgutf.ps1
```
Optional switches: `-Doc` (build docs), `-Nuget` (build NuGet packages), `-Package` (build zip packages).

### Build with CMake (cross-platform)
```bash
mkdir build && cd build
cmake <PATH_TO_EMGUTF_ROOT>
cmake --build . --config Release
```

### Visual Studio solutions
Platform-specific `.sln` files live under `Solution/`:
- `Solution/Windows.Desktop/` — `Emgu.TF.sln`, test/example solutions
- `Solution/Android/`, `Solution/CrossPlatform/`, `Solution/macos/`

## Running Tests

Tests use either NUnit or MSTest (selected at compile-time via the `VS_TEST` preprocessor symbol).

### Run all tests
```bash
dotnet test Emgu.TF.Test/Emgu.TF.Test.Net/Emgu.TF.Test.Net.csproj
```

### Run a single test
```bash
dotnet test <project.csproj> --filter "FullyQualifiedName~TestGetVersion"
```

Test assets (e.g., `grace_hopper.jpg`) must be present in the working directory when tests run.

## Key Conventions

- **Extern library name**: `tfextern`. On Unity this resolves to `__Internal`.
- **`*.g.cs` files**: auto-generated — do not edit manually.
- **`UnmanagedObject`** (in `Emgu.TF.Util/`): base class for all objects wrapping a native pointer; handles `Dispose`/finalizer pattern.
- **Calling convention**: `CallingConvention.Cdecl` throughout.
- **Boolean marshaling**: `UnmanagedType.U1` for `bool`, `UnmanagedType.Bool` for `int`-as-bool.
- **Error handling**: native errors are surfaced via a `Status` object; `StatusChecker` throws a managed exception on disposal if the status is not OK.
- The `tensorflow/` directory is a submodule pinned to the version declared in `cmake/modules/TensorflowVersion.cmake`. Do not modify files inside it directly.

#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIGURATION="Release"
RID="osx-arm64"
CLEAN=0
JOBS="${JOBS:-$(sysctl -n hw.ncpu 2>/dev/null || echo 4)}"
OUTPUT_DIR=""

usage() {
    cat <<'EOF'
Usage: ./build-macos-demo.sh [options]

Builds the native LVGL dylib for macOS and publishes the MacOsAotDemo app.

Options:
  --clean                 Remove previous build/publish output first.
  --configuration <cfg>   Build configuration. Default: Release
  --rid <rid>             Runtime identifier. Default: osx-arm64
                          Supported: osx-arm64, osx-x64
  -j, --jobs <count>      Parallel build jobs. Default: hw.ncpu
  --output <dir>          Publish output directory.
                          Default: dist/<rid>/MacOsAotDemo
  -h, --help              Show this help.

Examples:
  ./build-macos-demo.sh --clean
  ./build-macos-demo.sh --rid osx-x64 --configuration Debug
EOF
}

fail() {
    printf 'error: %s\n' "$*" >&2
    exit 1
}

get_cmd_path_or_null() {
    command -v "$1" 2>/dev/null || true
}

require_cmd() {
    local name="$1"
    local install_hint="${2:-}"
    local command_path

    command_path="$(get_cmd_path_or_null "$name")"
    if [[ -z "$command_path" ]]; then
        local message="missing required command: $name"
        if [[ -n "$install_hint" ]]; then
            message+=$'\ninstall hint: '
            message+="$install_hint"
        fi

        message+=$'\nminimum requirements: .NET SDK 10, CMake, Ninja, Xcode Command Line Tools.'
        fail "$message"
    fi

    printf '%s' "$command_path"
}

show_minimal_requirements() {
    printf '==> Minimal Requirements\n'
    printf '    1. .NET SDK 10.x\n'
    printf '    2. CMake\n'
    printf '    3. Ninja\n'
    printf '    4. Xcode Command Line Tools\n'
    printf '    5. macOS 13+ recommended\n'
}

assert_build_prerequisites() {
    local cmake_path dotnet_path ninja_path

    cmake_path="$(require_cmd cmake 'brew install cmake')"
    dotnet_path="$(require_cmd dotnet 'Install .NET SDK 10 and make sure dotnet is available in PATH.')"
    ninja_path="$(require_cmd ninja 'brew install ninja')"
    require_cmd cp 'The system cp command is sufficient.' >/dev/null
    require_cmd find 'The system find command is sufficient.' >/dev/null
    require_cmd mkdir 'The system mkdir command is sufficient.' >/dev/null
    require_cmd rm 'The system rm command is sufficient.' >/dev/null
    require_cmd sysctl 'The system sysctl command is sufficient.' >/dev/null

    printf '==> Checking Build Prerequisites\n'
    printf '    dotnet: %s\n' "$dotnet_path"
    printf '    cmake : %s\n' "$cmake_path"
    printf '    ninja : %s\n' "$ninja_path"
}

resolve_arch() {
    case "$1" in
        osx-arm64)
            printf 'arm64'
            ;;
        osx-x64)
            printf 'x86_64'
            ;;
        *)
            fail "unsupported RID: $1"
            ;;
    esac
}

while (($# > 0)); do
    case "$1" in
        --clean)
            CLEAN=1
            ;;
        --configuration)
            shift
            (($# > 0)) || fail "--configuration requires a value"
            CONFIGURATION="$1"
            ;;
        --rid)
            shift
            (($# > 0)) || fail "--rid requires a value"
            RID="$1"
            ;;
        -j|--jobs)
            shift
            (($# > 0)) || fail "--jobs requires a value"
            JOBS="$1"
            ;;
        --output)
            shift
            (($# > 0)) || fail "--output requires a value"
            OUTPUT_DIR="$1"
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            fail "unknown argument: $1"
            ;;
    esac
    shift
done

show_minimal_requirements
assert_build_prerequisites

MACOS_ARCH="$(resolve_arch "$RID")"
LVGL_SOURCE_DIR="$ROOT_DIR/libs/lvgl"
LVGL_BUILD_DIR="$ROOT_DIR/libs/build/lvgl-$RID"
LVGL_RUNTIME_DIR="$ROOT_DIR/src/LVGLSharp.Native/runtimes/$RID/native"
LVGL_RUNTIME_LIB="$LVGL_RUNTIME_DIR/liblvgl.dylib"
DEMO_PROJECT="$ROOT_DIR/src/Demos/MacOsAotDemo/MacOsAotDemo.csproj"
PUBLISH_DIR="${OUTPUT_DIR:-$ROOT_DIR/dist/$RID/MacOsAotDemo}"

if ((CLEAN)); then
    rm -rf "$LVGL_BUILD_DIR" "$PUBLISH_DIR"
fi

mkdir -p "$LVGL_RUNTIME_DIR"

printf '==> Building LVGL shared library (%s / %s)\n' "$RID" "$MACOS_ARCH"
cp "$ROOT_DIR/libs/lv_conf.h" "$ROOT_DIR/libs/lvgl/lv_conf.h"

cmake -S "$LVGL_SOURCE_DIR" -B "$LVGL_BUILD_DIR" \
    -G Ninja \
    -DCMAKE_BUILD_TYPE="$CONFIGURATION" \
    -DCMAKE_OSX_ARCHITECTURES="$MACOS_ARCH" \
    -DBUILD_SHARED_LIBS=ON \
    -DCONFIG_LV_BUILD_EXAMPLES=OFF \
    -DCONFIG_LV_BUILD_DEMOS=OFF \
    -DCONFIG_LV_USE_THORVG_INTERNAL=OFF \
    -DCONFIG_LV_USE_PRIVATE_API=ON \
    -DLV_BUILD_CONF_DIR="$ROOT_DIR/libs"

cmake --build "$LVGL_BUILD_DIR" -j"$JOBS"

LVGL_DYLIB="$(find "$LVGL_BUILD_DIR" -name 'liblvgl.dylib' | head -1)"
[[ -n "$LVGL_DYLIB" ]] || fail "missing built LVGL dylib under $LVGL_BUILD_DIR"

cp "$LVGL_DYLIB" "$LVGL_RUNTIME_LIB"
printf '    native: %s\n' "$LVGL_RUNTIME_LIB"

printf '==> Publishing MacOsAotDemo (%s)\n' "$RID"
dotnet publish "$DEMO_PROJECT" \
    -c "$CONFIGURATION" \
    -r "$RID" \
    -o "$PUBLISH_DIR"

[[ -d "$PUBLISH_DIR" ]] || fail "missing publish output: $PUBLISH_DIR"

printf '==> Done\n'
printf 'Published app under %s\n' "$PUBLISH_DIR"

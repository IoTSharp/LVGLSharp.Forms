#!/bin/bash
# LVGL 编译脚本 - Linux 版本
# 支持多种后端：framebuffer, sdl, drm, wayland
# 用法: ./build_lvgl.sh [后端] [架构]
# 示例: ./build_lvgl.sh sdl x64
#       ./build_lvgl.sh fb arm64

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
LVGL_DIR="$SCRIPT_DIR/lvgl"
BUILD_DIR="$SCRIPT_DIR/build"
OUTPUT_DIR="$SCRIPT_DIR/../src/LVGLSharp.Native/runtimes"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 默认值
BACKEND="fb"
ARCH=$(uname -m)

# 帮助信息
show_help() {
    echo -e "${GREEN}LVGL 编译脚本${NC}"
    echo ""
    echo "用法: $0 [后端] [架构]"
    echo ""
    echo -e "${BLUE}后端选项:${NC}"
    echo "  fb      - Linux Framebuffer (默认)"
    echo "  sdl     - SDL2 后端 (需要 libsdl2-dev)"
    echo "  drm     - DRM/KMS 后端 (需要 libdrm-dev)"
    echo "  wayland - Wayland 后端 (需要 wayland-client)"
    echo "  all     - 编译所有后端"
    echo ""
    echo -e "${BLUE}架构选项:${NC}"
    echo "  x64     - x86_64 (默认)"
    echo "  arm64   - aarch64"
    echo "  arm     - armhf"
    echo ""
    echo -e "${BLUE}示例:${NC}"
    echo "  $0 sdl x64       # 用 SDL2 后端编译 x64"
    echo "  $0 fb            # 用 Framebuffer 后端编译本机架构"
    echo "  $0 all x64       # 编译所有后端"
    exit 0
}

# 解析参数
while [ "$#" -gt 0 ]; do
    case "$1" in
        -h|--help)
            show_help
            ;;
        fb|framebuffer)
            BACKEND="fb"
            ;;
        sdl|sdl2)
            BACKEND="sdl"
            ;;
        drm)
            BACKEND="drm"
            ;;
        wayland)
            BACKEND="wayland"
            ;;
        all)
            BACKEND="all"
            ;;
        x64|x86_64|linux-x64)
            ARCH="x64"
            ;;
        arm64|aarch64|linux-arm64)
            ARCH="arm64"
            ;;
        arm|armhf|linux-arm)
            ARCH="arm"
            ;;
        *)
            echo -e "${RED}未知参数: $1${NC}"
            show_help
            ;;
    esac
    shift
done

echo -e "${GREEN}=== LVGL 编译脚本 ===${NC}"
echo -e "后端: ${YELLOW}$BACKEND${NC}"
echo -e "架构: ${YELLOW}$ARCH${NC}"
echo ""

# 检查 LVGL 源码
if [ ! -d "$LVGL_DIR" ]; then
    echo -e "${RED}错误: LVGL 源码目录不存在: $LVGL_DIR${NC}"
    echo "请先签出子模块: git submodule update --init --recursive"
    exit 1
fi

# 检查 cmake
if ! command -v cmake &> /dev/null; then
    echo -e "${RED}错误: cmake 未安装${NC}"
    echo "请安装 cmake: sudo apt install cmake"
    exit 1
fi

# 检查依赖
check_dependencies() {
    local backend=$1
    case "$backend" in
        sdl)
            if ! pkg-config --exists sdl2 2>/dev/null; then
                echo -e "${YELLOW}安装 SDL2 依赖...${NC}"
                sudo apt-get install -y libsdl2-dev
            fi
            ;;
        drm)
            if ! pkg-config --exists libdrm 2>/dev/null; then
                echo -e "${YELLOW}安装 DRM 依赖...${NC}"
                sudo apt-get install -y libdrm-dev
            fi
            ;;
        wayland)
            if ! pkg-config --exists wayland-client 2>/dev/null; then
                echo -e "${YELLOW}安装 Wayland 依赖...${NC}"
                sudo apt-get install -y wayland-protocols libwayland-dev libxkbcommon-dev
            fi
            ;;
    esac
}

# 编译函数
build_lvgl() {
    local backend=$1
    local target_arch=$2
    local build_name="linux-${target_arch}-${backend}"
    local build_path="$BUILD_DIR/$build_name"
    
    echo -e "${YELLOW}编译 $build_name...${NC}"
    
    # 检查依赖
    check_dependencies "$backend"
    
    mkdir -p "$build_path"
    cd "$build_path"
    
    # cmake 配置 - 通用参数
    local CMAKE_ARGS="-DCMAKE_BUILD_TYPE=Release \
        -DBUILD_SHARED_LIBS=ON \
        -DCONFIG_LV_BUILD_EXAMPLES=OFF \
        -DCONFIG_LV_BUILD_DEMOS=OFF \
        -DCONFIG_LV_USE_THORVG_INTERNAL=OFF"
    
    # 根据后端设置不同参数
    case "$backend" in
        fb)
            CMAKE_ARGS="$CMAKE_ARGS \
                -DCONFIG_LV_USE_LINUX_FBDEV=ON \
                -DCONFIG_LV_USE_SDL=OFF \
                -DCONFIG_LV_USE_LINUX_DRM=OFF \
                -DCONFIG_LV_USE_WAYLAND=OFF"
            ;;
        sdl)
            CMAKE_ARGS="$CMAKE_ARGS \
                -DCONFIG_LV_USE_LINUX_FBDEV=OFF \
                -DCONFIG_LV_USE_SDL=ON \
                -DCONFIG_LV_USE_LINUX_DRM=OFF \
                -DCONFIG_LV_USE_WAYLAND=OFF"
            ;;
        drm)
            CMAKE_ARGS="$CMAKE_ARGS \
                -DCONFIG_LV_USE_LINUX_FBDEV=OFF \
                -DCONFIG_LV_USE_SDL=OFF \
                -DCONFIG_LV_USE_LINUX_DRM=ON \
                -DCONFIG_LV_USE_WAYLAND=OFF"
            ;;
        wayland)
            CMAKE_ARGS="$CMAKE_ARGS \
                -DCONFIG_LV_USE_LINUX_FBDEV=OFF \
                -DCONFIG_LV_USE_SDL=OFF \
                -DCONFIG_LV_USE_LINUX_DRM=OFF \
                -DCONFIG_LV_USE_WAYLAND=ON"
            ;;
    esac
    
    # 交叉编译工具链
    local toolchain_file=""
    case "$target_arch" in
        arm64)
            toolchain_file="$SCRIPT_DIR/cmake/toolchains/aarch64-linux-gnu.cmake"
            if [ -f "$toolchain_file" ]; then
                CMAKE_ARGS="$CMAKE_ARGS -DCMAKE_TOOLCHAIN_FILE=$toolchain_file"
            else
                echo -e "${YELLOW}警告: 工具链文件不存在，使用本机编译${NC}"
            fi
            ;;
        arm)
            toolchain_file="$SCRIPT_DIR/cmake/toolchains/arm-linux-gnueabihf.cmake"
            if [ -f "$toolchain_file" ]; then
                CMAKE_ARGS="$CMAKE_ARGS -DCMAKE_TOOLCHAIN_FILE=$toolchain_file"
            else
                echo -e "${YELLOW}警告: 工具链文件不存在，使用本机编译${NC}"
            fi
            ;;
    esac
    
    # 运行 cmake
    echo -e "${BLUE}cmake 配置...${NC}"
    cmake $CMAKE_ARGS "$LVGL_DIR"
    
    # 编译
    echo -e "${BLUE}编译中...${NC}"
    cmake --build . --config Release -j$(nproc)
    
    # 复制输出
    local output_path="$OUTPUT_DIR/linux-${target_arch}-${backend}/native"
    mkdir -p "$output_path"
    
    if [ -f "lib/liblvgl.so" ] || [ -f "lib/liblvgl.so.9" ]; then
        cp lib/liblvgl.so* "$output_path/" 2>/dev/null || true
        echo -e "${GREEN}✓ 已复制到: $output_path/${NC}"
        ls -la "$output_path/"*.so* 2>/dev/null || true
    else
        echo -e "${RED}错误: 未找到编译输出文件${NC}"
        exit 1
    fi
    
    cd "$SCRIPT_DIR"
}

# 根据参数编译
if [ "$BACKEND" = "all" ]; then
    for b in fb sdl drm wayland; do
        build_lvgl "$b" "$ARCH"
        echo ""
    done
else
    build_lvgl "$BACKEND" "$ARCH"
fi

echo ""
echo -e "${GREEN}=== 编译完成 ===${NC}"
echo -e "输出目录: ${YELLOW}$OUTPUT_DIR${NC}"
echo ""
echo -e "${BLUE}编译结果:${NC}"
find "$OUTPUT_DIR" -name "liblvgl.so*" -type f 2>/dev/null | while read f; do
    SIZE=$(du -h "$f" | cut -f1)
    echo "  $f ($SIZE)"
done
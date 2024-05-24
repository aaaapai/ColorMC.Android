cmake_minimum_required(VERSION 3.22.1)

project("colormcnative")

set(ANDROID_PLATFORM android-26)

add_executable(${CMAKE_PROJECT_NAME} "test.c")

find_library(android-lib egl)

add_library("colormcnative_display" SHARED
        display/display.c
        display/shared.cpp
        display/shared_texture.cpp)

target_link_libraries("colormcnative_display"
        log
        android
        GLESv2
        ${android-lib})

add_library("colormcnative_run" SHARED
        run/run.c
        run/lwjgl_bridges/events.c
        run/lwjgl_bridges/lwjgl_use.c
        run/game_sock.c
        run/render_sock.c
        run/context_list.c
        run/render/render_gl4es.c
        run/render/render_zink.c
        run/dl_loader/gl_loader.c
        run/dl_loader/egl_loader.c
        run/dl_loader/gles_loader.c
        run/dl_loader/mesa_loader.c
        run/dl_loader/ah_loader.c
        run/test/run_shared_texture.c)
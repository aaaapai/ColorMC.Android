//
// Created by 40206 on 2024/5/23.
//
#include <stdbool.h>
#include <malloc.h>

#include "render_zink.h"

#include "../run.h"
#include "../GL/osmesa.h"
#include "../context_list.h"
#include "../dl_loader/mesa_loader.h"
#include "../lwjgl_bridges/events.h"

uint8_t* framebuffer;

bool mesa_create() {
    // 创建一个缓冲区来存储渲染结果
    framebuffer = malloc(width * height * 4);

    return framebuffer != NULL;
}

bool mesa_create_context(context_env * env) {
    // 创建 OsMesa 上下文
    env->context = OSMesaCreateContext_p(OSMESA_RGBA, env->context);
    if (env->context == NULL) {
        printf("[ColorMC Error] OSMesaCreateContext_p() error");
        fflush(stdout);
        return false;
    }

    return true;
}

bool mesa_make_current(context_env* env) {
    if (env == NULL) {
        //进行context取消绑定
        printf("[ColorMC Info] unbind mesa context\n");
        fflush(stdout);
        OSMesaMakeCurrent_p(NULL, NULL, 0, 0, 0);
        return true;
    }

    if (!OSMesaMakeCurrent_p(env->context, framebuffer, GL_UNSIGNED_BYTE, width, height)) {
        printf("[ColorMC Error] OSMesaMakeCurrent_p error");
        return false;
    }
    OSMesaPixelStore_p(OSMESA_ROW_LENGTH, width * 4);
    OSMesaPixelStore_p(OSMESA_Y_UP, 0);

    return true;
}
bool mesa_destroy_context(context_env* env) {
    OSMesaDestroyContext_p(env->context);
    return true;
}

void mesa_change_size() {
    printf("[ColorMC Info] mesa reload\n");
    fflush(stdout);
    context_env *env = now_env;
    if (env == NULL) {
        return;
    }
    mesa_close();
    ah_delete_buffer();

    ah_create_buffer();
    mesa_create();
    mesa_make_current(env);
}

void mesa_swap_buffers() {
    if (now_env == NULL) {
        return;
    }

    if (!OSMesaMakeCurrent_p(now_env->context, framebuffer, GL_UNSIGNED_BYTE, width, height)) {
        printf("[ColorMC Error] OSMesaMakeCurrent_p error");
        return;
    }
    OSMesaPixelStore_p(OSMESA_ROW_LENGTH, width * 4);
    glFinish_p(); // this will force osmesa to write the last rendered image into the buffer

    ah_copy_data(framebuffer);

    if (render_state == RENDER_CHANGE_SIZE) {
        render_state = RENDER_RUN;
        send_screen_size(width, height);
        mesa_change_size();
    }
}

void mesa_swap_interval(int interval) {

}

void mesa_close() {
    mesa_make_current(NULL);
    free(framebuffer);
}
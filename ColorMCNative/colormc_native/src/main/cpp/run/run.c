#include <stdbool.h>
#include <android/hardware_buffer.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

#include "GL/gl.h"

#include "render_sock.h"
#include "game_sock.h"
#include "run.h"

#include "dl_loader/egl_loader.h"
#include "dl_loader/gl_loader.h"
#include "dl_loader/gles_loader.h"
#include "dl_loader/ah_loader.h"
#include "lwjgl_bridges/events.h"
#include "context_list.h"
#include "render/render_gl4es.h"
#include "render/render_zink.h"
#include "dl_loader/mesa_loader.h"

#define EXTERNAL_API __attribute__((used))

int width = 640;
int height = 480;
int gles_version = 3;

bool v2 = false;
bool can_run = false;

uint8_t render_state;
uint8_t render_type;

AHardwareBuffer *a_buffer = NULL;

/**
 * 创建一个native buffer
 */
void ah_create_buffer() {
    AHardwareBuffer_Desc desc = {
            width,
            height,
            1,
            AHARDWAREBUFFER_FORMAT_R8G8B8A8_UNORM,
            AHARDWAREBUFFER_USAGE_CPU_READ_NEVER
            | AHARDWAREBUFFER_USAGE_CPU_WRITE_OFTEN
            | AHARDWAREBUFFER_USAGE_GPU_SAMPLED_IMAGE
            | AHARDWAREBUFFER_USAGE_GPU_COLOR_OUTPUT,
            0,
            0,
            0};
    int errCode = AHardwareBuffer_allocate_p(&desc, &a_buffer);
    if (errCode != 0 || !a_buffer) {
        printf("[ColorMC Error] Make AHardwarea allocate failed error: %d\n", errCode);
    } else {
        printf("[ColorMC Info] create AHardwareBuffer size %d x %d\n", width, height);
    }

    fflush(stdout);
}

/**
 * 删除buffer
 */
void ah_delete_buffer() {
    if (a_buffer) {
        AHardwareBuffer_release_p(a_buffer);
        a_buffer = NULL;
    }
}

void ah_copy_data(void* data) {
    AHardwareBuffer_Desc desc;
    AHardwareBuffer_describe_p(a_buffer, &desc);

    // 锁定AHardwareBuffer以获取其原始指针
    void *destBuffer = NULL;
    AHardwareBuffer_lock_p(a_buffer, AHARDWAREBUFFER_USAGE_CPU_WRITE_OFTEN, -1, NULL,
                           &destBuffer);

    if (!destBuffer) {
        printf("[ColorMC Error] Failed to lock AHardwareBuffer.");
        exit(1);
    }

    // 复制OSMesa的像素数据到AHardwareBuffer
    int pixelSize = 4; // 假设每个像素4字节（RGBA）
    memcpy(destBuffer, data, width * height * pixelSize);

    // 解锁AHardwareBuffer
    AHardwareBuffer_unlock_p(a_buffer, NULL);
}

void egl_start_change_size() {
    render_state = RENDER_CHANGE_SIZE;
}

bool sendTexture(int sock) {
    if (a_buffer == NULL) {
        return false;
    }
    return AHardwareBuffer_sendHandleToUnixSocket_p(a_buffer, sock) == 0;
}

/*
 * 获取正在使用的context
 */
void* game_get_context() {
    printf("[ColorMC Info] game_get_context\n");
    if (now_env == NULL) {
        printf("[ColorMC Info] game_get_context no now env\n");
        return NULL;
    }
    printf("[ColorMC Info] game_get_context output: %p\n", now_env->context);
    fflush(stdout);
    return now_env->context;
}


void* game_create_context(void* context) {
    printf("[ColorMC Info] game_create_context input: %p\n", context);

    context_env *env = context_find_empty();
    if (env == NULL) {
        printf("[ColorMC Error] gl context is full\n");
        fflush(stdout);
        exit(1);
    }
    env->share = context;

    if (render_type == GL4ES) {
        if (egl_create_context(env)) {
            return env->context;
        }
    } else if (render_type == ZINK) {
        if (mesa_create_context(env)) {
            return env->context;
        }
    }

    return NULL;
}

void game_make_current(void* window) {
    printf("[ColorMC Info] game_make_current context: %p\n", window);

    context_env *env = context_find_match(window);
    if(env == NULL) {
        printf("[ColorMC Error] can't found context\n");
        fflush(stdout);
        exit(1);
    }
    if (render_type == GL4ES) {
        if (!egl_make_current(env)) {
            printf("[ColorMC Error] gl4es context make current fail\n");
            fflush(stdout);
            exit(1);
        }
    } else if (render_type == ZINK) {
        if (!mesa_make_current(window)) {
            printf("[ColorMC Error] mesa context make current fail\n");
            fflush(stdout);
            exit(1);
        }
        printf("OSMDroid: vendor: %s\n", glGetString_p(GL_VENDOR));
        printf("OSMDroid: renderer: %s\n", glGetString_p(GL_RENDERER));
    }
    now_env = env;
    showingWindow = env->context;
}

void game_destroy_context(void* window) {
    printf("[ColorMC Info] egl_destroy_context input: %p\n", window);

    context_env *env = context_find_match(window);
    if (env == NULL) {
        printf("[ColorMC Error] gl context can't find\n");
        fflush(stdout);
        exit(1);
    }
    if (now_env == env) {
        if (render_type == GL4ES) {
            egl_make_current(NULL);
        } else if (render_type == ZINK) {
            mesa_make_current(NULL);
        }
        now_env = NULL;
    }

    if (render_type == GL4ES) {
        egl_destroy_context(env);
    } else if (render_type == ZINK) {
        mesa_destroy_context(env);
    }

    context_remove(env);
}

void game_swap_buffers() {
    if (now_env == NULL) {
        return;
    }

    if (render_type == GL4ES) {
        egl_swap_buffers();
    } else if (render_type == ZINK) {
        mesa_swap_buffers();
    }
}

void game_swap_interval(int interval) {
    printf("[ColorMC Info] game_swap_interval\n");

    if (render_type == GL4ES) {
        egl_swap_interval(interval);
    } else if (render_type == ZINK) {
        mesa_swap_interval(interval);
    }
}

void game_close() {
    printf("[ColorMC Info] game_close\n");

    if (render_type == GL4ES) {
        egl_close();
    } else if (render_type == ZINK) {
        mesa_close();
    }

    if (now_env != NULL) {
        if (render_type == GL4ES) {
            egl_destroy_context(now_env);
        } else if (render_type == ZINK) {
            mesa_destroy_context(now_env);
        }
        now_env = NULL;
    }
}

bool game_init() {
    printf("[ColorMC Info] game_init\n");

    //显示宽度
    char *temp1 = getenv("glfwstub.windowWidth");
    if (temp1 == NULL) {
        printf("[ColorMC Error] no set glfwstub.windowWidth env\n");
        width = 0;
    } else {
        width = strtol(temp1, NULL, 0);
    }

    if (width <= 0) {
        printf("[ColorMC Error] window width error set to 640\n");
        width = 640;
    }

    //显示高度
    temp1 = getenv("glfwstub.windowHeight");
    if (temp1 == NULL) {
        printf("[ColorMC Error] no set glfwstub.windowHeight env\n");
        height = 0;
    } else {
        height = strtol(temp1, NULL, 0);
    }

    //渲染器类型
    temp1 = getenv("GAME_RENDER");
    if (temp1 == NULL) {
        printf("[ColorMC Error] no GAME_RENDER\n");
        return false;
    }
    if (strcmp(temp1, "gl4es") == 0) {
        render_type = GL4ES;
    } else if (strcmp(temp1, "zink") == 0) {
        render_type = ZINK;
    } else {
        printf("[ColorMC Error] unsupper GAME_RENDER :%s\n", temp1);
        return false;
    }

    printf("[ColorMC Info] use GAME_RENDER :%d(%s)\n", render_type, temp1);

    //GLES版本
    temp1 = getenv("GL_ES_VERSION");
    if (temp1 != NULL) {
        gles_version = strtol(temp1, NULL, 0);
        if (gles_version < 0 || gles_version > INT16_MAX) gles_version = 2;
    }

    if (height <= 0) {
        printf("[ColorMC Error] window height error set to 640\n");
        height = 480;
    }

    //启动sock服务器
    if (game_sock_server() == false) {
        printf("[ColorMC Error] sock init fail\n");
        fflush(stdout);
        return false;
    }

    if (render_sock_server() == false) {
        printf("[ColorMC Error] sock init fail\n");
        fflush(stdout);
        return false;
    }

    //等待链接
    while (!can_run) {
        printf("[ColorMC Info] wait run start\n");
        fflush(stdout);
        sleep(1);
    }

    if (ah_dlopen() == false) {
        printf("[ColorMC Error] AH load fail\n");
        fflush(stdout);
        return false;
    }

    context_list_init();
    ah_create_buffer();

    if (render_type == GL4ES) {
        //加载符号
        if (egl_dlopen() == false) {
            printf("[ColorMC Error] egl load fail\n");
            fflush(stdout);
            return false;
        }
        if (gl_dlopen() == false) {
            printf("[ColorMC Error] gl load fail\n");
            fflush(stdout);
            return false;
        }
        if (gles_dlopen() == false) {
            printf("[ColorMC Error] gl load fail\n");
            fflush(stdout);
            return false;
        }

        //创建egl环境
        if (!egl_create()) {
            printf("[ColorMC Error] Egl create fail\n");
            fflush(stdout);
            return false;
        }
        printf("[ColorMC Info] gl4es load done\n");
    } else if(render_type == ZINK) {
        setenv("GALLIUM_DRIVER","zink",1);
        if (!mesa_dlopen()) {
            printf("[ColorMC Error] mesa load fail\n");
            fflush(stdout);
            return false;
        }
        //创建mesa环境
        if (!mesa_create()) {
            printf("[ColorMC Error] Mesa create fail\n");
            fflush(stdout);
            return false;
        }
        printf("[ColorMC Info] zink load done\n");
    }

    fflush(stdout);

    return true;
}
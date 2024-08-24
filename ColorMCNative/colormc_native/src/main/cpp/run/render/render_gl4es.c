//
// Created by 40206 on 2024/5/23.
//
#include <stdlib.h>
#include <stdio.h>

#include "../run.h"
#include "../context_list.h"
#include "../render_sock.h"
#include "../game_sock.h"
#include "../dl_loader/egl_loader.h"
#include "../dl_loader/gles_loader.h"
#include "../lwjgl_bridges/events.h"

EGLDisplay display = EGL_NO_DISPLAY;
EGLSurface surface = EGL_NO_SURFACE;
EGLConfig config;
EGLint format;

EGLImageKHR eglImage = EGL_NO_IMAGE_KHR;

const char *getEGLError() {
    switch (eglGetError_p()) {
        case EGL_SUCCESS:
            return "EGL_SUCCESS";
        case EGL_NOT_INITIALIZED:
            return "EGL_NOT_INITIALIZED";
        case EGL_BAD_ACCESS:
            return "EGL_BAD_ACCESS";
        case EGL_BAD_ALLOC:
            return "EGL_BAD_ALLOC";
        case EGL_BAD_ATTRIBUTE:
            return "EGL_BAD_ATTRIBUTE";
        case EGL_BAD_CONTEXT:
            return "EGL_BAD_CONTEXT";
        case EGL_BAD_CONFIG:
            return "EGL_BAD_CONFIG";
        case EGL_BAD_CURRENT_SURFACE:
            return "EGL_BAD_CURRENT_SURFACE";
        case EGL_BAD_DISPLAY:
            return "EGL_BAD_DISPLAY";
        case EGL_BAD_SURFACE:
            return "EGL_BAD_SURFACE";
        case EGL_BAD_MATCH:
            return "EGL_BAD_MATCH";
        case EGL_BAD_PARAMETER:
            return "EGL_BAD_PARAMETER";
        case EGL_BAD_NATIVE_PIXMAP:
            return "EGL_BAD_NATIVE_PIXMAP";
        case EGL_BAD_NATIVE_WINDOW:
            return "EGL_BAD_NATIVE_WINDOW";
        case EGL_CONTEXT_LOST:
            return "EGL_CONTEXT_LOST";
        default:
            return "Unknown error";
    }
}


//交换egl_buffer
void egl_swap_interval(int swapInterval) {
    eglSwapInterval_p(display, swapInterval);
}

//创建surface
void egl_create_surface() {
    printf("[ColorMC Info] egl create surface\n");
    // 创建Pbuffer表面
    EGLint pbufferAttribs[] = {
            EGL_WIDTH, width,
            EGL_HEIGHT, height,
            EGL_NONE
    };
    surface = eglCreatePbufferSurface_p(display, config, pbufferAttribs);
    if (surface == NULL) {
        printf("[ColorMC Error] createPbufferSurface failed: %s\n", getEGLError());
    }
    printf("[ColorMC Info] egl surface size %d x %d\n", width, height);
    fflush(stdout);
}

//创建image
void egl_create_image() {
    printf("[ColorMC Info] egl_create_image\n");
    EGLClientBuffer clientBuffer = eglGetNativeClientBufferANDROID_p(a_buffer);
    if (!clientBuffer) {
        printf("[ColorMC Error] bindTexture failed: clientBuffer null\n");
        fflush(stdout);
        return;
    }

    EGLint eglImageAttributes[] =
    {
        EGL_IMAGE_PRESERVED_KHR, EGL_TRUE,
        EGL_NONE
    };
    eglImage = eglCreateImageKHR_p(display, EGL_NO_CONTEXT,
                                   EGL_NATIVE_BUFFER_ANDROID,
                                   clientBuffer, eglImageAttributes);
    if (eglImage == EGL_NO_IMAGE_KHR) {
        printf("[ColorMC Error] bindTexture failed: eglCreateImageKHR null: %s\n", getEGLError());
        fflush(stdout);
        return;
    }
}

/*
 * egl创建
 * 初始化EGL，创建显示器
 * 创建EGL配置
 * 创建native buffer
 * 创建surface与image
 */
bool egl_create() {
    printf("[ColorMC Info] egl create\n");

    display = eglGetDisplay_p(EGL_DEFAULT_DISPLAY);
    if (display == EGL_NO_DISPLAY) {
        printf("[ColorMC Error] eglGetDisplay_p() returned EGL_NO_DISPLAY: %s\n", getEGLError());
        fflush(stdout);
        return false;
    }
    if (eglInitialize_p(display, NULL, NULL) != EGL_TRUE) {
        printf("[ColorMC Error] eglInitialize_p() failed: %s\n", getEGLError());
        fflush(stdout);
        return false;
    }

    // 配置EGL属性
    EGLint configAttribs[] = {
            EGL_BLUE_SIZE, 8,
            EGL_GREEN_SIZE, 8,
            EGL_RED_SIZE, 8,
            EGL_ALPHA_SIZE, 8,
            EGL_DEPTH_SIZE, 24,
            EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
            EGL_NONE, 0,
            EGL_NONE
    };

    // 选择EGL配置
    EGLint numConfigs;
    if (eglChooseConfig_p(display, configAttribs,
                          &config, 1, &numConfigs) != EGL_TRUE) {
        printf("[ColorMC Error] eglChooseConfig_p() failed: %s\n", getEGLError());
        fflush(stdout);
        return NULL;
    }

    if (numConfigs == 0) {
        printf("[ColorMC Error] eglChooseConfig_p() found no matching config\n");
        fflush(stdout);
        return NULL;
    }

    egl_create_surface();
    egl_create_image();

    return true;
}


/*
 * gl创建材质，用于显存共享
 * 创建一个材质，设置属性
 * 与EGLimage进行绑定
 */
void gl_create_texture(context_env * env){
    printf("[ColorMC Info] gl create texture\n");
    glGenTextures_p(1, &env->texture);
    glBindTexture_p(GL_TEXTURE_2D, env->texture);

    glTexParameteri_p(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri_p(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri_p(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri_p(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

    glEGLImageTargetTexture2DOES_p(GL_TEXTURE_2D, (GLeglImageOES) eglImage);
    glBindTexture_p(GL_TEXTURE_2D, 0);

    fflush(stdout);
}

/*
 * 创建gl的fbo用于显存共享
 * 创建一个fbo
 * 将材质附加到fbo中
 */
void gl_create_fbo(context_env * env){
    glGenFramebuffers_p(1, &env->fbo);
    glBindFramebuffer_p(GL_FRAMEBUFFER, env->fbo);
    glFramebufferTexture2D_p(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, env->texture, 0);
    GLenum fboStatus = glCheckFramebufferStatus_p(GL_FRAMEBUFFER);
    if (fboStatus != GL_FRAMEBUFFER_COMPLETE) {
        printf("[ColorMC Error] Failed to set up framebuffer: 0x%x\n", fboStatus);
    } else {
        printf("[ColorMC Info] gl framebuffer ok\n");
    }

    glBindFramebuffer_p(GL_FRAMEBUFFER, 0);
    fflush(stdout);
}

/*
 * 用于context首次初始化
 */
void gl_create(context_env * env) {
    printf("[ColorMC Info] gl create\n");
    fflush(stdout);
    gl_create_texture(env);
    gl_create_fbo(env);
    env->init = true;
    send_data(COMMAND_DISPLAY_READY);
}

/*
 * egl创建一个context
 */
bool egl_create_context(context_env * env) {
    // 创建EGL上下文
    EGLint contextAttribs[] = {
            EGL_CONTEXT_CLIENT_VERSION, gles_version,
            EGL_NONE
    };
    env->context = eglCreateContext_p(display, config, env->share, contextAttribs);

    if (env->context == EGL_NO_CONTEXT) {
        printf("[ColorMC Error] eglCreateContext_p() finished with error: %s", getEGLError());
        fflush(stdout);
        return false;
    }

    printf("[ColorMC Info] egl_create_context output: %p\n", env->context);
    fflush(stdout);

    return true;
}

/*
 * 切换context
 * 若context没有初始化过，则进行初始化
 */
bool egl_make_current(context_env* env) {
    if (env == NULL) {
        //进行context取消绑定
        printf("[ColorMC Info] unbind egl context\n");
        fflush(stdout);
        eglMakeCurrent_p(display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
        return true;
    }

    //进行context切换
    if (eglMakeCurrent_p(display, surface, surface, env->context) != EGL_TRUE) {
        printf("[ColorMC Error] eglMakeCurrent returned with error: %s\n", getEGLError());
        fflush(stdout);
        return false;
    } else {
        if(env->init == false) {
            //进行context初始化
            gl_create(env);
        }
        printf("[ColorMC Info] bind egl context to :%p\n", env);
        fflush(stdout);
        return true;
    }
}

/*
 * 销毁一个context
 */
void egl_destroy_context(context_env * env) {
    glBindFramebuffer_p(GL_FRAMEBUFFER, 0);
    glBindFramebuffer_p(GL_READ_FRAMEBUFFER, 0);
    glBindFramebuffer_p(GL_DRAW_FRAMEBUFFER, 0);
    glDeleteFramebuffers_p(1, &env->fbo);
    glDeleteTextures_p(1, &env->texture);
    eglDestroyContext_p(display, env->context);
}

/*
 * 改变渲染大小
 */
void egl_change_size() {
    printf("[ColorMC Info] egl reload\n");
    fflush(stdout);

    context_env *env = now_env;
    if (env == NULL) {
        return;
    }
    //删除fbo
    glDeleteFramebuffers_p(1, &env->fbo);
    //删除texture
    glDeleteTextures_p(1, &env->texture);
    //删除image
    if (eglImage != EGL_NO_IMAGE_KHR) {
        eglDestroyImageKHR_p(display, eglImage);
        eglImage = EGL_NO_IMAGE_KHR;
    }

    ah_delete_buffer();

    //删除surface
    egl_make_current(NULL);
    eglDestroySurface_p(display, surface);
    surface = EGL_NO_SURFACE;

    //创建surface
    egl_create_surface();
    egl_make_current(env->context);
    //创建buffer
    ah_create_buffer();
    //创建image
    egl_create_image();
    //创建texture
    gl_create_texture(env);
    //创建fbo
    gl_create_fbo(env);

    fflush(stdout);

    send_data(COMMAND_SET_SIZE);
}

//交换buffer
void egl_swap_buffers() {
    glBindFramebuffer_p(GL_READ_FRAMEBUFFER, 0);
    glBindFramebuffer_p(GL_DRAW_FRAMEBUFFER, now_env->fbo);
    glBlitFramebuffer_p(0, 0, width, height, 0, 0,
                        width, height, GL_COLOR_BUFFER_BIT, GL_NEAREST);

    glBindFramebuffer_p(GL_READ_FRAMEBUFFER, 0);
    glBindFramebuffer_p(GL_DRAW_FRAMEBUFFER, 0);

    if (render_state == RENDER_CHANGE_SIZE) {
        render_state = RENDER_RUN;
        send_screen_size(width, height);
        egl_change_size();
    }
}

/*
 * egl完整删除
 */
void egl_close() {
    eglMakeCurrent_p(display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
    eglDestroySurface_p(display, surface);
    eglTerminate_p(display);
    eglReleaseThread_p();
}

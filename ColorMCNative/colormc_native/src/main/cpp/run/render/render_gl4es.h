//
// Created by 40206 on 2024/5/23.
//

#ifndef COLORMCNATIVE_RENDER_GL4ES_H
#define COLORMCNATIVE_RENDER_GL4ES_H

void egl_swap_interval(int swapInterval);
void egl_create_surface();
void egl_create_image();
void* egl_create_context(context_env * env);
bool egl_create();
bool egl_make_current(context_env* context);
void egl_destroy_context(context_env* input);
void egl_swap_buffers();
void egl_close();

#endif //COLORMCNATIVE_RENDER_GL4ES_H

//
// Created by 40206 on 2024/5/23.
//

#ifndef COLORMCNATIVE_RENDER_ZINK_H
#define COLORMCNATIVE_RENDER_ZINK_H

#include <stdbool.h>
#include "../context_list.h"

bool mesa_create();
bool mesa_create_context(context_env * share);
bool mesa_make_current(context_env* window);
bool mesa_destroy_context(context_env* window);
void mesa_swap_buffers();
void mesa_swap_interval(int interval);
void mesa_close();

#endif //COLORMCNATIVE_RENDER_ZINK_H

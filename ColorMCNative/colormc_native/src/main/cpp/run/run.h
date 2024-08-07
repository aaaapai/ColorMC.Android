//
// Created by 40206 on 2024/1/11.
//

#ifndef COLORMCNATIVE_RUN_H
#define COLORMCNATIVE_RUN_H

#include <stdbool.h>
#include <android/hardware_buffer.h>

extern int width;
extern int height;
extern int gles_version;

extern uint8_t render_state;
extern uint8_t render_type;

extern AHardwareBuffer *a_buffer;
extern void* showingWindow;

void ah_create_buffer();
void ah_delete_buffer();
void ah_copy_data(void* data);

bool game_init();
void* game_create_context(void* context);
void* game_get_context();
void game_make_current(void* window);
void game_destroy_context(void* window);
void game_swap_buffers();
void game_swap_interval(int interval);
void game_close();

typedef enum RENDER_STATE {
    RENDER_RUN,
    RENDER_CHANGE_SIZE
} RENDER_STATE;

typedef enum RENDER_TYPE{
    GL4ES,
    ZINK
} RENDER_TYPE;

#endif //COLORMCNATIVE_RUN_H

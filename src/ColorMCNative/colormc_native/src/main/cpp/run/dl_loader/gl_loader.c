//
// Created by 40206 on 2024/5/23.
//

#include <stdlib.h>
#include <stdbool.h>
#include <dlfcn.h>

#include "gl_loader.h"

bool gl_dlopen() {
    char *gl = getenv("GL_SO");
    if (gl == NULL) {
        printf("[ColorMC Error] no GL_SO\n");
        return false;
    }
    void *libgl = dlopen(gl, RTLD_LAZY);
    if (!libgl) {
        printf("[ColorMC Error] Failed to load GL_SO\n");
        return false;
    }

    return true;
}
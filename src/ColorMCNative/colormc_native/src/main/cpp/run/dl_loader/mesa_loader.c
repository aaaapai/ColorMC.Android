#include <stdio.h>
#include <stdlib.h>
#include <dlfcn.h>
#include <stdbool.h>
#include "mesa_loader.h"
#include "../GL/osmesa.h"

GLboolean (*OSMesaMakeCurrent_p) (OSMesaContext ctx, void *buffer, GLenum type,
                                         GLsizei width, GLsizei height);
OSMesaContext (*OSMesaGetCurrentContext_p) (void);
OSMesaContext  (*OSMesaCreateContext_p) (GLenum format, OSMesaContext sharelist);
void (*OSMesaDestroyContext_p) (OSMesaContext ctx);
void (*OSMesaPixelStore_p) ( GLint pname, GLint value );
GLubyte* (*glGetString_p) (GLenum name);
void (*glFinish_p) (void);
void (*glClearColor_p) (GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);
void (*glClear_p) (GLbitfield mask);
void (*glReadPixels_p) (GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, void * data);

bool mesa_dlopen() {
    char *mesa_library = getenv("MESA_SO");
    if (mesa_library == NULL) {
        printf("[ColorMC Error] no MESA_SO\n");
        return false;
    }
    void *dl_handle = dlopen(mesa_library, RTLD_GLOBAL);
    if (!dl_handle) {
        printf("[ColorMC Error] Failed to load MESA_SO\n");
        return false;
    }
    OSMesaMakeCurrent_p = dlsym(dl_handle, "OSMesaMakeCurrent");
    OSMesaGetCurrentContext_p = dlsym(dl_handle, "OSMesaGetCurrentContext");
    OSMesaCreateContext_p = dlsym(dl_handle, "OSMesaCreateContext");
    OSMesaDestroyContext_p = dlsym(dl_handle, "OSMesaDestroyContext");
    OSMesaPixelStore_p = dlsym(dl_handle, "OSMesaPixelStore");
    glGetString_p = dlsym(dl_handle, "glGetString");
    glClearColor_p = dlsym(dl_handle, "glClearColor");
    glClear_p = dlsym(dl_handle, "glClear");
    glFinish_p = dlsym(dl_handle, "glFinish");
    glReadPixels_p = dlsym(dl_handle, "glReadPixels");

    return true;
}

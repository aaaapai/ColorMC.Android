//
// Created by 40206 on 2024/1/15.
//

#include <pthread.h>

#include "context_list.h"

#define ENV_COUNT 20

context_env env_list[ENV_COUNT] = {0};
context_env *now_env;

pthread_mutex_t mutex;

/*
 * context列表
 */
void context_list_init() {
    pthread_mutex_init(&mutex, NULL);
}

/*
 * 取列表中一个空的context
 */
context_env * context_find_empty() {
    pthread_mutex_lock(&mutex);
    for (int a = 0; a < ENV_COUNT; a++) {
        if (env_list[a].context == EGL_NO_CONTEXT) {
            pthread_mutex_unlock(&mutex);
            return &env_list[a];
        }
    }

    pthread_mutex_unlock(&mutex);
    return NULL;
}

/*
 * 找到指定的context
 */
context_env * context_find_match(context_env * env) {
    if(env == NULL) {
        return NULL;
    }
    pthread_mutex_lock(&mutex);
    for (int a = 0; a < ENV_COUNT; a++) {
        if (&env_list[a] == env || env_list[a].context == env) {
            pthread_mutex_unlock(&mutex);
            return &env_list[a];
        }
    }

    pthread_mutex_unlock(&mutex);
    return NULL;
}

/*
 * 删除context
 */
void context_remove(context_env* env) {
    pthread_mutex_lock(&mutex);
    for (int a = 0; a < ENV_COUNT; a++) {
        if (&env_list[a] == env || env_list[a].context == env) {
            env_list[a].context = NULL;
            env_list[a].fbo = 0;
            env_list[a].texture = 0;
            env_list[a].init = false;
            env_list[a].share = NULL;
        }
    }
    pthread_mutex_unlock(&mutex);
}
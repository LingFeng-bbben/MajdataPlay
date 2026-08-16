#include <pthread.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

int32_t pthread_threadid_np_(void* thread, uint64_t* threadId) {
    return (int32_t)pthread_threadid_np((pthread_t)thread, threadId);
}

#ifdef __cplusplus
}
#endif
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

typedef void (*AppStatusChangeCallback)(bool);

static AppStatusChangeCallback onForegroundChangeCallback = NULL;
static AppStatusChangeCallback onFocusChangeCallback = NULL;

@interface AppStatusObserver : NSObject
+ (instancetype)sharedInstance;
- (void)startObserving;
@end

@implementation AppStatusObserver

+ (instancetype)sharedInstance {
    static AppStatusObserver *instance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        instance = [[AppStatusObserver alloc] init];
    });
    return instance;
}

- (void)startObserving {
    static bool isObserving = false;
    if (isObserving) return;
    
    NSNotificationCenter *center = [NSNotificationCenter defaultCenter];
    
    [center addObserver:self selector:@selector(appDidBecomeActive) name:UIApplicationDidBecomeActiveNotification object:nil];
    [center addObserver:self selector:@selector(appWillResignActive) name:UIApplicationWillResignActiveNotification object:nil];
    
    [center addObserver:self selector:@selector(appWillEnterForeground) name:UIApplicationWillEnterForegroundNotification object:nil];
    [center addObserver:self selector:@selector(appDidEnterBackground) name:UIApplicationDidEnterBackgroundNotification object:nil];
    
    isObserving = true;
}

- (void)appDidBecomeActive {
    if (onFocusChangeCallback) onFocusChangeCallback(true);
}

- (void)appWillResignActive {
    if (onFocusChangeCallback) onFocusChangeCallback(false);
}

- (void)appWillEnterForeground {
    if (onForegroundChangeCallback) onForegroundChangeCallback(true);
}

- (void)appDidEnterBackground {
    if (onForegroundChangeCallback) onForegroundChangeCallback(false);
}

@end


extern "C" {

    void _RegisterAppStatusCallbacks(AppStatusChangeCallback foregroundCallback, AppStatusChangeCallback focusCallback) {
        onForegroundChangeCallback = foregroundCallback;
        onFocusChangeCallback = focusCallback;
        
        // 注册监听器
        [[AppStatusObserver sharedInstance] startObserving];
    }

    bool _IsAppInForeground() {
        __block bool isForeground = false;
        void (^block)(void) = ^{
            UIApplicationState state = [[UIApplication sharedApplication] applicationState];
            isForeground = (state != UIApplicationStateBackground);
        };

        if ([NSThread isMainThread]) block();
        else dispatch_sync(dispatch_get_main_queue(), block);
        return isForeground;
    }

    bool _IsAppFocused() {
        __block bool isFocused = false;
        void (^block)(void) = ^{
            UIApplicationState state = [[UIApplication sharedApplication] applicationState];
            isFocused = (state == UIApplicationStateActive);
        };

        if ([NSThread isMainThread]) block();
        else dispatch_sync(dispatch_get_main_queue(), block);
        return isFocused;
    }
}
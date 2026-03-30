#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "UnityAppController.h"
#import "UnityInterface.h"

static void SendToUnity(const char* method, const char* msg) {
    UnitySendMessage("ZipImporter", method, msg);
}
typedef void (*OnFileOpenCallback)(const char* tempFilePath);
static OnFileOpenCallback g_onFileOpenCallback = NULL;

extern "C" 
{
    void RegisterOnFileOpenCallback(OnFileOpenCallback callback)
    {
        g_onFileOpenCallback = callback;
    }

    void UnregisterOnFileOpenCallback()
    {
        g_onFileOpenCallback = NULL;
    }
}

@interface OpenFileBridge : UnityAppController
@end

@implementation OpenFileBridge

- (BOOL)application:(UIApplication*)app
            openURL:(NSURL*)url
            options:(NSDictionary<UIApplicationOpenURLOptionsKey,id>*)options
{
    if (!url) return [super application:app openURL:url options:options];

    NSString* ext = url.pathExtension.lowercaseString ?: @"";
    if (!([ext isEqualToString:@"zip"] || [ext isEqualToString:@"adx"])) {
        return [super application:app openURL:url options:options];
    }

    BOOL needStop = NO;
    if (url.isFileURL) {
        if ([url startAccessingSecurityScopedResource]) {
            needStop = YES;
        }
    }

    
    NSString* tempDir = [NSTemporaryDirectory() stringByAppendingPathComponent:@"maicharts_inbox"];
    [[NSFileManager defaultManager] createDirectoryAtPath:tempDir
                              withIntermediateDirectories:YES
                                               attributes:nil
                                                    error:nil];

    NSString* fileName = url.lastPathComponent;
    NSString* destPath = [tempDir stringByAppendingPathComponent:fileName];

    NSError* err = nil;
    [[NSFileManager defaultManager] removeItemAtPath:destPath error:nil];

    if (![[NSFileManager defaultManager] copyItemAtURL:url
                                                 toURL:[NSURL fileURLWithPath:destPath]
                                                 error:&err]) {
        NSLog(@"[OpenFileBridge] copy failed: %@", err);
        if (needStop) [url stopAccessingSecurityScopedResource];
        return YES;
    }

    if (needStop) 
    {
        [url stopAccessingSecurityScopedResource];
    }

    if (g_onFileOpenCallback != NULL)
    {
        g_onFileOpenCallback(destPath.UTF8String);
    }
    else
    {
        return NO;
    }
    return YES;
}

@end

IMPL_APP_CONTROLLER_SUBCLASS(OpenFileBridge)
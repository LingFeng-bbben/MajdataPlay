#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "UnityAppController.h"
#import "UnityInterface.h"

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
    UnitySendMessage("GameManager", "IOS_OnFileOpen", destPath.UTF8String);
    return YES;
}

@end

IMPL_APP_CONTROLLER_SUBCLASS(OpenFileBridge)
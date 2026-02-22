#import <Foundation/Foundation.h>

extern "C" {

bool _GetBoolSetting(const char* key, bool defaultValue) {
    NSString* nsKey = [NSString stringWithUTF8String:key];
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];

    id obj = [defaults objectForKey:nsKey];
    if (obj == nil) {
        return defaultValue;
    }

    return [defaults boolForKey:nsKey];
}

int _GetIntSetting(const char* key, int defaultValue) {
    NSString* nsKey = [NSString stringWithUTF8String:key];
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];

    id obj = [defaults objectForKey:nsKey];
    if (obj == nil) {
        return defaultValue;
    }

    return (int)[defaults integerForKey:nsKey];
}

const char* _GetStringSetting(const char* key, const char* defaultValue) {
    NSString* nsKey = [NSString stringWithUTF8String:key];
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];

    id obj = [defaults objectForKey:nsKey];
    if (obj == nil) {
        NSString* fallback = [NSString stringWithUTF8String:defaultValue];
        return strdup([fallback UTF8String]);
    }

    NSString* value = [defaults stringForKey:nsKey];
    if (value == nil) value = @"";

    return strdup([value UTF8String]);
}

}
#import <Foundation/Foundation.h>

extern "C" {

bool _GetBoolSetting(const char* key, bool defaultValue) {
    NSString* nsKey = [NSString stringWithUTF8String:key];
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];

    id obj = [defaults objectForKey:nsKey];
    if (obj == nil) {
        NSLog(@"[MajSettings] BOOL '%@' NOT FOUND -> return default=%@", nsKey, defaultValue ? @"YES" : @"NO");
        return defaultValue;
    }

    NSLog(@"[MajSettings] BOOL '%@' FOUND raw=%@ type=%@", nsKey, obj, NSStringFromClass([obj class]));

    bool result = [defaults boolForKey:nsKey];
    NSLog(@"[MajSettings] BOOL '%@' -> result=%@", nsKey, result ? @"YES" : @"NO");
    return result;
}

int _GetIntSetting(const char* key, int defaultValue) {
    NSString* nsKey = [NSString stringWithUTF8String:key];
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];

    id obj = [defaults objectForKey:nsKey];
    if (obj == nil) {
        NSLog(@"[MajSettings] INT '%@' NOT FOUND -> return default=%d", nsKey, defaultValue);
        return defaultValue;
    }

    NSLog(@"[MajSettings] INT '%@' FOUND raw=%@ type=%@", nsKey, obj, NSStringFromClass([obj class]));

    int result = (int)[defaults integerForKey:nsKey];
    NSLog(@"[MajSettings] INT '%@' -> result=%d", nsKey, result);
    return result;
}

const char* _GetStringSetting(const char* key, const char* defaultValue) {
    NSString* nsKey = [NSString stringWithUTF8String:key];
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];

    id obj = [defaults objectForKey:nsKey];
    if (obj == nil) {
        NSLog(@"[MajSettings] STRING '%@' NOT FOUND -> return default=%s", nsKey, defaultValue);
        NSString* fallback = [NSString stringWithUTF8String:defaultValue];
        return strdup([fallback UTF8String]);
    }

    NSLog(@"[MajSettings] STRING '%@' FOUND raw=%@ type=%@", nsKey, obj, NSStringFromClass([obj class]));

    NSString* value = [defaults stringForKey:nsKey];
    if (value == nil) value = @"";
    NSLog(@"[MajSettings] STRING '%@' -> result='%@'", nsKey, value);

    return strdup([value UTF8String]);
}

void _DumpEnableOnline() {
    NSString* key = @"enabled_online";
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];

    id obj = [defaults objectForKey:key];
    if (obj == nil) {
        NSLog(@"[MajSettings] DUMP '%@' NOT FOUND in UserDefaults", key);
        return;
    }

    NSLog(@"[MajSettings] DUMP '%@' raw=%@ type=%@", key, obj, NSStringFromClass([obj class]));
    BOOL b = [defaults boolForKey:key];
    NSLog(@"[MajSettings] DUMP '%@' boolForKey=%@", key, b ? @"YES" : @"NO");
}

}
/*
	BASS recording example
	Copyright (c) 2002-2021 Un4seen Developments Ltd.
*/

#import "AppDelegate.h"
#include "bass.h"

@interface AppDelegate ()

@end

@implementation AppDelegate

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
}

- (void)applicationWillTerminate:(NSNotification *)aNotification {
	BASS_RecordFree();
	BASS_Free();
}

@end

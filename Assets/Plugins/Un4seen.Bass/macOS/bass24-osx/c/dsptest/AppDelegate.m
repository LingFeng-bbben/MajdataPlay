/*
	BASS DSP example
	Copyright (c) 2000-2021 Un4seen Developments Ltd.
*/

#import "AppDelegate.h"
#include "bass.h"

@interface AppDelegate ()

@end

@implementation AppDelegate

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
}

- (void)applicationWillTerminate:(NSNotification *)aNotification {
	BASS_Free();
}

@end

/*
	BASS multi-speaker example
	Copyright (c) 2003-2021 Un4seen Developments Ltd.
*/

#import "ViewController.h"
#include "bass.h"

@implementation ViewController

HSTREAM chan[4];	// channel handles

// display error messages
void Error(NSString *es)
{
	es = [es stringByAppendingFormat:@"\n(error code: %d)",BASS_ErrorGetCode()];
	if (![NSThread isMainThread]) {
		dispatch_async(dispatch_get_main_queue(), ^{
			NSAlert *alert=[[NSAlert alloc] init];
			[alert setMessageText:es];
			[alert runModal];
		});
	} else {
		NSAlert *alert=[[NSAlert alloc] init];
		[alert setMessageText:es];
		[alert runModal];
	}
}

- (IBAction)openFile:(id)sender {
	int speaker=(int)[sender tag]-10;
	NSOpenPanel *panel = [NSOpenPanel openPanel];
	if ([panel runModal] == NSModalResponseOK) {
		BASS_ChannelFree(chan[speaker]); // free the old channel
		NSString *file = [panel filename];
		if (!(chan[speaker] = BASS_StreamCreateFile(0, [file UTF8String], 0, 0, BASS_SPEAKER_N(speaker + 1) | BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT))
			&& !(chan[speaker] = BASS_MusicLoad(0, [file UTF8String], 0, 0, BASS_SPEAKER_N(speaker + 1) | BASS_MUSIC_RAMPS | BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT, 1))) {
			[sender setTitle:@"Open file..."];
			Error(@"Can't play the file");
		} else {
			[sender setTitle:[file lastPathComponent]];
			BASS_ChannelPlay(chan[speaker], FALSE);
		}
	}
}

- (IBAction)swapSpeakers:(id)sender {
	int speaker=(int)[sender tag];
	// swap handles
	HSTREAM tempchan = chan[speaker];
	chan[speaker] = chan[speaker + 1];
	chan[speaker + 1] = tempchan;
	// swap text
	NSButton *b1 = (NSButton*)[[self view] viewWithTag:speaker+10];
	NSButton *b2 = (NSButton*)[[self view] viewWithTag:speaker+11];
	NSString *temptext = b1.title;
	[b1 setTitle:b2.title];
	[b2 setTitle:temptext];
	// update speaker flags
	BASS_ChannelFlags(chan[speaker], BASS_SPEAKER_N(speaker + 1), BASS_SPEAKER_FRONT);
	BASS_ChannelFlags(chan[speaker + 1], BASS_SPEAKER_N(speaker + 2), BASS_SPEAKER_FRONT);
}

- (void)viewDidLoad {
	[super viewDidLoad];

	// check the correct BASS was loaded
	if (HIWORD(BASS_GetVersion()) != BASSVERSION) {
		Error(@"An incorrect version of BASS was loaded");
		exit(0);
	}

	// initialize default output device
	if (!BASS_Init(-1, 44100, 0, NULL, NULL)) {
		Error(@"Can't initialize device");
		exit(0);
	}

	// check how many speakers the device supports
	BASS_INFO i;
	BASS_GetInfo(&i);
	if (i.speakers < 8) {
		[self.open4Button setEnabled:false];
		[self.swap34Button setEnabled:false];
	}
	if (i.speakers < 6) {
		[self.open3Button setEnabled:false];
		[self.swap23Button setEnabled:false];
	}
	if (i.speakers < 4) {
		[self.open2Button setEnabled:false];
		[self.swap12Button setEnabled:false];
	}
}

@end

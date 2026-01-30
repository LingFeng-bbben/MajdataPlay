/*
	BASSmix multiple output example
	Copyright (c) 2009-2021 Un4seen Developments Ltd.
*/

#import "ViewController.h"
#include "bass.h"
#include "bassmix.h"

@implementation ViewController

DWORD outdev[2] = { 1, 0 };	// output devices
DWORD source;		// source channel
HSTREAM split[2];	// output splitter streams

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

- (BOOL)CreateClone {
	// set the device to create 2nd splitter stream on, and then create it
	BASS_SetDevice(outdev[1]);
	if (!(split[1] = BASS_Split_StreamCreate(source, 0, NULL))) {
		Error(@"Can't create splitter");
		return FALSE;
	}
	BASS_ChannelSetLink(split[0], split[1]); // link the splitters to start together
	return TRUE;
}

- (IBAction)changeDevice:(id)sender {
	// device selection changed
	int sel = (int)[sender indexOfSelectedItem]; // get the selection
	int devn = (int)[sender tag];
	if (outdev[devn] == sel) return;
	if (!BASS_Init(sel, 44100, 0, NULL, NULL)) { // initialize new device
		Error(@"Can't initialize device");
		[sender selectItemAtIndex:outdev[devn]];
	} else {
		if (split[devn]) BASS_ChannelSetDevice(split[devn], sel); // move channel to new device
		BASS_SetDevice(outdev[devn]); // set context to old device
		BASS_Free(); // free it
		outdev[devn] = sel;
	}
}

- (IBAction)openFile:(id)sender {
	NSOpenPanel *panel=[NSOpenPanel openPanel];
	if ([panel runModal] == NSModalResponseOK) {
		BASS_ChannelFree(source); // free old channel (splitters automatically freed too)
		NSString *file=[panel filename];
		if (!(source = BASS_StreamCreateFile(0, [file UTF8String], 0, 0, BASS_STREAM_DECODE | BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT))
			&& !(source = BASS_MusicLoad(0, [file UTF8String], 0, 0, BASS_MUSIC_DECODE | BASS_MUSIC_PRESCAN | BASS_MUSIC_POSRESET | BASS_MUSIC_RAMPS | BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT, 1))) {
			[sender setTitle:@"Open file..."];
			Error(@"Can't play the file");
			return;
		}
		// disassociate source from any device so it isn't freed when changing device
		BASS_ChannelSetDevice(source, BASS_NODEVICE);
		// set the device to create 1st splitter stream on, and then create it
		BASS_SetDevice(outdev[0]);
		if (!(split[0] = BASS_Split_StreamCreate(source, 0, NULL))) {
			BASS_ChannelFree(source);
			source = 0;
			[sender setTitle:@"Open file..."];
			Error(@"Can't create splitter");
		}
		if (self.cloneSwitch.state)
			[self CreateClone]; // create a clone
		else
			split[1] = 0; // no clone
		[sender setTitle:[file lastPathComponent]];
		// update scroller range
		QWORD len = BASS_ChannelGetLength(source, BASS_POS_BYTE);
		if (len == -1) len = 0; // unknown length
		self.positionSlider.maxValue=BASS_ChannelBytes2Seconds(source, len);
		BASS_ChannelPlay(split[0], FALSE); // start playback
	}
}

- (IBAction)switchClone:(id)sender {
	if (!split[0]) return;
	if ([sender state]) { // create clone on device #2
		if (!split[1] && [self CreateClone]) {
			int offset;
			BASS_INFO info;
			BASS_GetInfo(&info);
			offset = BASS_Split_StreamGetAvailable(split[0]) // get the amount of data the 1st splitter has buffered
				+ BASS_ChannelGetData(split[0], NULL, BASS_DATA_AVAILABLE) // add the amount in its playback buffer
				- BASS_ChannelSeconds2Bytes(split[0], info.latency / 1000.0); // subtract the device's playback delay
			if (offset < 0) offset = 0; // just in case
			BASS_Split_StreamResetEx(split[1], offset); // set the new splitter that far back in the source buffer
			BASS_ChannelPlay(split[1], FALSE); // start the clone
		}
	} else { // remove clone on device #2
		BASS_ChannelFree(split[1]);
		split[1] = 0;
	}
}

- (IBAction)changePosition:(id)sender {
	BASS_ChannelPause(split[0]); // pause splitter streams (so that resumption following seek can be synchronized)
	BASS_ChannelSetPosition(source, BASS_ChannelSeconds2Bytes(source, [sender doubleValue]), BASS_POS_BYTE); // set source position
	BASS_Split_StreamReset(source); // reset buffers of all (both) the source's splitters
	BASS_ChannelPlay(split[0], FALSE); // resume playback
}

- (void)TimerProc:(NSTimer*)timer {
	if (source)
		self.positionSlider.doubleValue = BASS_ChannelBytes2Seconds(split[0], BASS_ChannelGetPosition(split[0], BASS_POS_BYTE)); // update position (using 1st splitter)
}

- (void)viewDidLoad {
	[super viewDidLoad];

	// check the correct BASS was loaded
	if (HIWORD(BASS_GetVersion()) != BASSVERSION) {
		Error(@"An incorrect version of BASS was loaded");
		exit(0);
	}

	{ // get list of output devices
		int c;
		BASS_DEVICEINFO di;
		for (c = 0; BASS_GetDeviceInfo(c, &di); c++) {
			[self.device1Selector addItemWithTitle:[NSString stringWithUTF8String:di.name]];
			if (c == outdev[0]) [self.device1Selector selectItemAtIndex:c];;
			[self.device2Selector addItemWithTitle:[NSString stringWithUTF8String:di.name]];
			if (c == outdev[1]) [self.device2Selector selectItemAtIndex:c];;
		}
	}
	// initialize the output devices
	if (!BASS_Init(outdev[0], 44100, 0, NULL, NULL) || !BASS_Init(outdev[1], 44100, 0, NULL, NULL)) {
		Error(@"Can't initialize device");
		exit(0);
	}
	
	// timer to update the position display
	[[NSRunLoop currentRunLoop] addTimer:[NSTimer timerWithTimeInterval:0.1 target:self selector:@selector(TimerProc:) userInfo:nil repeats:YES] forMode:NSRunLoopCommonModes];
}

@end

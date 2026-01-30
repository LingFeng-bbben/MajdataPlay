/*
	BASS multiple output example
	Copyright (c) 2001-2021 Un4seen Developments Ltd.
*/

#import "ViewController.h"
#include "bass.h"

@implementation ViewController

DWORD outdev[2] = { 1, 0 };	// output devices
DWORD chan[2];		// channel handles

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

// Cloning DSP function
void CALLBACK CloneDSP(HDSP handle, DWORD channel, void *buffer, DWORD length, void *user)
{
	BASS_StreamPutData((HSTREAM)user, buffer, length); // user = clone
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
		if (chan[devn]) BASS_ChannelSetDevice(chan[devn], sel); // move channel to new device
		BASS_SetDevice(outdev[devn]); // set context to old device
		BASS_Free(); // free it
		outdev[devn] = sel;
	}
}

- (IBAction)openFile:(id)sender {
	int devn = (int)[sender tag];
	NSOpenPanel *panel = [NSOpenPanel openPanel];
	if ([panel runModal] == NSModalResponseOK) {
		BASS_ChannelFree(chan[devn]); // free the old channel
		BASS_SetDevice(outdev[devn]); // set the device to create new channel on
		NSString *file = [panel filename];
		if (!(chan[devn] = BASS_StreamCreateFile(0, [file UTF8String], 0, 0, BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT))
			&& !(chan[devn] = BASS_MusicLoad(0, [file UTF8String], 0, 0, BASS_MUSIC_RAMPS | BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT, 1))) {
			[sender setTitle:@"Open file..."];
			Error(@"Can't play the file");
		} else {
			[sender setTitle:[file lastPathComponent]];
			BASS_ChannelPlay(chan[devn], FALSE);
		}
	}
}

- (IBAction)cloneChannel:(id)sender {
	int devn=(int)[sender tag];
	BASS_CHANNELINFO chaninfo;
	if (!BASS_ChannelGetInfo(chan[devn ^ 1], &chaninfo)) {
		Error(@"Nothing to clone");
		return;
	}
	BASS_ChannelFree(chan[devn]); // free the old channel
	BASS_SetDevice(outdev[devn]); // set the device to create clone on
	if (!(chan[devn] = BASS_StreamCreate(chaninfo.freq, chaninfo.chans, chaninfo.flags, STREAMPROC_PUSH, 0))) { // create a "push" stream
		if (!devn)
			[self.open1Button setTitle:@"Open file..."];
		else
			[self.open2Button setTitle:@"Open file..."];
		Error(@"Can't create clone");
	} else {
		BASS_INFO info;
		BASS_GetInfo(&info); // get latency info
		BASS_ChannelLock(chan[devn ^ 1], TRUE); // lock source stream to synchonise buffer contents
		BASS_ChannelSetDSP(chan[devn ^ 1], CloneDSP, (void*)(intptr_t)chan[devn], 0); // set DSP to feed data to clone
		{ // copy buffered data to clone
			DWORD d = BASS_ChannelSeconds2Bytes(chan[devn], info.latency / 1000.0); // playback delay
			DWORD c = BASS_ChannelGetData(chan[devn ^ 1], 0, BASS_DATA_AVAILABLE);
			BYTE *buf = (BYTE*)malloc(c);
			c = BASS_ChannelGetData(chan[devn ^ 1], buf, c);
			if (c > d) BASS_StreamPutData(chan[devn], buf + d, c - d);
			free(buf);
		}
		BASS_ChannelLock(chan[devn ^ 1], FALSE); // unlock source stream
		BASS_ChannelPlay(chan[devn], FALSE); // play clone
		if (!devn)
			[self.open1Button setTitle:@"clone"];
		else
			[self.open2Button setTitle:@"clone"];
	}
}

- (IBAction)swapChannels:(id)sender {
	// swap handles
	HSTREAM tempchan = chan[0];
	chan[0] = chan[1];
	chan[1] = tempchan;
	// swap text
	NSString *temptext = self.open1Button.title;
	[self.open1Button setTitle:self.open2Button.title];
	[self.open2Button setTitle:temptext];
	// update the channel devices
	BASS_ChannelSetDevice(chan[0], outdev[0]);
	BASS_ChannelSetDevice(chan[1], outdev[1]);
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
}

@end

/*
	BASS effects example
	Copyright (c) 2001-2021 Un4seen Developments Ltd.
*/

#import "ViewController.h"
#include "bass.h"

@implementation ViewController

DWORD chan;			// channel handle
DWORD fxchan;		// output stream handle
DWORD fxchansync;	// output stream FREE sync
HFX fx[4];			// 3 eq bands + reverb
float fxval[5];		// effect settings

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

void UpdateFX(int b)
{
	if (b < 3) { // EQ
		BASS_DX8_PARAMEQ p;
		BASS_FXGetParameters(fx[b], &p);
		p.fGain = fxval[b];
		BASS_FXSetParameters(fx[b], &p);
	} else if (b == 3) { // reverb
		BASS_DX8_REVERB p;
		BASS_FXGetParameters(fx[3], &p);
		p.fReverbMix = (fxval[b] > 0 ? log(fxval[b]) * 20 : -96);
		BASS_FXSetParameters(fx[3], &p);
	} else // volume
		BASS_ChannelSetAttribute(chan, BASS_ATTRIB_VOL, fxval[b]);
}

void SetupFX()
{
	// setup the effects
	BASS_DX8_PARAMEQ p;
	DWORD ch = fxchan ? fxchan : chan; // set on output stream if enabled, else file stream
	fx[0] = BASS_ChannelSetFX(ch, BASS_FX_DX8_PARAMEQ, 0);
	fx[1] = BASS_ChannelSetFX(ch, BASS_FX_DX8_PARAMEQ, 0);
	fx[2] = BASS_ChannelSetFX(ch, BASS_FX_DX8_PARAMEQ, 0);
	fx[3] = BASS_ChannelSetFX(ch, BASS_FX_DX8_REVERB, 0);
	p.fGain = 0;
	p.fBandwidth = 18;
	p.fCenter = 125;
	BASS_FXSetParameters(fx[0], &p);
	p.fCenter = 1000;
	BASS_FXSetParameters(fx[1], &p);
	p.fCenter = 8000;
	BASS_FXSetParameters(fx[2], &p);
	UpdateFX(0);
	UpdateFX(1);
	UpdateFX(2);
	UpdateFX(3);
}

void CALLBACK DeviceFreeSync(HSYNC handle, DWORD channel, DWORD data, void *user)
{
	// the device output stream has been freed due to format change, get a new one with new format
	if (!fxchan) return;
	fxchan = BASS_StreamCreate(0, 0, 0, STREAMPROC_DEVICE, 0);
	fxchansync = BASS_ChannelSetSync(fxchan, BASS_SYNC_FREE, 0, DeviceFreeSync, 0);
	SetupFX();
}

- (IBAction)openFile:(id)sender {
	NSOpenPanel *panel = [NSOpenPanel openPanel];
	if ([panel runModal] == NSModalResponseOK) {
		BASS_ChannelFree(chan); // free the old channel
		NSString *file = [panel filename];
		if (!(chan = BASS_StreamCreateFile(0, [file UTF8String], 0, 0, BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT))
			&& !(chan = BASS_MusicLoad(0, [file UTF8String], 0, 0, BASS_MUSIC_RAMPS | BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT, 1))) {
			[sender setTitle:@"Open file..."];
			Error(@"Can't play the file");
		} else {
			[sender setTitle:[file lastPathComponent]];
			if (!fxchan) SetupFX(); // set effects on file if not using output stream
			UpdateFX(4); // set volume
			BASS_ChannelPlay(chan, FALSE);
		}
	}
}

- (IBAction)adjustEffect:(id)sender {
	int b=(int)[sender tag] - 1;
	fxval[b]=[sender doubleValue];
	UpdateFX(b);
}

- (IBAction)changeOutput:(id)sender {
	// remove current effects
	DWORD ch = fxchan ? fxchan : chan;
	BASS_ChannelRemoveFX(ch, fx[0]);
	BASS_ChannelRemoveFX(ch, fx[1]);
	BASS_ChannelRemoveFX(ch, fx[2]);
	BASS_ChannelRemoveFX(ch, fx[3]);
	if ([sender state]) {
		fxchan = BASS_StreamCreate(0, 0, 0, STREAMPROC_DEVICE, 0); // get device output stream
		fxchansync = BASS_ChannelSetSync(fxchan, BASS_SYNC_FREE, 0, DeviceFreeSync, 0); // sync when device output stream is freed (format change)
	} else {
		BASS_ChannelRemoveSync(fxchan, fxchansync); // remove sync from device output stream
		fxchan = 0; // stop using device output stream
	}
	SetupFX();
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

	fxval[4] = 1; // default volume
}

@end

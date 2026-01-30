/*
	BASS simple synth
	Copyright (c) 2001-2021 Un4seen Developments Ltd.
*/

#import "ViewController.h"
#include "bass.h"

@implementation ViewController

BASS_INFO info;
HSTREAM stream; // the stream

const DWORD fxtype[5] = { BASS_FX_DX8_REVERB, BASS_FX_DX8_ECHO, BASS_FX_DX8_CHORUS, BASS_FX_DX8_FLANGER, BASS_FX_DX8_DISTORTION };
HFX fx[5];        	// effect handles

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

#define KEYS 20
const CGKeyCode keys[KEYS]={
	'Q','2','W','3','E','R','5','T','6','Y','7','U',
	'I','9','O','0','P','[','=',']'
};

#define MAXVOL 0.22
#define DECAY (MAXVOL/4000)
float vol[KEYS], pos[KEYS]; // key volume and position/phase

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

DWORD CALLBACK StreamProc(HSTREAM handle, float *buffer, DWORD length, void *user)
{
	int k, c;
	float omega;
	memset(buffer, 0, length);
	for (k = 0; k < KEYS; k++) {
		if (!vol[k]) continue;
		omega = 2 * M_PI * pow(2.0, (k + 3) / 12.0) * 440.0 / info.freq;
		for (c = 0; c < length / sizeof(float); c += 2) {
			buffer[c] += sin(pos[k]) * vol[k];
			buffer[c + 1] = buffer[c]; // left and right channels are the same
			pos[k] += omega;
			if (vol[k] < MAXVOL) {
				vol[k] -= DECAY;
				if (vol[k] <= 0) { // faded-out
					vol[k] = 0;
					break;
				}
			}
		}
		pos[k] = fmod(pos[k], 2 * M_PI);
	}
	return length;
}

- (IBAction)changeEffects:(id)sender {
	// toggle effects
	int n=(int)[sender tag];
	if (fx[n]) {
		BASS_ChannelRemoveFX(stream,fx[n]);
		fx[n]=0;
	} else
		fx[n]=BASS_ChannelSetFX(stream,fxtype[n],n);
}

- (void)viewDidLoad {
	[super viewDidLoad];
	
	// check the correct BASS was loaded
	if (HIWORD(BASS_GetVersion())!=BASSVERSION) {
		Error(@"An incorrect version of BASS was loaded");
		exit(0);
	}
	
	// initialize default output device
	if (!BASS_Init(-1,44100,0,NULL,NULL)) {
		Error(@"Can't initialize device");
		exit(0);
	}

	BASS_GetInfo(&info);
	stream = BASS_StreamCreate(info.freq, 2, BASS_SAMPLE_FLOAT, (STREAMPROC*)StreamProc, 0); // create a stream (stereo for effects)
	BASS_ChannelSetAttribute(stream, BASS_ATTRIB_BUFFER, 0); // no buffering for minimum latency
	BASS_ChannelPlay(stream, FALSE); // start it
	
	[NSEvent addLocalMonitorForEventsMatchingMask:NSEventMaskKeyDown|NSEventMaskKeyUp handler:^NSEvent *(NSEvent *event) {
		if ([event modifierFlags] & NSEventModifierFlagCommand) return event;
		if (![event isARepeat]) {
			int ch=toupper([[event characters] characterAtIndex:0]);
			int key;
			for (key = 0; key < KEYS; key++) {
				if (ch==keys[key]) {
					bool down = [event type] == NSEventTypeKeyDown;
					if (down && vol[key] < MAXVOL) {
						pos[key] = 0;
						vol[key] = MAXVOL + DECAY / 2; // start key (setting "vol" slightly higher than MAXVOL to cover any rounding-down)
					} else if (!down && vol[key])
						vol[key] -= DECAY; // trigger key fadeout
					break;
				}
			}
		}
		return nil; // disable the beeps!
	}];
}

@end

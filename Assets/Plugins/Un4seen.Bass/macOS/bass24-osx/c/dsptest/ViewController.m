/*
	BASS DSP example
	Copyright (c) 2000-2021 Un4seen Developments Ltd.
*/

#import "ViewController.h"
#include "bass.h"

@implementation ViewController

DWORD chan;			// channel handle

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

// "rotate"
HDSP rotdsp;	// DSP handle
float rotpos;	// cur.pos
void CALLBACK Rotate(HDSP handle, DWORD channel, void *buffer, DWORD length, void *user)
{
	float *d = (float*)buffer;
	DWORD a;

	for (a = 0; a < length / 4; a += 2) {
		d[a] *= fabs(sin(rotpos));
		d[a + 1] *= fabs(cos(rotpos));
		rotpos += 0.00003;
	}
	rotpos = fmod(rotpos, 2 * M_PI);
}

// "echo"
HDSP echdsp;	// DSP handle
#define ECHBUFLEN 1200	// buffer length
float echbuf[ECHBUFLEN][2];	// buffer
int echpos;	// cur.pos
void CALLBACK Echo(HDSP handle, DWORD channel, void *buffer, DWORD length, void *user)
{
	float *d = (float*)buffer;
	DWORD a;

	for (a = 0; a < length / 4; a += 2) {
		float l = d[a] + (echbuf[echpos][1] / 2);
		float r = d[a + 1] + (echbuf[echpos][0] / 2);
#if 1 // 0=echo, 1=basic "bathroom" reverb
		echbuf[echpos][0] = d[a] = l;
		echbuf[echpos][1] = d[a + 1] = r;
#else
		echbuf[echpos][0] = d[a];
		echbuf[echpos][1] = d[a + 1];
		d[a] = l;
		d[a + 1] = r;
#endif
		echpos++;
		if (echpos == ECHBUFLEN) echpos = 0;
	}
}

// "flanger"
HDSP fladsp;	// DSP handle
#define FLABUFLEN 350	// buffer length
float flabuf[FLABUFLEN][2];	// buffer
int flapos;	// cur.pos
float flas, flasinc;	// sweep pos/increment
void CALLBACK Flange(HDSP handle, DWORD channel, void *buffer, DWORD length, void *user)
{
	float *d = (float*)buffer;
	DWORD a;

	for (a = 0; a < length / 4; a += 2) {
		int p1 = (flapos + (int)flas) % FLABUFLEN;
		int p2 = (p1 + 1) % FLABUFLEN;
		float f = flas - (int)flas;
		float s;

		s = (d[a] + ((flabuf[p1][0] * (1 - f)) + (flabuf[p2][0] * f))) * 0.7;
		flabuf[flapos][0] = d[a];
		d[a] = s;

		s = (d[a + 1] + ((flabuf[p1][1] * (1 - f)) + (flabuf[p2][1] * f))) * 0.7;
		flabuf[flapos][1] = d[a + 1];
		d[a + 1] = s;

		flapos++;
		if (flapos == FLABUFLEN) flapos = 0;
		flas += flasinc;
		if (flas < 0 || flas > FLABUFLEN - 1) {
			flasinc = -flasinc;
			flas += flasinc;
		}
	}
}

- (IBAction)enableRotate:(id)sender {
	if ([sender state]) {
		rotpos = M_PI / 4;
		rotdsp = BASS_ChannelSetDSP(chan, &Rotate, 0, 2);
	} else
		BASS_ChannelRemoveDSP(chan, rotdsp);
}

- (IBAction)enableEcho:(id)sender {
	if ([sender state]) {
		memset(echbuf, 0, sizeof(echbuf));
		echpos = 0;
		echdsp = BASS_ChannelSetDSP(chan, &Echo, 0, 1);
	} else
		BASS_ChannelRemoveDSP(chan, echdsp);
}

- (IBAction)enableFlanger:(id)sender {
	if ([sender state]) {
		memset(flabuf, 0, sizeof(flabuf));
		flapos = 0;
		flas = FLABUFLEN / 2;
		flasinc = 0.002f;
		fladsp = BASS_ChannelSetDSP(chan, &Flange, 0, 0);
	} else
		BASS_ChannelRemoveDSP(chan, fladsp);
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
			BASS_CHANNELINFO info;
			BASS_ChannelGetInfo(chan, &info);
			if (info.chans != 2) { // the DSP expects stereo
				[sender setTitle:@"Open file..."];
				BASS_ChannelFree(chan);
				Error(@"only stereo sources are supported");
			} else {
				[sender setTitle:[file lastPathComponent]];
				// setup DSPs on new channel
				[self enableRotate:self.rotateSwitch];
				[self enableEcho:self.echoSwitch];
				[self enableFlanger:self.flangerSwitch];
				BASS_ChannelPlay(chan, FALSE);
			}
		}
	}
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
	
	// enable floating-point DSP (not really necessary as the channels will be floating-point anyway)
	BASS_SetConfig(BASS_CONFIG_FLOATDSP, TRUE);
}

@end

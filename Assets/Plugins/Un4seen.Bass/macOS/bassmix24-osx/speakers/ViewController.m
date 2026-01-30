/*
	BASSmix multi-speaker example
	Copyright (c) 2009-2021 Un4seen Developments Ltd.
*/

#import "ViewController.h"
#include "bass.h"
#include "bassmix.h"

@implementation ViewController

HSTREAM mixer, source; // mixer and source channels

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

- (void)setMatrix:(BOOL)ramp {
	float *matrix;
	BASS_CHANNELINFO mi, si;
	BASS_ChannelGetInfo(mixer, &mi); // get mixer info for channel count
	BASS_ChannelGetInfo(source, &si); // get source info for channel count
	matrix = (float*)malloc(mi.chans * si.chans * sizeof(float)); // allocate matrix (mixer channel count * source channel count)
	memset(matrix, 0, mi.chans * si.chans * sizeof(float)); // initialize it to empty/silence
/*
	set the mixing matrix depending on the speaker switches
	mono & stereo sources are duplicated on each enabled pair of speakers
*/
	if (self.speaker1Switch.state) { // 1st pair of speakers enabled
		matrix[0 * si.chans + 0] = 1;
		if (si.chans == 1) // mono source
			matrix[1 * si.chans + 0] = 1;
		else
			matrix[1 * si.chans + 1] = 1;
	}
	if (mi.chans >= 4 && self.speaker2Switch.state) { // 2nd pair of speakers enabled
		if (si.chans > 2) { // multi-channel source
			matrix[2 * si.chans + 2] = 1;
			if (si.chans > 3) matrix[3 * si.chans + 3] = 1;
		} else {
			matrix[2 * si.chans + 0] = 1;
			if (si.chans == 1) // mono source
				matrix[3 * si.chans + 0] = 1;
			else // stereo source
				matrix[3 * si.chans + 1] = 1;
		}
	}
	if (mi.chans >= 6 && self.speaker3Switch.state) { // 3rd pair of speakers enabled
		if (si.chans > 2) { // multi-channel source
			if (si.chans > 4) matrix[4 * si.chans + 4] = 1;
			if (si.chans > 5) matrix[5 * si.chans + 5] = 1;
		} else {
			matrix[4 * si.chans + 0] = 1;
			if (si.chans == 1) // mono source
				matrix[5 * si.chans + 0] = 1;
			else // stereo source
				matrix[5 * si.chans + 1] = 1;
		}
	}
	if (mi.chans >= 8 && self.speaker4Switch.state) { // 4th pair of speakers enabled
		if (si.chans > 2) { // multi-channel source
			if (si.chans > 6) matrix[6 * si.chans + 6] = 1;
			if (si.chans > 7) matrix[7 * si.chans + 7] = 1;
		} else {
			matrix[6 * si.chans + 0] = 1;
			if (si.chans == 1) // mono source
				matrix[7 * si.chans + 0] = 1;
			else // stereo source
				matrix[7 * si.chans + 1] = 1;
		}
	}
	BASS_Mixer_ChannelSetMatrixEx(source, matrix, ramp ? 0.1 : 0); // apply the matrix
	free(matrix);
}

- (IBAction)openFile:(id)sender {
	NSOpenPanel *panel=[NSOpenPanel openPanel];
	if ([panel runModal] == NSModalResponseOK) {
		BASS_CHANNELINFO ci;
		BASS_INFO di;
		BASS_StreamFree(mixer); // free old mixer (and source due to AUTOFREE)
		NSString *file=[panel filename];
		if (!(source = BASS_StreamCreateFile(0, [file UTF8String], 0, 0, BASS_STREAM_DECODE | BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT))
			&& !(source = BASS_MusicLoad(0, [file UTF8String], 0, 0, BASS_MUSIC_DECODE | BASS_MUSIC_RAMPS | BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT, 1))) {
			[sender setTitle:@"Open file..."];
			Error(@"Can't play the file");
			return;
		}
		BASS_ChannelGetInfo(source, &ci); // get source info for sample rate
		BASS_GetInfo(&di); // get device info for speaker count
		mixer = BASS_Mixer_StreamCreate(ci.freq, di.speakers < 8 ? di.speakers : 8, BASS_SAMPLE_FLOAT); // create mixer with source sample rate and device speaker count
		if (!mixer) { // failed
			BASS_ChannelFree(source);
			[sender setTitle:@"Open file..."];
			Error(@"Can't play the file");
			return;
		}
		BASS_ChannelSetAttribute(mixer, BASS_ATTRIB_BUFFER, 0); // disable playback buffering to minimize latency
		BASS_Mixer_StreamAddChannel(mixer, source, BASS_MIXER_CHAN_MATRIX | BASS_STREAM_AUTOFREE); // add the source to the mix with matrix-mixing enabled
		[self setMatrix:FALSE]; // set the matrix
		BASS_ChannelPlay(mixer, FALSE); // start playing
		[sender setTitle:[file lastPathComponent]];
		// enable the speaker switches according to the speaker count
		[self.speaker2Switch setEnabled:(di.speakers >= 4)];
		[self.speaker3Switch setEnabled:(di.speakers >= 6)];
		[self.speaker4Switch setEnabled:(di.speakers >= 8)];
	}
}

- (IBAction)switchSpeakers:(id)sender {
	[self setMatrix:TRUE]; // update the matrix
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
}

@end

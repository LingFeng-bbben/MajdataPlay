/*
	BASS live spectrum analyser example
	Copyright (c) 2002-2024 Un4seen Developments Ltd.
*/

#import "ViewController.h"
#include "bass.h"

@implementation ViewController

#define SPECWIDTH 368	// display width
#define SPECHEIGHT 127	// height (changing requires palette adjustments too)
#define SPECRATE 30		// refresh rate

typedef struct {
	BYTE rgbRed, rgbGreen, rgbBlue, rgbReserved;
} RGBQUAD;

HRECORD chan;	// recording channel

RGBQUAD specbuf[SPECWIDTH * SPECHEIGHT];
RGBQUAD palette[256];

int specmode;	// spectrum mode
int specpos;	// marker pos for 3D mode

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

- (void)UpdateSpectrum:(NSTimer*)timer {
	static DWORD quietcount = 0;
	int x, y, y1;

	if (specmode == 3) { // waveform
		short buf[SPECWIDTH];
		BASS_ChannelGetData(chan, buf, SPECWIDTH * sizeof(short)); // get the sample data
		memset(specbuf, 0, sizeof(specbuf));
		for (x = 0; x < SPECWIDTH; x++) {
			int v = (32767 - buf[x]) * SPECHEIGHT / 65536; // invert and scale to fit display
			if (!x) y = v;
			do { // draw line from previous sample...
				if (y < v) y++;
				else if (y > v) y--;
				specbuf[y * SPECWIDTH + x] = palette[abs(y - SPECHEIGHT / 2) * 2 + 1];
			} while (y != v);
		}
	} else {
		float fft[1024]; // get the FFT data
		BASS_ChannelGetData(chan, fft, BASS_DATA_FFT2048);

		if (!specmode) { // "normal" FFT
			memset(specbuf, 0, sizeof(specbuf));
			for (x = 0; x < SPECWIDTH / 2; x++) {
#if 1
				y = sqrt(fft[x + 1]) * 3 * SPECHEIGHT - 4; // scale it (sqrt to make low values more visible)
#else
				y = fft[x + 1] * 10 * SPECHEIGHT; // scale it (linearly)
#endif
				if (y > SPECHEIGHT) y = SPECHEIGHT; // cap it
				if (x && (y1 = (y + y1) / 2)) // interpolate from previous to make the display smoother
					while (--y1 >= 0) specbuf[(SPECHEIGHT - 1 - y1) * SPECWIDTH + x * 2 - 1] = palette[y1 + 1];
				y1 = y;
				while (--y >= 0) specbuf[(SPECHEIGHT - 1 - y) * SPECWIDTH + x * 2] = palette[y + 1]; // draw level
			}
		} else if (specmode == 1) { // logarithmic, acumulate & average bins
			int b0 = 0;
			memset(specbuf, 0, sizeof(specbuf));
#define BANDS 28
			for (x = 0; x < BANDS; x++) {
				float peak = 0;
				int b1 = pow(2, x * 10.0 / (BANDS - 1));
				if (b1 > 1023) b1 = 1023;
				if (b1 <= b0) b1 = b0 + 1; // make sure it uses at least 1 FFT bin
				for (; b0 < b1; b0++)
					if (peak < fft[1 + b0]) peak = fft[1 + b0];
				y = sqrt(peak) * 3 * SPECHEIGHT - 4; // scale it (sqrt to make low values more visible)
				if (y > SPECHEIGHT) y = SPECHEIGHT; // cap it
				while (--y >= 0)
					for (y1 = 0; y1 < SPECWIDTH / BANDS - 2; y1++)
						specbuf[(SPECHEIGHT - 1 - y) * SPECWIDTH + x * (SPECWIDTH / BANDS) + y1] = palette[y + 1]; // draw bar
			}
		} else { // "3D"
			for (x = 0; x < SPECHEIGHT; x++) {
				y = sqrt(fft[x + 1]) * 3 * 127; // scale it (sqrt to make low values more visible)
				if (y > 127) y = 127; // cap it
				specbuf[(SPECHEIGHT - 1 - x) * SPECWIDTH + specpos] = palette[128 + y]; // plot it
			}
			// move marker onto next position
			specpos = (specpos + 1) % SPECWIDTH;
			for (x = 0; x < SPECHEIGHT; x++) specbuf[x * SPECWIDTH + specpos] = palette[255];
		}
	}

	// update the display
	NSImage *img = [[NSImage alloc] initWithSize:NSMakeSize(SPECWIDTH, SPECHEIGHT)];
	BYTE *pspecbuf = (BYTE*)specbuf;
	[img addRepresentation:[[NSBitmapImageRep alloc] initWithBitmapDataPlanes:&pspecbuf pixelsWide:SPECWIDTH pixelsHigh:SPECHEIGHT bitsPerSample:8 samplesPerPixel:3 hasAlpha:NO isPlanar:NO colorSpaceName:NSDeviceRGBColorSpace bytesPerRow:SPECWIDTH*4 bitsPerPixel:32]];
	self.specView.image = img;

	if (LOWORD(BASS_ChannelGetLevel(chan)) < 1000) { // check if it's quiet
		quietcount++;
		if (quietcount > SPECRATE) // it's been quiet for over a second
			self.noticeText.stringValue = quietcount & 16 ? @"make some noise!" : @"";
	} else { // not quiet
		quietcount = 0;
		self.noticeText.stringValue = @"click to toggle mode";
	}
}

- (IBAction)changeMode:(id)sender {
	specmode = (specmode + 1) % 4; // change spectrum mode
	memset(specbuf, 0, sizeof(specbuf)); // clear display
}

- (void)viewDidLoad {
	[super viewDidLoad];

	// check the correct BASS was loaded
	if (HIWORD(BASS_GetVersion()) != BASSVERSION) {
		Error(@"An incorrect version of BASS was loaded");
		exit(0);
	}

	// initialize default recording device
	if (!BASS_RecordInit(-1)) {
		Error(@"Can't initialize device");
		exit(0);
	}
	// start recording (44100hz mono 16-bit)
	if (!(chan = BASS_RecordStart(44100, 1, 0, RECORDPROC_TRUE, 0))) {
		Error(@"Can't start recording");
		BASS_RecordFree();
		exit(0);
	}

	// setup palette
	RGBQUAD *pal = palette;
	int a;
	memset(palette, 0, sizeof(palette));
	for (a = 1; a < 128; a++) {
		pal[a].rgbGreen = 256 - 2 * a;
		pal[a].rgbRed = 2 * a;
	}
	for (a = 0; a < 32; a++) {
		pal[128 + a].rgbBlue = 8 * a;
		pal[128 + 32 + a].rgbBlue = 255;
		pal[128 + 32 + a].rgbRed = 8 * a;
		pal[128 + 64 + a].rgbRed = 255;
		pal[128 + 64 + a].rgbBlue = 8 * (31 - a);
		pal[128 + 64 + a].rgbGreen = 8 * a;
		pal[128 + 96 + a].rgbRed = 255;
		pal[128 + 96 + a].rgbGreen = 255;
		pal[128 + 96 + a].rgbBlue = 8 * a;
	}

	// start display update timer
	[[NSRunLoop currentRunLoop] addTimer:[NSTimer timerWithTimeInterval:1.0/SPECRATE target:self selector:@selector(UpdateSpectrum:) userInfo:nil repeats:YES] forMode:NSRunLoopCommonModes];}

@end

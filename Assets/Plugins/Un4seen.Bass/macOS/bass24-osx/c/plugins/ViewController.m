/*
	BASS plugins example
	Copyright (c) 2005-2022 Un4seen Developments Ltd.
*/

#import "ViewController.h"
#include "bass.h"

#include <glob.h>

@implementation ViewController

DWORD chan;	// channel handle

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

// translate a CTYPE value to text
const char *GetCTypeString(DWORD ctype, HPLUGIN plugin)
{
	if (plugin) { // using a plugin
		const BASS_PLUGININFO *pinfo = BASS_PluginGetInfo(plugin); // get plugin info
		int a;
		for (a = 0; a < pinfo->formatc; a++) {
			if (pinfo->formats[a].ctype == ctype) // found a "ctype" match
				return pinfo->formats[a].name; // return it's name
		}
	}
	// check built-in stream formats
	if (ctype == BASS_CTYPE_STREAM_VORBIS) return "Ogg Vorbis";
	if (ctype == BASS_CTYPE_STREAM_MP1) return "MPEG layer 1";
	if (ctype == BASS_CTYPE_STREAM_MP2) return "MPEG layer 2";
	if (ctype == BASS_CTYPE_STREAM_MP3) return "MPEG layer 3";
	if (ctype == BASS_CTYPE_STREAM_AIFF) return "Audio IFF";
	if (ctype == BASS_CTYPE_STREAM_WAV_PCM) return "PCM WAVE";
	if (ctype == BASS_CTYPE_STREAM_WAV_FLOAT) return "Floating-point WAVE";
	if (ctype & BASS_CTYPE_STREAM_WAV) return "WAVE";
	if (ctype == BASS_CTYPE_STREAM_CA) { // CoreAudio codec
		static char buf[100];
		const TAG_CA_CODEC *codec = (TAG_CA_CODEC*)BASS_ChannelGetTags(chan, BASS_TAG_CA_CODEC); // get codec info
		snprintf(buf, sizeof(buf), "CoreAudio: %s", codec->name);
		return buf;
	}
	return "?";
}

- (IBAction)openFile:(id)sender {
	NSOpenPanel *panel = [NSOpenPanel openPanel];
	if ([panel runModal] == NSModalResponseOK) {
		BASS_StreamFree(chan); // free old stream before opening new
		NSString *file = [panel filename];
		if (!(chan=BASS_StreamCreateFile(0, [file UTF8String], 0, 0, BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT))) {
			// it ain't playable
			[sender setTitle:@"Open file..."];
			self.infoText.stringValue=@"";
			self.positionText.stringValue=@"";
			self.positionSlider.maxValue=0;
			Error(@"Can't play the file");
		} else {
			[sender setTitle:[file lastPathComponent]];
			// display the file type
			BASS_CHANNELINFO info;
			BASS_ChannelGetInfo(chan, &info);
			self.infoText.stringValue=[NSString stringWithFormat:@"channel type = %x (%s)",
				info.ctype, GetCTypeString(info.ctype, info.plugin)];
			// update scroller range
			QWORD len = BASS_ChannelGetLength(chan, BASS_POS_BYTE);
			if (len == -1) len = 0; // unknown length
			self.positionSlider.maxValue=BASS_ChannelBytes2Seconds(chan, len);
			BASS_ChannelPlay(chan, FALSE);
		}
	}
}

- (IBAction)changePosition:(id)sender {
	BASS_ChannelSetPosition(chan, BASS_ChannelSeconds2Bytes(chan, [sender doubleValue]), BASS_POS_BYTE); // set the position
}

- (void)TimerProc:(NSTimer*)timer {
	if (chan) {
		double len = BASS_ChannelBytes2Seconds(chan, BASS_ChannelGetLength(chan, BASS_POS_BYTE));
		double pos = BASS_ChannelBytes2Seconds(chan, BASS_ChannelGetPosition(chan, BASS_POS_BYTE));
		self.positionText.stringValue=[NSString stringWithFormat:@"%u:%02u / %u:%02u", (int)pos / 60, (int)pos % 60, (int)len / 60, (int)len % 60];
		self.positionSlider.doubleValue = pos;
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

	// look for plugins (alongside BASS library)
	const char *basspath = BASS_GetConfigPtr(BASS_CONFIG_FILENAME);
	if (basspath) {
		NSString *path = [[NSString stringWithUTF8String:basspath] stringByDeletingLastPathComponent];
		glob_t g;
		if (!glob([[path stringByAppendingString:@"/libbass?*.dylib"] UTF8String], 0, 0, &g)) {
			for (int b = 0; b < g.gl_pathc; b++) {
				if (BASS_PluginLoad(g.gl_pathv[b], 0)) { // plugin loaded, add it to the list...
					[self.pluginListController addObject:[[NSString stringWithUTF8String:g.gl_pathv[b]] lastPathComponent]];
				}
			}
			globfree(&g);
		}
	}
	if (![[self.pluginListController arrangedObjects] count])
		[self.pluginListController addObject:@"no plugins - visit the BASS webpage to get some"];

	// timer to update the position display
	[[NSRunLoop currentRunLoop] addTimer:[NSTimer timerWithTimeInterval:0.1 target:self selector:@selector(TimerProc:) userInfo:nil repeats:YES] forMode:NSRunLoopCommonModes];
}

@end

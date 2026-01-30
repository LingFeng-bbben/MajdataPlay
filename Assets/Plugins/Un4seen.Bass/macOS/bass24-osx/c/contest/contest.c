/*
	BASS simple console player
	Copyright (c) 1999-2022 Un4seen Developments Ltd.
*/

#include <stdlib.h>
#include <stdio.h>
#include "bass.h"

#ifdef _WIN32 // Windows
#include <conio.h>
#else // OSX/Linux
#include <sys/time.h>
#include <termios.h>
#include <string.h>
#include <unistd.h>

#define Sleep(x) usleep(x*1000)

int _kbhit()
{
	int r;
	fd_set rfds;
	struct timeval tv = { 0 };
	struct termios term, oterm;
	tcgetattr(0, &oterm);
	memcpy(&term, &oterm, sizeof(term));
	cfmakeraw(&term);
	tcsetattr(0, TCSANOW, &term);
	FD_ZERO(&rfds);
	FD_SET(0, &rfds);
	r = select(1, &rfds, NULL, NULL, &tv);
	tcsetattr(0, TCSANOW, &oterm);
	return r;
}
#endif

// display error messages
void Error(const char *text)
{
	printf("Error(%d): %s\n", BASS_ErrorGetCode(), text);
	BASS_Free();
	exit(0);
}

void ListDevices()
{
	BASS_DEVICEINFO di;
	int a;
	for (a = 0; BASS_GetDeviceInfo(a, &di); a++) {
		if (di.flags & BASS_DEVICE_ENABLED) // enabled output device
			printf("dev %d: %s\n", a, di.name);
	}
}

int main(int argc, char **argv)
{
	DWORD chan, act, level;
	QWORD pos;
	double secs;
	int a, filep, device = -1;
	BASS_CHANNELINFO info;

	printf("BASS simple console player\n"
		"--------------------------\n");

	// check the correct BASS was loaded
	if (HIWORD(BASS_GetVersion()) != BASSVERSION) {
		printf("An incorrect version of BASS was loaded");
		return 0;
	}

	for (filep = 1; filep < argc; filep++) {
		if (!strcmp(argv[filep], "-l")) {
			ListDevices();
			return 0;
		} else if (!strcmp(argv[filep], "-d") && filep + 1 < argc)
			device = atoi(argv[++filep]);
		else
			break;
	}
	if (filep == argc) {
		printf("\tusage: contest [-l] [-d #] <file>\n"
			"\t-l = list devices\n"
			"\t-d = device number\n");
		return 0;
	}

	BASS_SetConfig(BASS_CONFIG_NET_PLAYLIST, 2); // enable playlist processing

	// initialize output device
	if (!BASS_Init(device, 48000, 0, 0, NULL))
		Error("Can't initialize device");

	if (strstr(argv[filep], "://")) {
		// try streaming the URL
		chan = BASS_StreamCreateURL(argv[filep], 0, BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT, 0, 0);
	} else {
		// try streaming the file
		chan = BASS_StreamCreateFile(0, argv[filep], 0, 0, BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT);
		if (!chan && BASS_ErrorGetCode() == BASS_ERROR_FILEFORM) {
			// try MOD music formats
			chan = BASS_MusicLoad(0, argv[filep], 0, 0, BASS_SAMPLE_LOOP | BASS_SAMPLE_FLOAT | BASS_MUSIC_RAMPS | BASS_MUSIC_PRESCAN, 1);
		}
	}
	if (!chan) Error("Can't play the file");

	BASS_ChannelGetInfo(chan, &info);
	printf("ctype: %x\n", info.ctype);
	if (HIWORD(info.ctype) != 2) { // 2 = HMUSIC
		if (info.origres)
			printf("format: %u Hz, %d chan, %d bit\n", info.freq, info.chans, LOWORD(info.origres));
		else
			printf("format: %u Hz, %d chan\n", info.freq, info.chans);
	}
	pos = BASS_ChannelGetLength(chan, BASS_POS_BYTE);
	if (pos != -1) {
		secs = BASS_ChannelBytes2Seconds(chan, pos);
		if (HIWORD(info.ctype) == 2)
			printf("length: %u:%02u (%llu samples), %u orders\n", (int)secs / 60, (int)secs % 60, (long long)(secs * info.freq), (DWORD)BASS_ChannelGetLength(chan, BASS_POS_MUSIC_ORDER));
		else
			printf("length: %u:%02u (%llu samples)\n", (int)secs / 60, (int)secs % 60, (long long)(secs * info.freq));
	} else if (HIWORD(info.ctype) == 2)
		printf("length: %u orders\n", (DWORD)BASS_ChannelGetLength(chan, BASS_POS_MUSIC_ORDER));

	BASS_ChannelPlay(chan, FALSE);

	while (!_kbhit() && (act = BASS_ChannelIsActive(chan))) {
		// display some stuff and wait a bit
		level = BASS_ChannelGetLevel(chan);
		pos = BASS_ChannelGetPosition(chan, BASS_POS_BYTE);
		secs = BASS_ChannelBytes2Seconds(chan, pos);
		printf(" %u:%02u (%08lld)", (int)secs / 60, (int)secs % 60, (long long)(secs * info.freq));
		if (HIWORD(info.ctype) == 2) {
			pos = BASS_ChannelGetPosition(chan, BASS_POS_MUSIC_ORDER);
			printf(" | %03u:%03u", LOWORD(pos), HIWORD(pos));
		}
		printf(" | L ");
		if (act == BASS_ACTIVE_STALLED) { // playback has stalled
			printf("-     buffering: %3u%%     -", 100 - (DWORD)BASS_StreamGetFilePosition(chan, BASS_FILEPOS_BUFFERING));
		} else {
			for (a = 27204; a > 200; a = a * 2 / 3) putchar(LOWORD(level) >= a ? '*' : '-');
			putchar(' ');
			for (a = 210; a < 32768; a = a * 3 / 2) putchar(HIWORD(level) >= a ? '*' : '-');
		}
		printf(" R | cpu %.2f%%  \r", BASS_GetCPU());
		fflush(stdout);
		Sleep(50);
	}
	printf("                                                                             \n");

	BASS_Free();
	return 0;
}

/**************************************************************************
 reverse.c - Copyright (c) 2002-2008 (: JOBnik! :) [Arthur Aminov, ISRAEL]
                                                   [http://www.jobnik.org]
                                                   [   bass_fx@jobnik.org]
 
 BASS_FX playing in reverse with tempo
**************************************************************************/

#include <Carbon/Carbon.h>
#include "bass.h"
#include "bass_fx.h"

WindowPtr win=NULL;

HSTREAM chan;		// reversed tempo handle

ControlRef GetControl(int id)
{
	ControlRef cref;
	ControlID cid={0,id};
	GetControlByID(win,&cid,&cref);
	return cref;
}

void SetupControlHandler(int id, DWORD event, EventHandlerProcPtr proc)
{
	EventTypeSpec cspec={kEventClassControl,event};
	ControlRef cref=GetControl(id);
	InstallControlEventHandler(cref,NewEventHandlerUPP(proc),1,&cspec,cref,NULL);
}

void SetControlText(int id, const char *text)
{
	CFStringRef cs=CFStringCreateWithCString(0,text,0);
	SetControlTitleWithCFString(GetControl(id),cs);
	CFRelease(cs);
}

void SetStaticText(int id, const char *text)
{
	ControlRef cref=GetControl(id);
	SetControlData(cref,kControlNoPart,kControlStaticTextTextTag,strlen(text),text);
	DrawOneControl(cref);
}

// display error dialogs
void Error(const char *es)
{
	short i;
	char mes[200];
	sprintf(mes,"%s\n\n(error code: %d)",es,BASS_ErrorGetCode());
	CFStringRef ces=CFStringCreateWithCString(0,mes,0);
	DialogRef alert;
	CreateStandardAlert(0,CFSTR("Error"),ces,NULL,&alert);
	RunStandardAlert(alert,NULL,&i);
	CFRelease(ces);
}

pascal OSStatus OpenEventHandler(EventHandlerCallRef inHandlerRef, EventRef inEvent, void *inUserData)
{
	DWORD p;
	NavDialogRef fileDialog;
	NavDialogCreationOptions fo;
	NavGetDefaultDialogCreationOptions(&fo);
	fo.optionFlags=0;
	fo.parentWindow=win;
	NavCreateChooseFileDialog(&fo,NULL,NULL,NULL,NULL,NULL,&fileDialog);
	if (!NavDialogRun(fileDialog)) {
		NavReplyRecord r;
		if (!NavDialogGetReply(fileDialog,&r)) {
			AEKeyword k;
			FSRef fr;
			if (!AEGetNthPtr(&r.selection,1,typeFSRef,&k,NULL,&fr,sizeof(fr),NULL)) {
				char file[256];
				FSRefMakePath(&fr,(BYTE*)file,sizeof(file));
				BASS_StreamFree(chan); // free old streams before opening new
		
				// create decode channel
				chan = BASS_StreamCreateFile(FALSE,file,0,0,BASS_SAMPLE_FLOAT|BASS_STREAM_DECODE|BASS_STREAM_PRESCAN);

				// check for MOD
				if (!chan) chan = BASS_MusicLoad(FALSE, file, 0, 0, BASS_SAMPLE_FLOAT|BASS_MUSIC_RAMP|BASS_STREAM_DECODE|BASS_MUSIC_PRESCAN,0);

				if (!chan) {
					SetControlTitleWithCFString(inUserData,CFSTR("click here to open a file && play it..."));
					Error("Selected file couldn't be loaded!");
				} else {
					// create new stream - decoded & reversed
					// 2 seconds decoding block as a decoding channel
					if (!(chan=BASS_FX_ReverseCreate(chan, 2, BASS_STREAM_DECODE|BASS_FX_FREESOURCE))) {
						SetControlTitleWithCFString(inUserData,CFSTR("click here to open a file && play it..."));
						Error("Couldn't create a reversed stream!");
						BASS_StreamFree(chan);
						BASS_MusicFree(chan);
					} else {
						// create a new stream - decoded & resampled :)
						if (!(chan=BASS_FX_TempoCreate(chan, BASS_SAMPLE_LOOP|BASS_FX_FREESOURCE))){
							SetControlTitleWithCFString(inUserData,CFSTR("click here to open a file && play it..."));
							Error("Couldn't create a resampled stream!");
							BASS_StreamFree(chan);
							BASS_MusicFree(chan);
						} else {
							// update the Button to show the loaded file
							c2pstrcpy((BYTE*)file,file);
							SetControlTitle(inUserData,(BYTE*)file);

							// update the position slider
							p = BASS_ChannelBytes2Seconds(chan, BASS_ChannelGetLength(chan, BASS_POS_BYTE));
							SetControl32BitMaximum(GetControl(15),p);
							SetControl32BitValue(GetControl(15),p);

							// set Volume
							p = GetControl32BitValue(GetControl(11));
							BASS_ChannelSetAttribute(chan, BASS_ATTRIB_VOL, (float)p/100.0f);

							// update tempo slider
							SetControl32BitValue(GetControl(13),0);
							SetStaticText(12,"Tempo = 0%");

							// play new created stream
							BASS_ChannelPlay(chan,FALSE);
						}
					}
				}
			}
			NavDisposeReply(&r);
		}
	}
	NavDialogDispose(fileDialog);
	return noErr;
}

pascal void VolEventHandler(ControlHandle control, SInt16 part)
{
	float p=(float)GetControl32BitValue(control);
	BASS_ChannelSetAttribute(chan, BASS_ATTRIB_VOL, p/100.0f);
}

pascal void PosEventHandler(ControlHandle control, SInt16 part)
{
	double p=(double)GetControl32BitValue(control);
	BASS_ChannelSetPosition(chan,BASS_ChannelSeconds2Bytes(chan,p), BASS_POS_BYTE);
}

pascal void TempoEventHandler(ControlHandle control, SInt16 part)
{
	float p=(float)GetControl32BitValue(control);
	char c[20];

	// set new tempo
	BASS_ChannelSetAttribute(chan, BASS_ATTRIB_TEMPO, p);

	// update tempo static text
	sprintf(c,"Tempo = %d%%", (long)p);
	SetStaticText(12,c);
}

pascal OSStatus DirEventHandler(EventHandlerCallRef inHandlerRef, EventRef inEvent, void *inUserData)
{
	DWORD srcChan=BASS_FX_TempoGetSource(chan);
	float dir;
	BASS_ChannelGetAttribute(srcChan, BASS_ATTRIB_REVERSE_DIR, &dir);

	if(dir<0){
		BASS_ChannelSetAttribute(srcChan, BASS_ATTRIB_REVERSE_DIR, BASS_FX_RVS_FORWARD);
		SetControlText(16, "Playing Direction - Forward");
	}else{
		BASS_ChannelSetAttribute(srcChan, BASS_ATTRIB_REVERSE_DIR, BASS_FX_RVS_REVERSE);
		SetControlText(16,"Playing Direction - Reverse");
	}

	return noErr;
}

pascal void TimerProc(EventLoopTimerRef inTimer, void *inUserData)
{
	float ratio=BASS_FX_TempoGetRateRatio(chan);
	if (!ratio) return;
	SetControl32BitValue(GetControl(15),(DWORD)BASS_ChannelBytes2Seconds(chan,BASS_ChannelGetPosition(chan,BASS_POS_BYTE))); // update position
	{ // show the approximate position in MM:SS format
		char c[50];
		DWORD totalsec=GetControl32BitMaximum(GetControl(15))/ratio;
		DWORD posec=GetControl32BitValue(GetControl(15))/ratio;
		sprintf(c,"Playing position: %02u:%02u / %02u:%02u",posec/60,posec%60,totalsec/60,totalsec%60);
		SetStaticText(14,c);
	}
}

int main(int argc, char* argv[])
{
	IBNibRef 		nibRef;
	OSStatus		err;
	
	InitCursor();

	// check the correct BASS was loaded
	if (HIWORD(BASS_GetVersion())!=BASSVERSION) {
		Error("An incorrect version of BASS.DLL was loaded (2.4 is required)");
		return 0;
	}

	// check the correct BASS_FX was loaded
	if (HIWORD(BASS_FX_GetVersion())!=BASSVERSION) {
		Error("An incorrect version of BASS_FX.DLL was loaded (2.4 is required)");
		return 0;
	}

	// initialize default output device
	if(!BASS_Init(-1,44100,0,NULL,NULL)) {
		Error("Can't initialize device");
		return 0;
	}

	// Create Window and Stuff
	err = CreateNibReference(CFSTR("reverse"), &nibRef);
	if (err) return err;
	err = CreateWindowFromNib(nibRef, CFSTR("MainWindow"), &win);
	if (err) return err;
	DisposeNibReference(nibRef);

	SetupControlHandler(10,kEventControlHit,OpenEventHandler);
	SetControlAction(GetControl(11),NewControlActionUPP(VolEventHandler));
	SetControlAction(GetControl(13),NewControlActionUPP(TempoEventHandler));
	SetControlAction(GetControl(15),NewControlActionUPP(PosEventHandler));
	SetupControlHandler(16,kEventControlHit,DirEventHandler);

	EventLoopTimerRef timer;
	InstallEventLoopTimer(GetCurrentEventLoop(),kEventDurationNoWait,kEventDurationSecond/2,TimerProc,0,&timer);

	// The window was created hidden so show it.
	ShowWindow(win);
    
	// Call the event loop
	RunApplicationEventLoop();
   
	// Close output
	BASS_Free();

	return 0;
}

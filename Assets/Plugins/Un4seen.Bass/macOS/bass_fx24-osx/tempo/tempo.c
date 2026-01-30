/************************************************************************
 tempo.c - Copyright (c) 2003-2008 (: JOBnik! :) [Arthur Aminov, ISRAEL]
                                                 [http://www.jobnik.org]
                                                 [   bass_fx@jobnik.org]
 
 BASS_FX tempo / rate / pitch with dsp fx
************************************************************************/

#include <Carbon/Carbon.h>
#include "bass.h"
#include "bass_fx.h"

WindowPtr win=NULL;

HSTREAM chan;			// tempo channel handle
HFX fxEQ;				// dsp peaking eq handle
float oldfreq;			// old sample rate

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

// update dsp eq
void UpdateFX(int b)
{
	BASS_BFX_PEAKEQ eq;

	int v = GetControl32BitValue(GetControl(12+b));
    
	eq.lBand = b;	// get values of the selected band
	BASS_FXGetParameters(fxEQ, &eq);
	eq.fGain = v;
	BASS_FXSetParameters(fxEQ, &eq);
}

// set dsp eq
void SetDSP_EQ(float fGain, float fBandwidth, float fQ, float fCenter_Bass, float fCenter_Mid, float fCenter_Treble)
{
	BASS_BFX_PEAKEQ eq;

	// set peaking equalizer effect with no bands
	fxEQ=BASS_ChannelSetFX(chan, BASS_FX_BFX_PEAKEQ,0);

	eq.fGain=fGain;
	eq.fQ=fQ;
	eq.fBandwidth=fBandwidth;
	eq.lChannel=BASS_BFX_CHANALL;

	// create 1st band for bass
	eq.lBand=0;
	eq.fCenter=fCenter_Bass;
	BASS_FXSetParameters(fxEQ, &eq);

	// create 2nd band for mid
	eq.lBand=1;
	eq.fCenter=fCenter_Mid;
	BASS_FXSetParameters(fxEQ, &eq);

	// create 3rd band for treble
	eq.lBand=2;
	eq.fCenter=fCenter_Treble;
	BASS_FXSetParameters(fxEQ, &eq);

	// update dsp eq
	UpdateFX(0);
	UpdateFX(1);
	UpdateFX(2);
}

void SetTempoText(int id, const char *txt, int val)
{
	char c[50];

	// update tempo/rate/pitch static text
	sprintf(c,txt, val);
	SetStaticText(id,c);
}

pascal OSStatus OpenEventHandler(EventHandlerCallRef inHandlerRef, EventRef inEvent, void *inUserData)
{
	DWORD p;
	float freq;
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
				BASS_StreamFree(chan); // free old streams/dsps before opening new
		
				// create decode channel
				chan = BASS_StreamCreateFile(FALSE,file,0,0,BASS_SAMPLE_FLOAT|BASS_STREAM_DECODE);

				// check for MOD
				if (!chan) chan = BASS_MusicLoad(FALSE, file, 0, 0, BASS_SAMPLE_FLOAT|BASS_MUSIC_RAMP|BASS_MUSIC_PRESCAN|BASS_STREAM_DECODE,0);

				if (!chan) {
					SetControlTitleWithCFString(inUserData,CFSTR("click here to open a file && play it..."));
					Error("Selected file couldn't be loaded!");
				} else {
					// update the position slider
					p = (DWORD)BASS_ChannelBytes2Seconds(chan, BASS_ChannelGetLength(chan, BASS_POS_BYTE));
					SetControl32BitMaximum(GetControl(22),p);
					SetControl32BitValue(GetControl(22),0);

					// get current sample rate
					BASS_ChannelGetAttribute(chan, BASS_ATTRIB_FREQ, &freq);
					oldfreq = freq;

					// create a new stream - decoded & resampled :)
					if (!(chan=BASS_FX_TempoCreate(chan, BASS_SAMPLE_LOOP|BASS_FX_FREESOURCE))){
						SetControlTitleWithCFString(inUserData,CFSTR("click here to open a file && play it..."));
						Error("Couldn't create a resampled stream!");
						BASS_StreamFree(chan);
						BASS_MusicFree(chan);
					} else {
						// set dsp eq to channel
						SetDSP_EQ(0.0f, 2.5f, 0.0f, 125.0f, 1000.0f, 8000.0f);

						// update the Button to show the loaded file
						c2pstrcpy((BYTE*)file,file);
						SetControlTitle(inUserData,(BYTE*)file);

						// set Volume
						p = GetControl32BitValue(GetControl(11));
						BASS_ChannelSetAttribute(chan, BASS_ATTRIB_VOL, (float)p/100.0f);

						// update tempo slider
						SetControl32BitValue(GetControl(16),0);
						SetStaticText(15,"Tempo = 0%");

						// update rate slider min/max values
						SetControl32BitMaximum(GetControl(18),(long)(freq * 1.3f));
						SetControl32BitMinimum(GetControl(18),(long)(freq * 0.7f));
						SetControl32BitValue(GetControl(18),(long)freq);
						SetTempoText(17,"Samplerate = %dHz", (long)freq);
						
						SetControl32BitValue(GetControl(20),0);
						SetStaticText(19,"Pitch Scaling = 0 semitones");

						// play new created stream
						BASS_ChannelPlay(chan,FALSE);
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
	DWORD p=GetControl32BitValue(control);
	BASS_ChannelSetAttribute(chan, BASS_ATTRIB_VOL, p/100.f);
}

pascal void PosEventHandler(ControlHandle control, SInt16 part)
{
	DWORD p=GetControl32BitValue(control);
	BASS_ChannelSetPosition(chan,BASS_ChannelSeconds2Bytes(chan,p),BASS_POS_BYTE);
}

pascal void TempoEventHandler(ControlHandle control, SInt16 part)
{
	float p=(float)GetControl32BitValue(control);

	// set new tempo
	BASS_ChannelSetAttribute(chan, BASS_ATTRIB_TEMPO, p);

	// update tempo static text
	SetTempoText(15,"Tempo = %d%%", (long)p);
}

pascal void RateEventHandler(ControlHandle control, SInt16 part)
{
	float p=(float)GetControl32BitValue(control);

	// set new samplerate
	BASS_ChannelSetAttribute(chan, BASS_ATTRIB_TEMPO_FREQ, p);

	// update samplerate static text
	SetTempoText(17,"Samplerate = %dHz", (long)p);

	// update all bands fCenters after changing samplerate
	BASS_BFX_PEAKEQ eq;
	int i;

	for(i=0;i<3;i++){
		eq.lBand = i;
		BASS_FXGetParameters(fxEQ, &eq);
			eq.fCenter = eq.fCenter * p / oldfreq;
		BASS_FXSetParameters(fxEQ, &eq);
	}
	oldfreq = p;
}

pascal void PitchEventHandler(ControlHandle control, SInt16 part)
{
	float p=(float)GetControl32BitValue(control);

	// set new pitch scale
	BASS_ChannelSetAttribute(chan, BASS_ATTRIB_TEMPO_PITCH, p);

	// update pitch static text
	SetTempoText(19,"Pitch Scaling = %d semitones", (long)p);
}

pascal void eqBassEventHandler(ControlHandle control, SInt16 part)
{
	UpdateFX(0);
}

pascal void eqMidEventHandler(ControlHandle control, SInt16 part)
{
	UpdateFX(1);
}

pascal void eqTrebleEventHandler(ControlHandle control, SInt16 part)
{
	UpdateFX(2);
}

pascal void TimerProc(EventLoopTimerRef inTimer, void *inUserData)
{
	float ratio=BASS_FX_TempoGetRateRatio(chan);
	if (!ratio) return;
	SetControl32BitValue(GetControl(22),(DWORD)BASS_ChannelBytes2Seconds(chan,BASS_ChannelGetPosition(chan,BASS_POS_BYTE))); // update position
	{ // show the approximate position in MM:SS format
		char c[50];
		DWORD totalsec=GetControl32BitMaximum(GetControl(22))/ratio;
		DWORD posec=GetControl32BitValue(GetControl(22))/ratio;
		sprintf(c,"Playing position: %02u:%02u / %02u:%02u",posec/60,posec%60,totalsec/60,totalsec%60);
		SetStaticText(21,c);
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
	err = CreateNibReference(CFSTR("tempo"), &nibRef);
	if (err) return err;
	err = CreateWindowFromNib(nibRef, CFSTR("MainWindow"), &win);
	if (err) return err;
	DisposeNibReference(nibRef);

	SetupControlHandler(10,kEventControlHit,OpenEventHandler);
	SetControlAction(GetControl(11),NewControlActionUPP(VolEventHandler));
	SetControlAction(GetControl(16),NewControlActionUPP(TempoEventHandler));
	SetControlAction(GetControl(18),NewControlActionUPP(RateEventHandler));
	SetControlAction(GetControl(20),NewControlActionUPP(PitchEventHandler));
	SetControlAction(GetControl(22),NewControlActionUPP(PosEventHandler));
	SetControlAction(GetControl(12),NewControlActionUPP(eqBassEventHandler));
	SetControlAction(GetControl(13),NewControlActionUPP(eqMidEventHandler));
	SetControlAction(GetControl(14),NewControlActionUPP(eqTrebleEventHandler));

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

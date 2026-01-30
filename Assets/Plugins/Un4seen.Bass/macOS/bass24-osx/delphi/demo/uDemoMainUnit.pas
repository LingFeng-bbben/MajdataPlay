unit uDemoMainUnit;

interface

uses
  System.SysUtils, System.Types, System.UITypes, System.Rtti, System.Classes,
  System.Variants, FMX.Types, FMX.Controls, FMX.Forms, FMX.Dialogs, BASS;

type

  { TDemoMainForm }

  TDemoMainForm = class(TForm)
    btnOpen: TButton;
    btnPlayPause: TButton;
    btnStop: TButton;
    lbTime: TLabel;
    OpenDialog1: TOpenDialog;
    tbSeek: TTrackBar;
    tmTimer: TTimer;
    procedure btnOpenClick(Sender: TObject);
    procedure btnPlayPauseClick(Sender: TObject);
    procedure btnStopClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure tbSeekChange(Sender: TObject);
    procedure tmTimerTimer(Sender: TObject);
  private
    FSeekBarChanging: Boolean;
    FStreamHandle: HSTREAM;
    procedure FreeStream;
    procedure ShowBassErrorMessage(const AInvoker: string);
    procedure UpdateControlsState;
    procedure UpdateTrackPosition;
  end;

var
  DemoMainForm: TDemoMainForm;

implementation

{$R *.fmx}

procedure TDemoMainForm.btnOpenClick(Sender: TObject);
begin
  if OpenDialog1.Execute then
  begin
    FreeStream;
    FStreamHandle := BASS_StreamCreateFile(False,
      PWideChar(OpenDialog1.FileName), 0, 0, BASS_UNICODE or BASS_STREAM_PRESCAN);
    if FStreamHandle = 0 then
      ShowBassErrorMessage('BASS_StreamCreateFile')
    else
      BASS_ChannelPlay(FStreamHandle, False);

    UpdateControlsState;
  end;
end;

procedure TDemoMainForm.btnPlayPauseClick(Sender: TObject);
begin
  if BASS_ChannelIsActive(FStreamHandle) = BASS_ACTIVE_PLAYING then
    BASS_ChannelPause(FStreamHandle)
  else
    BASS_ChannelPlay(FStreamHandle, False);
end;

procedure TDemoMainForm.btnStopClick(Sender: TObject);
begin
  FreeStream;
  UpdateControlsState;
end;

procedure TDemoMainForm.FormCreate(Sender: TObject);
begin
  UpdateControlsState;

  if not BASS_Init(-1, 44100, 0, {$IFDEF MSWINDOWS}0{$ELSE}nil{$ENDIF}, nil) then
  begin
    ShowBassErrorMessage('BASS_Init');
    Exit;
  end;

  if not BASS_Start then
    ShowBassErrorMessage('BASS_Start');
end;

procedure TDemoMainForm.FormDestroy(Sender: TObject);
begin
  BASS_Stop;
  BASS_Free;
end;

procedure TDemoMainForm.FreeStream;
begin
  if FStreamHandle <> 0 then
  begin
    BASS_StreamFree(FStreamHandle);
    FStreamHandle := 0;
  end;
end;

procedure TDemoMainForm.ShowBassErrorMessage(const AInvoker: string);
begin
  MessageDlg(Format('%s fails, error code: %d', [AInvoker, BASS_ErrorGetCode]),
    TMsgDlgType.mtError, [TMsgDlgBtn.mbOK], 0);
end;

procedure TDemoMainForm.tbSeekChange(Sender: TObject);
begin
  if not FSeekBarChanging then
    BASS_ChannelSetPosition(FStreamHandle, BASS_ChannelSeconds2Bytes(FStreamHandle, tbSeek.Value), BASS_POS_BYTE);
end;

procedure TDemoMainForm.tmTimerTimer(Sender: TObject);
begin
  UpdateTrackPosition;
end;

procedure TDemoMainForm.UpdateControlsState;
begin
  btnPlayPause.Enabled := FStreamHandle <> 0;
  btnStop.Enabled := FStreamHandle <> 0;
  tmTimer.Enabled := FStreamHandle <> 0;
  tbSeek.Enabled := FStreamHandle <> 0;
  UpdateTrackPosition;
end;

procedure TDemoMainForm.UpdateTrackPosition;
var
  ADuration, APosition: Double;
begin
  if FStreamHandle <> 0 then
  begin
    ADuration := BASS_ChannelBytes2Seconds(FStreamHandle, BASS_ChannelGetLength(FStreamHandle, BASS_POS_BYTE));
    APosition := BASS_ChannelBytes2Seconds(FStreamHandle, BASS_ChannelGetPosition(FStreamHandle, BASS_POS_BYTE));
    lbTime.Text := FormatDateTime('hh:mm:ss', APosition / SecsPerDay) + ' / ' + FormatDateTime('hh:mm:ss', ADuration / SecsPerDay);

    FSeekBarChanging := True;
    try
      tbSeek.Max := ADuration;
      tbSeek.Value := APosition;
    finally
      FSeekBarChanging := False;
    end;
  end
  else
    lbTime.Text := '--:--:-- / --:--:--';
end;

end.

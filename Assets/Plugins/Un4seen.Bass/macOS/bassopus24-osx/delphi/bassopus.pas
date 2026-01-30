{
  BASSOPUS 2.4 Delphi/Pascal unit
  Copyright (c) 2012-2025 Un4seen Developments Ltd.

  See the BASSOPUS.CHM file for more detailed documentation
}

unit BASSOPUS;

interface

{$IFDEF MSWINDOWS}
uses BASS, Windows;
{$ELSE}
uses BASS;
{$ENDIF}

const
  // BASS_CHANNELINFO type
  BASS_CTYPE_STREAM_OPUS        = $11200;

  // Additional attributes
  BASS_ATTRIB_OPUS_ORIGFREQ     = $13000;
  BASS_ATTRIB_OPUS_GAIN         = $13001;

  BASS_STREAMPROC_OPUS_LOSS     = $40000000;

type
  BASS_OPUS_HEAD = record
	version: Byte;
	channels: Byte;
    preskip: Word;
    inputrate: Cardinal;
    gain: SmallInt;
    mapping: Byte;
    streams: Byte;
    coupled: Byte;
    chanmap: array[0..254] of Byte;
  end;

const
{$IFDEF MSWINDOWS}
  bassopusdll = 'bassopus.dll';
{$ENDIF}
{$IFDEF LINUX}
  bassopusdll = 'libbassopus.so';
{$ENDIF}
{$IFDEF ANDROID}
  bassopusdll = 'libbassopus.so';
{$ENDIF}
{$IFDEF MACOS}
  {$IFDEF IOS}
    bassopusdll = 'bassopus.framework/bassopus';
  {$ELSE}
    bassopusdll = 'libbassopus.dylib';
  {$ENDIF}
{$ENDIF}

function BASS_OPUS_StreamCreate(var head: BASS_OPUS_HEAD; flags: DWORD; proc: STREAMPROC; user: Pointer): HSTREAM; {$IFDEF MSWINDOWS}stdcall{$ELSE}cdecl{$ENDIF}; external bassopusdll;
function BASS_OPUS_StreamCreateFile(filetype: DWORD; fl: pointer; offset, length: QWORD; flags: DWORD): HSTREAM; {$IFDEF MSWINDOWS}stdcall{$ELSE}cdecl{$ENDIF}; external bassopusdll;
function BASS_OPUS_StreamCreateURL(url: PAnsiChar; offset,flags: DWORD; proc: DOWNLOADPROC; user: Pointer): HSTREAM; {$IFDEF MSWINDOWS}stdcall{$ELSE}cdecl{$ENDIF}; external bassopusdll;
function BASS_OPUS_StreamCreateFileUser(system, flags: DWORD; var procs: BASS_FILEPROCS; user: Pointer): HSTREAM; {$IFDEF MSWINDOWS}stdcall{$ELSE}cdecl{$ENDIF}; external bassopusdll;
function BASS_OPUS_StreamPutData(handle: HSTREAM; buffer: Pointer; length: DWORD): DWORD; {$IFDEF MSWINDOWS}stdcall{$ELSE}cdecl{$ENDIF}; external bassopusdll;

implementation

end.
program uDemo;

uses
  FMX.Forms,
  BASS in '..\BASS.pas',
  uDemoMainUnit in 'uDemoMainUnit.pas' {DemoMainForm};

{$R *.res}

begin
  Application.Initialize;
  Application.CreateForm(TDemoMainForm, DemoMainForm);
  Application.Run;
end.

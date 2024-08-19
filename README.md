# MajdataPlay
 A Simai Player.
 
 This project is based on [@LeZi9916](https://github.com/LeZi9916) 's DJauto branch for MajdataView.

 
## How to use
By default, the app use SoundBackend=2 for unity audio, this provide somehow acceptable latency.
However, for best performance, you will need an asio driver. [ASIO4ALL](https://asio4all.org/about/download-asio4all/) is a common choice.

If you encounter desync issues, try to tweak the sound control pannel: Turn off all audio enhancements and allow exclusive control.

For the big iPod, you need to ensure the touch sensors are connected to COM3.

Put your songs in `MaiCharts\` folder, and you are good to go.

## Adjusting settings
Navigate to `settings.json`

```Json
{
  "TapSpeed": 7.5,
  "TouchSpeed": 7.5,
  "BackgroundDim": 0.8, //0-1 bigger dimmer
  "AudioOffset": 0.0, //in seconds. + is late. same as &first
  "DisplayOffset": 0.0, //in seconds. + is late
  "lastSelectedSongIndex": 0,
  "lastSelectedSongDifficulty": 0,
  "AsioDeviceIndex": 0, // If you have multiple ASIO devices you can choose them here
  "DisplaySensorDebug": false, // this will display sensor feedback
  "VolumeAnwser": 0.8,
  "VolumeBgm": 1.0,
  "VolumeJudge": 0.3,
  "VolumeSlide": 0.3,
  "VolumeBreak": 0.3,
  "VolumeTouch": 0.3,
  "VolumeVoice": 1.0,
  "SoundBackend": 2, // 0 = WaveOut(High Latency), 1 = ASIO(Low Latency, Driver needed), 2 = (Unity Classic, FMod i think?)
  "SoundOutputSamplerate": 44100 //Dont touch this if you dont know what does it mean
}
```

## Keybindings
* Buttons: qwedcxza
* Exit Song: * (One of the side button on your big iPod)

## Adjusting Skin/SFX/Voice/MovieBG
Navigate to `MajdataPlay_Data\StreamingAssets\` and replace the files you want.

## Reporting Problems
Note this is project is still in a very early stage.
Feel free if you wanna participate in coding or testing!!
Please report problems to issues page.

The log files should be in `C:\Users\YOUR_USERNAME\AppData\LocalLow\bbben\MajdataPlay\Player.log`

## Note
Please don't ask about mobile porting, unless you wanna do it yourself.

This software has no affair with the `big S four letter` company, please support the arcade whenever you can.

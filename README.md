# MajdataPlay
 A Simai Player

## How to use
For best performance, you will need an asio driver. [ASIO4ALL](https://asio4all.org/about/download-asio4all/) is a common choice.
For the big iPod, you need to ensure the touch sensors is in COM3.
Put your songs in Songs/ folder, and you are good to go

## Adjusting settings
Navigate to `MajdataPlay_Data\StreamingAssets\settings.json`

```Json
{
  "TapSpeed": 7.5,
  "TouchSpeed": 7.5,
  "BackgroundDim": 0.8, //0-1 bigger dimmer
  "AudioOffset": 0.0, //in seconds. + is late. same as &first
  "DisplayOffset": 0.0, //in seconds. + is late
  "lastSelectedSongIndex": 0,
  "lastSelectedSongDifficulty": 6,
  "AsioDeviceIndex": 0, // If you have multiple ASIO devices you can choose them here
  "SoundBackend": 1, // 0 = WaveOut(High Latency), 1 = ASIO(Low Latency, Driver needed), 2 = (Unity Classic, FMod i think?)
  "SoundOutputSamplerate": 44100 //Dont touch this if you dont know what does it mean
}
```

## Adjusting Skin and SFX
Navigate to `MajdataPlay_Data\StreamingAssets\` and replace the files you want.

## Reporting Problems
The log files should be in `C:\Users\YOUR_USERNAME\AppData\LocalLow\bbben\MajdataPlay\Player.log`

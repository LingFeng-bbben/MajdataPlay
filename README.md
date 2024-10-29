# MajdataPlay

 A Simai Player.

 This project is based on [@LeZi9916](https://github.com/LeZi9916) 's DJAuto branch for MajdataView.

## How to use

For the big iPod, you need to ensure the touch sensors are connected to COM3, and the lights are connected to COM21.

Put your songs folder in `MaiCharts\` folder, and you are good to go. You will need to group your songs by folder.

If you are not satisfied with audio latency, you can use an asio driver. [ASIO4ALL](https://asio4all.org/about/download-asio4all/) is a common choice.

If you encounter desync issues, try to tweak the sound control pannel: Turn off all audio enhancements and allow exclusive control.

## Online Charts

The api endpoints fills in by default now. If you have majnet account, you can login by modify `setting.json` to enable chart reaction.

## Adjusting settings

~Navigate to `settings.json`~

You can use the in-game UI for most settings now.

```Json
{
  "Game": {
    "TapSpeed": 7.5,
    "TouchSpeed": 7.5,
    "SlideFadeInOffset": 0, // in seconds. this will advance or delay the timing of the Slide fade-in. + is delay
    "BackgroundDim": 0.8,
    "StarRotation": true,
    "Language": "ja-JP - Majdata",
    "BGInfo": "Combo"
  },
  "Judge": {
    "AudioOffset": 0,
    "JudgeOffset": 0,
    "Mode": "Modern"
  },
  "Display": {
    "Skin": "default",
    "DisplayCriticalPerfect": false,
    "FastLateType": "Disable",
    "NoteJudgeType": "All",
    "TouchJudgeType": "All",
    "SlideJudgeType": "All",
    "BreakJudgeType": "All",
    "BreakFastLateType": "Disable",
    "SlideSortOrder": "Modern",
    "OuterJudgeDistance": 1,
    "InnerJudgeDistance": 1,
    "Resolution": "Auto"
  },
  "Audio": {
    "Samplerate": 44100,
    "AsioDeviceIndex": 0,
    //Select your ASIO sound card here
    "Volume": {
      "Answer": 0.8,
      "BGM": 1,
      "Tap": 0.3,
      "Slide": 0.3,
      "Break": 0.3,
      "Touch": 0.3,
      "Voice": 1
    },
    "Backend": "Wasapi"
    //"WaveOut" (NAudio), "Asio" (NAudio), "Unity", "Wasapi" (Bass), "BassAsio" (Bass)
  },
  "Debug": {
    "DisplaySensor": false,
    "DisplayFPS": true,
    "FullScreen": true,
    "TryFixAudioSync": false,
    "NoteAppearRate": 0.36, 
    //Important! this affects the note fade in speed before it drops!!
    "DisableGCInGameing": true
  },
  "Online": {
    "Enable": false,
    "ApiEndpoints": [
      {
        "Name": "Majnet",
        "Url": "https://majdata.net/api3/api",
        "Username": "YourUsername",
        "Password": "YourPassword"
      },
      {
        "Name": "Contest",
        "Url": "https://majdata.net/api1/api",
        "Username": null,
        "Password": null
      }
    ]
  },
  "Misc": {
    "SelectedIndex": 0,
    "SelectedDir": 0,
    "SelectedDiff": "Easy",
    "OrderBy": {
      "Keyword": "",
      "SortBy": "Default"
    }
  }
}
```

## Keybindings

* Buttons: QWEDCXZA
* Exit Song: Num* (One of the side button on your big iPod)

## Custom Adjusting

### Skin

Your skin can be grouped by folder, adjust `Skin` field in `settings.json` to use skin what you want (After Alpha1.0 )

Navigate to `Skins\` and replace the files you want.

### SFX/Voice/MovieBG

Navigate to `MajdataPlay_Data\StreamingAssets\` and replace the files you want.

## Reporting Problems

Note this is project is still in a very early stage. Feel free if you wanna participate in coding or testing!!

Please report problems to issues page.

The log files should be in `Logs/`

## Note

Please don't ask about mobile porting, unless you wanna do it yourself.

This software has no affair with the `big S four letter` company, please support the arcade whenever you can.

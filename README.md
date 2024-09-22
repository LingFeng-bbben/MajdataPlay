# MajdataPlay

 A Simai Player.

 This project is based on [@LeZi9916](https://github.com/LeZi9916) 's DJAuto branch for MajdataView.

## How to use

By default, the app use unity audio, this provide somehow acceptable latency.

However, for best performance, you will need an asio driver. [ASIO4ALL](https://asio4all.org/about/download-asio4all/) is a common choice.

If you encounter desync issues, try to tweak the sound control pannel: Turn off all audio enhancements and allow exclusive control.

For the big iPod, you need to ensure the touch sensors are connected to COM3, and the lights are connected to COM21.

Put your songs folder in `MaiCharts\` folder, and you are good to go. You will need to group your songs by folder.

## Online Charts

Fill in the api endpoint for downloading charts online. More online functions comming soon.

## Adjusting settings

~Navigate to `settings.json`~

You can use the in-game UI for most settings now.

```Json
{
  "Game": {
    "TapSpeed": 7.5,
    "TouchSpeed": 7.5,
    "SlideFadeInOffset": 0,       // in seconds. this will advance or delay the timing of the Slide fade-in. + is delay
    "BackgroundDim": 0.800000012, // 0-1 bigger dimmer
    "StarRotation": false,
    "BGInfo": "Combo" // what the center will display in game
    // options:
    // Combo
    // PCombo
    // CPCombo
    // Achievement_101
    // Achievement_100
    // Achievement
    // AchievementClassical
    // AchievementClassical_100
    // DXScore
    // DXScoreRank
    // S_Board
    // SS_Board
    // SSS_Board
    // MyBest
    // Diff
    // None
  },
  "Judge": {
    "AudioOffset": 0, // in seconds. + is late. same as &first
    "JudgeOffset": 0, // in seconds. + is late. influence judge
    "Mode": "Modern"  // judge mode, options: "Modern" or "Classic"
  },
  "Display": {
    "Skin": "default", // the subdirectory name under "Skins"
    "DisplayCriticalPerfect": false,
    "FastLateType": "Disable", // options: All, BelowCP, BelowP, BelowGR, Disable
    "NoteJudgeType": "All",    // ditto
    "TouchJudgeType": "All",   // ditto
    "SlideJudgeType": "All",   // ditto
    "OuterJudgeDistance": 1,   // adjust the value to control where the judge result is displayed
                               // options: 1 - 0
                               // influence: Tap, Hold, Star, Break
    "InnerJudgeDistance": 1,   // adjust the value to control where the judge result is displayed
                               // options: 1 - 0
                               // influence: Touch, TouchHold
    "Resolution": "Auto"  // Screen Resolution
                          // format: "width x height" or "Auto"
                          // e.g. "1080x1920" 
  },  
  "Audio": {
    "Samplerate": 44100,  // Dont touch this if you dont know what does it mean
    "AsioDeviceIndex": 0, // If you have multiple ASIO devices you can choose them here
    "Volume": {
      "Anwser": 0.800000012,
      "BGM": 1,
      "Judge": 0.300000012,
      "Slide": 0.300000012,
      "Break": 0.300000012,
      "Touch": 0.300000012,
      "Voice": 1
    },
    "Backend": "Asio" // WaveOut(High Latency), Asio(Low Latency, Driver needed), Unity(Unity Classic, FMod i think?)
  },
  "Debug": {
    "DisplaySensor": false, // this will display sensor feedback
    "DisplayFPS": true,     // this will display FPS at the top right of the screen
    "FullScreen": true,     // MajdataPlay will be windowed if this option is false
    "TryFixAudioSync": true,
    "NoteAppearRate": 0.360000014
  },
  "SelectedIndex": 0,    // dont touch it
  "SelectedDiff": "Easy" // dont touch it
}
```

## Keybindings

* Buttons: QWEDCXZA
* Exit Song: * (One of the side button on your big iPod)

## Custom Adjusting

### Skin

Your skin can be grouped by folder, adjust `Skin` field in `settings.json` to use skin what you want (After Alpha1.0 )

Navigate to `Skins\` and replace the files you want.

### SFX/Voice/MovieBG

Navigate to `MajdataPlay_Data\StreamingAssets\` and replace the files you want.

## Reporting Problems

Note this is project is still in a very early stage. Feel free if you wanna participate in coding or testing!!

Please report problems to issues page.

The log files should be in `C:\Users\YOUR_USERNAME\AppData\LocalLow\bbben\MajdataPlay\Player.log`

## Note

Please don't ask about mobile porting, unless you wanna do it yourself.

This software has no affair with the `big S four letter` company, please support the arcade whenever you can.

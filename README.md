# MajdataPlay

![license GPL-3.0](https://img.shields.io/badge/license-GPL--3.0-blue)
![GitHub Release](https://img.shields.io/github/v/release/LingFeng-bbben/MajdataPlay?include_prereleases)
![Discord](https://badgen.net/discord/online-members/AcWgZN7j6K)

 A Simai Player.

 This project is based on [@LeZi9916](https://github.com/LeZi9916) 's DJAuto branch for [MajdataView](https://github.com/LingFeng-bbben/MajdataView).
 
 Simai is a maimai chart discription language developed by [Celeca](https://twitter.com/formiku39854)
 
## How to use

For the big iPod, you need to ensure the touch sensors are connected to COM3, and the lights are connected to COM21.

Group your songs by folder and put them in `MaiCharts\`, and you are good to go.

If you are not satisfied with audio latency, you can use an asio driver. [ASIO4ALL](https://asio4all.org/about/download-asio4all/) is a common choice.

If you encounter desync issues, try to tweak the sound control pannel: Turn off all audio enhancements and allow exclusive control.

## Online Charts

The api endpoints fills in by default now. 

Change `Online` to `true` in `setting.json` if you want to pull online charts from [Majnet](https://majdata.net). 

If you have majnet account, you can login by modify `setting.json` to enable chart reaction.

## Adjusting settings

You can use the in-game UI for most settings now.

## Keybindings

* Buttons: QWEDCXZA
* Exit Song: Num* (First side button 🔺 on your big iPod)

## Custom Adjusting

### Skin

Your skin can be grouped by folder, adjust `Skin` field in `settings.json` to use skin what you want (After Alpha1.0 )

Navigate to `Skins\` and replace the files you want.

### SFX/Voice/MovieBG

Navigate to `MajdataPlay_Data\StreamingAssets\` and replace the files you want.

You can find and replace the Allperfect and FullCombo animation in it as well.

## Reporting Problems

Note this is project is still in a very early stage. Feel free if you wanna participate in coding or testing!!

Please report problems to issues page.

The log files should be in `Logs/`

## Note

Please don't ask about mobile porting, unless you wanna do it yourself.

This software has no affair with the `big S four letter` company, please support the arcade whenever you can.

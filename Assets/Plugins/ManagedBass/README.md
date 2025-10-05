<img width="128" height="128" alt="mb-logo-rounded-128" src="https://github.com/user-attachments/assets/834c571c-6fba-4d6c-a203-9774a456424e" />

# ManagedBass
[![NuGet](https://img.shields.io/nuget/v/ManagedBass.svg)](https://www.nuget.org/packages/ManagedBass/) [![NuGet Downloads](https://img.shields.io/nuget/dt/ManagedBass.svg)](https://www.nuget.org/packages/ManagedBass/) [![Build](https://github.com/ManagedBass/ManagedBass/actions/workflows/build-test.yml/badge.svg)](https://github.com/ManagedBass/ManagedBass/actions/workflows/build-test.yml)

Free Open-Source Cross-Platform .Net Wrapper for [Un4seen Bass audio library](https://www.un4seen.com)   and its AddOns.

## Bass Audio Library
Summary from Un4Seen Bass website:
> BASS is an audio library for use in software on several platforms. Its purpose is to provide developers with powerful and efficient sample, stream (MP3, MP2, MP1, OGG, WAV, AIFF, custom generated, and more via OS codecs and add-ons), MOD music (XM, IT, S3M, MOD, MTM, UMX), MO3 music (MP3/OGG compressed MODs), and recording functions. All in a compact DLL that won't bloat your distribution.
> C/C++, Delphi, and Visual Basic APIs are provided, with several examples to get you started. .NET and other APIs are also available.


**IMPORTANT**: Bass and its Add-Ons can be downloaded at http://un4seen.com/. This project provides a wrapper for the C-based library.    

ManagedBass is targeted for **Any CPU**, but bass Libraries(.dll/.so/.dylib/.a) are separate for x86, x64, ARM, etc.  
Download the versions you need directly from Un4Seen's website.

See the [Sample Repositories](https://github.com/ManagedBass) for examples.

See [Un4Seen Bass documentation for full details](https://www.un4seen.com/doc/). This documentation is for their c-based library, which this project provides an almost 1:1 wrapper for.  

ManagedBass is now provided as a set of packages split per AddOn.

----
### Supporting the project

ManagedBass is developed and supported by [@olitee](https://github.com/olitee) and [@DustinBond](https://github.com/DustinBond) in their spare time for free. The project was originally created by [@MathewSachin](https://github.com/MathewSachin). Contributions are very welcome, but please be patient.

----

## Getting Started

* Install the NuGet package
```powershell
Install-Package ManagedBass
```

* Download the BASS libraries from http://un4seen.com and place them in Build Output Directory.

## Supported Frameworks and Platforms
Our goal is to support all platforms that Un4seen Bass supports.  
The nuget package contains support for the following targets:
- .NET Framework 4.5+
- .NET Standard 2.0+
- .NET 8.0+

with the following platforms:
- Windows
- Linux
- MacOS
- Android
- iOS**

** NOTE: This library no longer support iOS prior to version 8, where it was necessary for the native Bass libraries to be statically linked. iOS 8 and later support dynamic linking, so you no longer need a dedicated version of our library to support iOS.

# AddOns
AddOns add more functionality to BASS.

AddOns may need to be loaded into memory before using with BASS. This happens automatically if you call any method on the AddOn class. But, in cases where you use the features of an AddOn directly through BASS, you may need to load it explicitly. This can be done in the best way by calling the `Version` member which is least resource intensive, or you can call any method like `CreateStream` with invalid parameters.

## Plugin AddOns
A Plugin plugs into standard BASS functions like sample or stream creation to provide support for more audio formats.
BASS has built in support for various audio codecs like MPEG, OGG, WAV, AIFF, etc.
A Plugin is loaded using `Bass.PluginLoad` method and unloaded using `Bass.PluginFree`.

## Platform Availability
AddOn        | Is Plugin          | Windows            | Linux              | Mac                | Android            | iOS                | WindowsStore
-------------|--------------------|--------------------|--------------------|--------------------|--------------------|--------------------|------------------
BassAac      | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |                    | :heavy_check_mark: |                    |
BassAc3      | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |                    |                    |
BassAdx      | :heavy_check_mark: | :heavy_check_mark: |                    |                    |                    |                    |
BassAix      | :heavy_check_mark: | :heavy_check_mark: |                    |                    |                    |                    |
BassAlac     | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |                    | :heavy_check_mark: |                    |
BassApe      | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassAsio     |                    | :heavy_check_mark: |                    |                    |                    |                    |
BassCd       | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |                    |                    |                    |
BassDsd      | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassDShow    |                    | :heavy_check_mark: |                    |                    |                    |                    |
BassEnc      |                    | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassEnc_Ogg  |                    | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassEnc_Opus |                    | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassFlac     | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassFx       |                    | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark:
BassHls      | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassMidi     | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark:
BassMix      |                    | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark:
BassMpc      | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassOfr      | :heavy_check_mark: | :heavy_check_mark: |                    |                    |                    |                    |
BassOpus     | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassSfx      |                    | :heavy_check_mark: |                    |                    |                    |                    |
BassSpx      | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |                    |                    |
BassTags     |                    | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassTta      | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassVst      |                    | :heavy_check_mark: |                    |                    |                    |                    |
BassWA       |                    | :heavy_check_mark: |                    |                    |                    |                    |
BassWaDsp    |                    | :heavy_check_mark: |                    |                    |                    |                    |
BassWasapi   |                    | :heavy_check_mark: |                    |                    |                    |                    |
BassWinamp   |                    | :heavy_check_mark: |                    |                    |                    |                    |
BassWma      | :heavy_check_mark: | :heavy_check_mark: |                    |                    |                    |                    |
BassWv       | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
BassZXTune   | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |                    |

## Changelog
See our [releases page](https://github.com/ManagedBass/ManagedBass/releases)

## 

using System.Runtime.InteropServices;

namespace ManagedBass.Midi
{
    /// <summary>
    /// Wraps BassMidi: bassmidi.dll
    /// 
    /// <para>Supports: .midi, .mid, .rmi, .kar</para>
    /// </summary>
    public static partial class BassMidi
    {
        /// <summary>
        /// Automatically compact all soundfonts following a configuration change?
        /// compact (bool): If true, all soundfonts are compacted following a MIDI stream being freed, or a <see cref="StreamSetFonts(int,MidiFont[],int)"/> call.
        /// The compacting isn't performed immediately upon a MIDI stream being freed or <see cref="StreamSetFonts(int,MidiFont[],int)"/> being called.
        /// It's actually done 2 seconds later (in another thread), 
        /// so that if another MIDI stream starts using the soundfonts in the meantime, they aren't needlessly closed and reopened.
        /// Samples that have been preloaded by <see cref="FontLoad"/> are not affected by automatic compacting.
        /// Other samples that have been preloaded by <see cref="StreamLoadSamples"/> are affected though,
        /// so it is probably wise to disable this option when using that function.
        /// By default, this option is enabled.
        /// </summary>
        public static bool Compact
        {
            get => Bass.GetConfigBool(Configuration.MidiCompact);
            set => Bass.Configure(Configuration.MidiCompact, value);
        }

        /// <summary>
        /// Automatically load matching soundfonts?
        /// If set to 1 (default), BASSMIDI will try to load a soundfont matching the MIDI file. If set to 2, the matching soundfont will also be used on all banks. 
        /// </summary>
        public static int AutoFont
        {
            get => Bass.GetConfig(Configuration.MidiAutoFont);
            set => Bass.Configure(Configuration.MidiAutoFont, value);
        }

        /// <summary>
        /// The maximum number of samples to play at a time (polyphony).
        /// voices (int): Maximum number of samples to play at a time... 1 (min) - 1000 (max).
        /// This setting determines the maximum number of samples that can play together in a single MIDI stream. 
        /// This isn't necessarily the same thing as the maximum number of notes, due to presets often layering multiple samples. 
        /// When there are no voices available to play a new sample, the voice with the lowest volume will be killed to make way for it.
        /// The more voices that are used, the more CPU that is required. 
        /// So this option can be used to restrict that, for example on a less powerful system. 
        /// The CPU usage of a MIDI stream can also be restricted via the <see cref="ChannelAttribute.MidiCPU"/> attribute.
        /// Changing this setting only affects subsequently created MIDI streams, not any that have already been created. 
        /// The default setting is 128 voices.
        /// Platform-specific
        /// The default setting is 100, except on iOS, where it is 40.
        /// </summary>
        public static int Voices
        {
            get => Bass.GetConfig(Configuration.MidiVoices);
            set => Bass.Configure(Configuration.MidiVoices, value);
        }

        /// <summary>
        /// The number of MIDI Input ports to make available (Linux Only).
        /// ports (int): Number of Input ports... 0 (min) - 10 (max).
        /// MIDI Input ports allow MIDI data to be received from other software, not only MIDI devices. 
        /// Once a port has been initialized via <see cref="InInit"/>, the ALSA client and port IDs can be retrieved from <see cref="InGetDeviceInfo(int, out MidiDeviceInfo)"/>,
        /// which other software can use to connect to the port and send data to it.
        /// Prior to initialization, an Input port will have a client ID of 0.
        /// The default is for 1 Input port to be available. 
        /// Note: This option is only available on Linux.
        /// </summary>
        public static int InputPorts
        {
            get => Bass.GetConfig(Configuration.MidiInputPorts);
            set => Bass.Configure(Configuration.MidiInputPorts, value);
        }

        /// <summary>
        /// Default soundfont usage
        /// filename (string): Filename of the default soundfont to use (null = no default soundfont).
        /// If the specified soundfont cannot be loaded, the default soundfont setting will remain as it is.
        /// On Windows, the default is to use one of the Creative soundfonts (28MBGM.SF2 or CT8MGM.SF2 or CT4MGM.SF2 or CT2MGM.SF2),
        /// if present in the windows system directory.
        /// </summary>
        public static string DefaultFont
        {
            get => Marshal.PtrToStringAnsi(Bass.GetConfigPtr(Configuration.MidiDefaultFont));
            set
            {
                var ptr = Marshal.StringToHGlobalAnsi(value);

                Bass.Configure(Configuration.MidiDefaultFont, ptr);

                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
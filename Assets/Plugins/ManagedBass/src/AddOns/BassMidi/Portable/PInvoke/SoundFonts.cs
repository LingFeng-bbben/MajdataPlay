using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Midi
{
    public static partial class BassMidi
    {
		/// <summary>
		/// Compact a soundfont's memory usage.
		/// </summary>
		/// <param name="Handle">The soundfont to get info on (e.g. as returned by <see cref="FontInit(string,FontInitFlags)" />)... 0 = all soundfonts.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
        /// Compacting involves freeing any samples that are currently loaded but unused.
        /// The amount of sample data currently loaded can be retrieved using <see cref="FontGetInfo(int, out MidiFontInfo)" />.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_FontCompact")]
        public static extern bool FontCompact(int Handle);
        
		/// <summary>
		/// Frees a soundfont.
		/// </summary>
		/// <param name="Handle">The soundfont handle to free (e.g. as returned by <see cref="FontInit(string,FontInitFlags)" />).</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>When a soundfont is freed, it is automatically removed from any MIDI streams that are using it.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_FontFree")]
        public static extern bool FontFree(int Handle);

        /// <summary>
        /// Retrieves information on a soundfont.
        /// </summary>
        /// <param name="Handle">The soundfont to get info on (e.g. as returned by <see cref="FontInit(string,FontInitFlags)" />).</param>
        /// <param name="Info">An instance of <see cref="MidiFontInfo"/> structure to store the information into.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_FontGetInfo")]
        public static extern bool FontGetInfo(int Handle, out MidiFontInfo Info);

        /// <summary>
        /// Retrieves information on a soundfont.
        /// </summary>
        /// <param name="Handle">The soundfont to get info on (e.g. as returned by <see cref="FontInit(string,FontInitFlags)" />).</param>
        /// <returns>An instance of <see cref="MidiFontInfo"/> structure is returned. Throws <see cref="BassException"/> on Error.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        public static MidiFontInfo FontGetInfo(int Handle)
        {
            if (!FontGetInfo(Handle, out var info))
                throw new BassException();
            return info;
        }

        [DllImport(DllName)]
        static extern IntPtr BASS_MIDI_FontGetPreset(int handle, int preset, int bank);

		/// <summary>
        /// Retrieves the name of a preset in a soundfont.
		/// </summary>
		/// <param name="Handle">The soundfont handle to get the preset name from.</param>
		/// <param name="Preset">Preset number to load... -1 = all presets (the first encountered).</param>
		/// <param name="Bank">Bank number to load... -1 = all banks (the first encountered).</param>
		/// <returns>If successful, the requested preset name is returned, else <see langword="null" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The soundfont does not contain the requested preset.</exception>
        public static string FontGetPreset(int Handle, int Preset, int Bank)
        {
            return Marshal.PtrToStringAnsi(BASS_MIDI_FontGetPreset(Handle, Preset, Bank));
        }
        
		/// <summary>
		/// Retrieves the presets in a soundfont.
		/// </summary>
		/// <param name="Handle">The soundfont handle to get the preset name from.</param>
        /// <param name="Presets">The array to receive the presets.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>The presets are delivered with the preset number in the LOWORD and the bank number in the HIWORD, and in numerically ascending order.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_FontGetPresets")]
        public static extern bool FontGetPresets(int Handle, [In, Out] int[] Presets);
                
		/// <summary>
		/// Retrieves the presets in a soundfont.
		/// </summary>
		/// <param name="Handle">The soundfont handle to get the preset name from.</param>
		/// <returns>If successful, an array of presets is returned, else <see langword="null" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>The presets are delivered with the preset number in the LOWORD and the bank number in the HIWORD, and in numerically ascending order.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        public static int[] FontGetPresets(int Handle)
        {
            if (!FontGetInfo(Handle, out var info))
                return null;

            var ret = new int[info.Presets];
            
            return FontGetPresets(Handle, ret) ? ret : null;
        }
        
		/// <summary>
		/// Retrieves a soundfont's volume level.
		/// </summary>
		/// <param name="Handle">The soundfont to get the volume of.</param>
		/// <returns>If successful, the soundfont's volume level is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_FontGetVolume")]
        public static extern float FontGetVolume(int Handle);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_MIDI_FontInit(string File, FontInitFlags flags);
        
        /// <summary>
        /// Initializes a soundfont from a file (unicode).
        /// </summary>
        /// <param name="File">The file name of the sound font (e.g. an .sf2 file).</param>
        /// <param name="Flags">Any combination of <see cref="FontInitFlags"/>.</param>
        /// <returns>If successful, the soundfont's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// BASSMIDI uses SF2 and/or SFZ soundfonts to provide the sounds to use in the rendering of MIDI files.
        /// Several soundfonts can be found on the internet, including a couple on the BASS website.
        /// </para>
        /// <para>
        /// A soundfont needs to be initialized before it can be used to render MIDI streams.
        /// Once initialized, a soundfont can be assigned to MIDI streams using the <see cref="StreamSetFonts(int,MidiFont[],int)" /> function.
        /// A single soundfont can be shared by multiple MIDI streams.
        /// </para>
        /// <para>Information on the initialized soundfont can be retrieved using <see cref="FontGetInfo(int,out MidiFontInfo)" />.</para>
        /// <para>If a soundfont is initialized multiple times, each instance will have its own handle but share the same sample/etc data.</para>
        /// <para>
        /// Soundfonts use PCM sample data as standard, but BASSMIDI can accept any format that is supported by BASS or its add-ons.
        /// The FontPack function can be used to compress the sample data in SF2 files.
        /// SFZ samples are in separate files and can be compressed using standard encoding tools.
        /// </para>
        /// <para>Using soundfonts that are located somewhere other than the file system is possible via <see cref="FontInit(FileProcedures,IntPtr,BassFlags)" />.</para>
        /// <para><b>SFZ support</b></para>
        /// <para>
        /// The following SFZ opcodes are supported: ampeg_attack, ampeg_decay, ampeg_delay, ampeg_hold, ampeg_release, ampeg_sustain, ampeg_vel2attack, ampeg_vel2decay, amplfo_delay/fillfo_delay/pitchlfo_delay, amplfo_depth, amplfo_freq/fillfo_freq/pitchlfo_freq, amp_veltrack, cutoff, effect1, effect2, end, fileg_attack/pitcheg_attack, fileg_decay/pitcheg_decay, fileg_delay/pitcheg_delay, fileg_depth, fileg_hold/pitcheg_hold, fileg_release/pitcheg_release, fileg_sustain/pitcheg_sustain, fileg_vel2depth, fillfo_depth, fil_veltrack, group, hikey, hivel, key, lokey, loop_end, loop_mode, loop_start, lovel, offset, off_by, pan, pitcheg_depth, pitchlfo_depth, pitch_keycenter, pitch_keytrack, pitch_veltrack, resonance, sample, seq_length, seq_position, transpose, tune, volume. 
        /// The fil_type opcode is also supported, but only to confirm that a low pass filter is wanted (the filter will be disabled otherwise). 
        /// The combined EG and LFO entries in the opcode list reflect that there is a shared EG for pitch/filter and a shared LFO for amplitude/pitch/filter, as is the case in SF2. 
        /// Information on these (and other) SFZ opcodes can be found at www.sfzformat.com.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>The <see cref="BassFlags.MidiFontMemoryMap"/> option is not available on big-endian systems (eg. PowerPC) as a soundfont's little-endian sample data cannot be played directly from a mapping; its byte order needs to be reversed.</para>
        /// </remarks>
        /// <exception cref="Errors.FileOpen">The <paramref name="File" /> could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The file's format is not recognised/supported.</exception>
        public static int FontInit(string File, FontInitFlags Flags) => BASS_MIDI_FontInit(File, Flags | FontInitFlags.Unicode);

        /// <summary>
        /// Initializes a soundfont via user callback functions.
        /// </summary>
        /// <param name="Procedures">The user defined file function (see <see cref="FileProcedures" />).</param>
        /// <param name="User">User instance data to pass to the callback functions.</param>
        /// <param name="Flags">Unused.</param>
        /// <returns>If successful, the soundfont's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>The unbuffered file system (<see cref="StreamSystem.NoBuffer"/>) is always used by this function.</remarks>
        /// <exception cref="Errors.FileFormat">The file's format is not recognised/supported.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_FontInitUser")]
        public static extern int FontInit([In, Out] FileProcedures Procedures, IntPtr User, FontInitFlags Flags);

        /// <summary>
        /// Preloads presets from a soundfont.
        /// </summary>
        /// <param name="Handle">The soundfont handle.</param>
        /// <param name="Preset">Preset number to load... -1 = all presets.</param>
        /// <param name="Bank">Bank number to load... -1 = all banks.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// Samples are normally loaded as they are needed while rendering a MIDI stream, which can result in CPU spikes, particularly with packed soundfonts.
        /// That generally won't cause any problems, but when smooth/constant performance is critical this function can be used to preload the samples before rendering, so avoiding the need to load them while rendering.
        /// </para>
        /// <para>When preloading samples to render a particular MIDI stream, it is more efficient to use <see cref="StreamLoadSamples" /> to preload the specific samples that the MIDI stream will use, rather than preloading the entire soundfont.</para>
        /// <para>Samples that are preloaded by this function are not affected by automatic compacting via the <see cref="Compact" /> option, but can be compacted/unloaded manually with <see cref="FontCompact" />.</para>
        /// <para>A soundfont should not be preloaded while it's being used to render any MIDI streams, as that could delay the rendering.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Codec">The appropriate add-on to decode the samples is not loaded.</exception>
        /// <exception cref="Errors.NotAvailable">The soundfont does not contain the requested preset.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_FontLoad")]
        public static extern bool FontLoad(int Handle, int Preset, int Bank);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern bool BASS_MIDI_FontPack(int handle, string outfile, string encoder, BassFlags flags);

        /// <summary>
        /// Produces a compressed version of a soundfont.
        /// </summary>
        /// <param name="Handle">The soundfont to pack.</param>
        /// <param name="OutFile">Filename for the packed soundfont.</param>
        /// <param name="Encoder">Encoder command-line.</param>
        /// <param name="Flags">Any combination of <see cref="BassFlags.MidiNoHeader"/> and <see cref="BassFlags.Midi16Bit"/>.</param>
        /// <returns>If successful, the <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Standard soundfonts use PCM samples, so they can be quite large, which can be a problem if they're to be distributed. 
        /// To reduce the size, BASSMIDI can compress the samples using any command-line encoder with STDIN and STDOUT support. 
        /// Packed soundfonts can be used for rendering by BASSMIDI just like normal soundfonts. 
        /// They can also be unpacked using <see cref="FontUnpack" />.
        /// <para>
        /// Although any command-line encoder can be used, it is best to use a lossless format like FLAC or WavPack, rather than a lossy one like OGG or MP3. 
        /// Using a lossless encoder, the packed soundfont will produce exactly the same results as the original soundfont, and will be identical to the original when unpacked. 
        /// As a compromise between quality and size, the WavPack hybrid/lossy mode also produces good sounding results.
        /// </para>
        /// <para>The encoder must be told (via the command-line) to expect input from STDIN and to send it's output to STDOUT.</para>
        /// <para>
        /// Before using a packed soundfont, the appropriate BASS add-on needs to be loaded via <see cref="Bass.PluginLoad" />. 
        /// For example, if the samples are FLAC encoded, BASSFLAC would need to be loaded.
        /// During rendering, the samples are unpacked as they're needed, which could result in CPU spikes. 
        /// Where smooth performance is critical, it may be wise to preload the samples using <see cref="FontLoad" /> or <see cref="StreamLoadSamples" />.
        /// </para>
        /// <para>
        /// A soundfont should not be packed while it is being used to render any MIDI streams, as that could delay the rendering. 
        /// This function only applies to SF2 soundfonts.
        /// SFZ samples can be compressed using standard encoding tools.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>This function is not available on iOS or Android.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.FileOpen">Couldn't start the encoder. Check that the executable exists.</exception>
        /// <exception cref="Errors.Create">Couldn't create the output file, <paramref name="OutFile" />.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool FontPack(int Handle, string OutFile, string Encoder, BassFlags Flags)
        {
            return BASS_MIDI_FontPack(Handle, OutFile, Encoder, Flags | BassFlags.Unicode);
        }

        /// <summary>
        /// Sets a soundfont's volume level.
        /// </summary>
        /// <param name="Handle">The soundfont to set the volume of.</param>
        /// <param name="Volume">The volume level... 0=silent, 1.0=normal/default.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// By default, some soundfonts may be louder than others, which could be a problem when mixing multiple soundfonts. 
        /// This function can be used to compensate for any differences, by raising the level of the quieter soundfonts or lowering the louder ones.
        /// <para>Changes take immediate effect in any MIDI streams that are using the soundfont.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_FontSetVolume")]
        public static extern bool FontSetVolume(int Handle, float Volume);
        
		/// <summary>
		/// Unloads presets from a soundfont.
		/// </summary>
		/// <param name="Handle">The soundfont handle.</param>
		/// <param name="Preset">Preset number to load... -1 = all presets.</param>
		/// <param name="Bank">Bank number to load... -1 = all banks.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
        /// An unloaded preset will be loaded again when needed by a MIDI stream.
        /// Any samples that are currently being used by a MIDI stream will not be unloaded.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The soundfont does not contain the specified preset, or the soundfont is memory mapped.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_FontUnload")]
        public static extern bool FontUnload(int Handle, int Preset, int Bank);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern bool BASS_MIDI_FontUnpack(int handle, string outfile, BassFlags flags = BassFlags.Unicode);
        
        /// <summary>
        /// Produces a decompressed version of a packed soundfont.
        /// </summary>
        /// <param name="Handle">The soundfont to unpack.</param>
        /// <param name="OutFile">Filename for the unpacked soundfont.</param>
        /// <returns>If successful, the <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// To unpack a soundfont, the appropriate BASS add-on needs to be loaded (via <see cref="Bass.PluginLoad" />) to decode the samples. 
        /// For example, if the samples are FLAC encoded, BASSFLAC would need to be loaded. 
        /// BASS also needs to have been initialized, using <see cref="Bass.Init" />. 
        /// For just unpacking a soundfont, the <see cref="Bass.NoSoundDevice"/> could be used.
        /// <para>
        /// A soundfont should not be unpacked while it is being used to render any MIDI streams, as that could delay the rendering.
        /// <see cref="FontGetInfo(int,out MidiFontInfo)" /> can be used to check if a soundfont is packed.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The soundfont is not packed.</exception>
        /// <exception cref="Errors.Init"><see cref="Bass.Init" /> has not been successfully called - it needs to be to decode the samples.</exception>
        /// <exception cref="Errors.Codec">The appropriate add-on to decode the samples is not loaded.</exception>
        /// <exception cref="Errors.Create">Couldn't create the output file, <paramref name="OutFile" />.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool FontUnpack(int Handle, string OutFile) => BASS_MIDI_FontUnpack(Handle, OutFile);
    }
}
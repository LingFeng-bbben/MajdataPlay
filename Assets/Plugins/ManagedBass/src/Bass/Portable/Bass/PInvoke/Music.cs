using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        /// <summary>
        /// Frees a MOD music's resources, including any sync/DSP/FX it has.
        /// </summary>
        /// <param name="Handle">The MOD music handle.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MusicFree")]
        public static extern bool MusicFree(int Handle);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_MusicLoad(bool mem, string file, long offset, int Length, BassFlags flags, int freq);

        [DllImport(DllName)]
        static extern int BASS_MusicLoad(bool mem, IntPtr file, long offset, int Length, BassFlags flags, int freq);

        /// <summary>
        /// Loads a MOD music file - MO3 / IT / XM / S3M / MTM / MOD / UMX formats.
        /// </summary>
        /// <param name="File">The file name from where to load the music.</param>
        /// <param name="Offset">File offset to load the MOD music from.</param>
        /// <param name="Length">Data length... 0 = use all data up to the end of file. If length over-runs the end of the file, it'll automatically be lowered to the end of the file.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <param name="Frequency">Sample rate to render/play the MOD music at... 0 = the rate specified in the <see cref="Init" /> call.</param>
        /// <returns>If successful, the loaded music's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>BASS uses the same code as XMPlay for it's MOD music support, giving the most accurate reproduction of MO3 / IT / XM / S3M / MTM / MOD / UMX files available from any sound system.</para>
        /// <para>
        /// MO3s are treated and used in exactly the same way as normal MOD musics.
        /// The advantage of MO3s is that they can be a lot smaller with virtually identical quality.
        /// Playing a MO3 does not use any more CPU power than playing the original MOD version does.
        /// The only difference is a slightly longer load time as the samples are being decoded.
        /// MO3 files are created using the MO3 encoder available at the BASS website.
        /// </para>
        /// <para>
        /// DMO effects (the same as available with <see cref="ChannelSetFX" />) can be used in IT and XM files (and MO3 versions of them) created with Modplug Tracker.
        /// This allows effects to be added to a track without having to resort to an MP3 or OGG version, so it can remain small and still sound fancy.
        /// Of course, the effects require some CPU, so should not be used carelessly if performance is key.
        /// DirectX 8 (or above) is required for the effects to be heard - without that, the music can still be played, but the effects are disabled.
        /// </para>
        /// <para>
        /// "Ramping" does not take a lot of extra processing and improves the sound quality by removing clicks, by ramping/smoothing volume and pan changes.
        /// The start of a sample may also be ramped-in.
        /// That is always the case with XM files (or MOD files in FT2 mode) when using normal ramping, and possibly with all formats when using sensitive ramping; senstitive ramping will only ramp-in when necessary to avoid a click.
        /// Generally, normal ramping is recommended for XM files, and sensitive ramping for the other formats, but some XM files may also sound better using sensitive ramping.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// DMO effects are not supported in MOD music on Windows CE, and DirectX 8 (or above) is required on Windows.
        /// They are always available on other platforms, except for the following: DXCompressor, DXGargle, and DX_I3DL2Reverb.
        /// When a DMO effect is unavailable, the MOD music can still be played, but the effect will be disabled.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">The <see cref="BassFlags.AutoFree"/> flag is unavailable to decoding channels.</exception>
        /// <exception cref="Errors.FileOpen">The <paramref name="File"/> could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The <paramref name="File"/>'s format is not recognised/supported.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the stream is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Speaker">The specified SPEAKER flags are invalid. The device/drivers do not support them, they are attempting to assign a stereo stream to a mono speaker or 3D functionality is enabled.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int MusicLoad(string File, long Offset = 0, int Length = 0, BassFlags Flags = BassFlags.Default, int Frequency = 0)
        {
            return BASS_MusicLoad(false, File, Offset, Length, Flags | BassFlags.Unicode, Frequency);
        }

        /// <summary>
        /// Loads a MOD music file - MO3 / IT / XM / S3M / MTM / MOD / UMX formats from memory.
        /// </summary>
        /// <param name="Memory">An unmanaged pointer to the memory location as an IntPtr.</param>
        /// <param name="Offset">Memory offset to load the MOD music from.</param>
        /// <param name="Length">Data length.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <param name="Frequency">Sample rate to render/play the MOD music at... 0 = the rate specified in the <see cref="Init" /> call.</param>
        /// <returns>If successful, the loaded music's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>BASS uses the same code as XMPlay for it's MOD music support, giving the most accurate reproduction of MO3 / IT / XM / S3M / MTM / MOD / UMX files available from any sound system.</para>
        /// <para>
        /// MO3s are treated and used in exactly the same way as normal MOD musics.
        /// The advantage of MO3s is that they can be a lot smaller with virtually identical quality.
        /// Playing a MO3 does not use any more CPU power than playing the original MOD version does.
        /// The only difference is a slightly longer load time as the samples are being decoded.
        /// MO3 files are created using the MO3 encoder available at the BASS website.
        /// </para>
        /// <para>
        /// DMO effects (the same as available with <see cref="ChannelSetFX" />) can be used in IT and XM files (and MO3 versions of them) created with Modplug Tracker.
        /// This allows effects to be added to a track without having to resort to an MP3 or OGG version, so it can remain small and still sound fancy.
        /// Of course, the effects require some CPU, so should not be used carelessly if performance is key.
        /// DirectX 8 (or above) is required for the effects to be heard - without that, the music can still be played, but the effects are disabled.
        /// </para>
        /// <para>
        /// "Ramping" does not take a lot of extra processing and improves the sound quality by removing clicks, by ramping/smoothing volume and pan changes.
        /// The start of a sample may also be ramped-in.
        /// That is always the case with XM files (or MOD files in FT2 mode) when using normal ramping, and possibly with all formats when using sensitive ramping; senstitive ramping will only ramp-in when necessary to avoid a click.
        /// Generally, normal ramping is recommended for XM files, and sensitive ramping for the other formats, but some XM files may also sound better using sensitive ramping.
        /// </para>
        /// <para>
        /// When loading a MOD music from memory, Bass does not use the memory after it has loaded the MOD music.
        /// So you can do whatever you want with the memory after calling this function.
        /// This means there is no need to pin the memory buffer for this method.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// DMO effects are not supported in MOD music on Windows CE, and DirectX 8 (or above) is required on Windows.
        /// They are always available on other platforms, except for the following: Compressor, Gargle, and I3DL2Reverb.
        /// When a DMO effect is unavailable, the MOD music can still be played, but the effect will be disabled.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">The <see cref="BassFlags.AutoFree"/> flag is unavailable to decoding channels.</exception>
        /// <exception cref="Errors.FileOpen"><paramref name="Memory"/> could not be opened.</exception>
        /// <exception cref="Errors.FileFormat"><paramref name="Memory"/>'s format is not recognised/supported.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the stream is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Speaker">The specified SPEAKER flags are invalid. The device/drivers do not support them, they are attempting to assign a stereo stream to a mono speaker or 3D functionality is enabled.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int MusicLoad(IntPtr Memory, long Offset, int Length, BassFlags Flags = BassFlags.Default, int Frequency = 0)
        {
            return BASS_MusicLoad(true, new IntPtr(Memory.ToInt64() + Offset), 0, Length, Flags, Frequency);
        }

        /// <summary>
        /// Loads a MOD music file - MO3 / IT / XM / S3M / MTM / MOD / UMX formats from memory.
        /// </summary>
        /// <param name="Memory">byte[] containing the music data.</param>
        /// <param name="Offset">Memory offset to load the MOD music from.</param>
        /// <param name="Length">Data length.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <param name="Frequency">Sample rate to render/play the MOD music at... 0 = the rate specified in the <see cref="Init" /> call.</param>
        /// <returns>If successful, the loaded music's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>BASS uses the same code as XMPlay for it's MOD music support, giving the most accurate reproduction of MO3 / IT / XM / S3M / MTM / MOD / UMX files available from any sound system.</para>
        /// <para>
        /// MO3s are treated and used in exactly the same way as normal MOD musics.
        /// The advantage of MO3s is that they can be a lot smaller with virtually identical quality.
        /// Playing a MO3 does not use any more CPU power than playing the original MOD version does.
        /// The only difference is a slightly longer load time as the samples are being decoded.
        /// MO3 files are created using the MO3 encoder available at the BASS website.
        /// </para>
        /// <para>
        /// DMO effects (the same as available with <see cref="ChannelSetFX" />) can be used in IT and XM files (and MO3 versions of them) created with Modplug Tracker.
        /// This allows effects to be added to a track without having to resort to an MP3 or OGG version, so it can remain small and still sound fancy.
        /// Of course, the effects require some CPU, so should not be used carelessly if performance is key.
        /// DirectX 8 (or above) is required for the effects to be heard - without that, the music can still be played, but the effects are disabled.
        /// </para>
        /// <para>
        /// "Ramping" does not take a lot of extra processing and improves the sound quality by removing clicks, by ramping/smoothing volume and pan changes.
        /// The start of a sample may also be ramped-in.
        /// That is always the case with XM files (or MOD files in FT2 mode) when using normal ramping, and possibly with all formats when using sensitive ramping; senstitive ramping will only ramp-in when necessary to avoid a click.
        /// Generally, normal ramping is recommended for XM files, and sensitive ramping for the other formats, but some XM files may also sound better using sensitive ramping.
        /// </para>
        /// <para>
        /// When loading a MOD music from memory, Bass does not use the memory after it has loaded the MOD music.
        /// So you can do whatever you want with the memory after calling this function.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// DMO effects are not supported in MOD music on Windows CE, and DirectX 8 (or above) is required on Windows.
        /// They are always available on other platforms, except for the following: Compressor, Gargle, and I3DL2Reverb.
        /// When a DMO effect is unavailable, the MOD music can still be played, but the effect will be disabled.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">The <see cref="BassFlags.AutoFree"/> flag is unavailable to decoding channels.</exception>
        /// <exception cref="Errors.FileOpen"><paramref name="Memory"/> could not be opened.</exception>
        /// <exception cref="Errors.FileFormat"><paramref name="Memory"/>'s format is not recognised/supported.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the stream is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Speaker">The specified SPEAKER flags are invalid. The device/drivers do not support them, they are attempting to assign a stereo stream to a mono speaker or 3D functionality is enabled.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int MusicLoad(byte[] Memory, long Offset, int Length, BassFlags Flags = BassFlags.Default, int Frequency = 0)
        {
            return GCPin.CreateStreamHelper(Pointer => MusicLoad(Pointer, Offset, Length, Flags), Memory);
        }
    }
}

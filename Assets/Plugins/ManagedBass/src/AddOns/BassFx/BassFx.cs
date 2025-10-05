using System.Runtime.InteropServices;
using System;

namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx is a BASS addon providing several DSP functions, including tempo and pitch control.
    /// </summary>
    public static class BassFx
    {
        const string DllName = "bass_fx";
        
        #region Version
        [DllImport(DllName)]
        static extern int BASS_FX_GetVersion();

        /// <summary>
        /// Gets the Version of BassFx that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_FX_GetVersion());
        #endregion

        /// <summary>
        /// Creates a resampling stream from a decoding channel.
        /// </summary>
        /// <param name="Channel">Stream/music/wma/cd/any other supported add-on format using a decoding channel (use <see cref="BassFlags.Decode"/> when creating the channel).</param>
        /// <param name="Flags">A combination of the <see cref="BassFlags"/>.</param>
        /// <returns>If successful, the tempo stream handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Multi-channels are supported.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Channel" /> is not valid.</exception>
        /// <exception cref="Errors.Decode">The <paramref name="Channel" /> is not a decoding channel. Make sure the channel was created using the <see cref="BassFlags.Decode"/> flag.</exception>
        /// <exception cref="Errors.SampleFormat">The <paramref name="Channel" />'s format is not supported. Make sure the channel is either Stereo or Mono.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_TempoCreate")]
        public static extern int TempoCreate(int Channel, BassFlags Flags);
        
        /// <summary>
        /// Get the source channel handle of the reversed stream.
        /// </summary>
        /// <param name="Channel">The handle of the reversed stream.</param>
        /// <returns>If successful, the handle of the source of the reversed stream is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Channel" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_TempoGetSource")]
        public static extern int TempoGetSource(int Channel);
        
        /// <summary>
        /// Get the ratio of the resulting rate and source rate (the resampling ratio).
        /// </summary>
        /// <param name="Channel">Tempo stream (or source channel) handle.</param>
        /// <returns>If successful, the resampling ratio is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Channel" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_TempoGetRateRatio")]
        public static extern float TempoGetRateRatio(int Channel);

        /// <summary>
        /// Creates a reversed stream from a decoding channel.
        /// </summary>
        /// <param name="Channel">Stream/music/wma/cd/any other supported add-on format using a decoding channel.</param>
        /// <param name="DecodingBlockLength">Length of decoding blocks in seconds. Larger blocks means less seeking overhead but larger spikes.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <returns>If successful, the handle of the reversed stream is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// MODs are supported, if <see cref="BassFlags.Prescan"/> flag was applied to a source handle.
        /// <para>For better MP3/2/1 reverse playback create the source stream using the <see cref="BassFlags.Prescan"/> flag.</para>
        /// <para>
        /// Thes <see cref="ChannelAttribute.ReverseDirection"/> attribute can either be applied to the reverse channel or the underlying decoding source channel.
        /// Note, that when playing the channel reverse, the end of a reverse stream is reached at the logial beginning of the stream (this also applies to <see cref="SyncFlags.End"/>).
        /// By default stream's position will start from the end.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Channel" /> is not valid.</exception>
        /// <exception cref="Errors.Decode">The <paramref name="Channel" /> is not a decoding channel. Make sure the channel was created using the <see cref="BassFlags.Decode"/> flag.</exception>
        /// <exception cref="Errors.SampleFormat">The <paramref name="Channel" />'s format is not supported. Make sure the channel is either Stereo or Mono.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_ReverseCreate")]
        public static extern int ReverseCreate(int Channel, float DecodingBlockLength, BassFlags Flags);
        
        /// <summary>
        /// Get the source channel handle of the reversed stream.
        /// </summary>
        /// <param name="Channel">The handle of the reversed stream.</param>
        /// <returns>If successful, the handle of the source of the reversed stream is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Channel" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_ReverseGetSource")]
        public static extern int ReverseGetSource(int Channel);

        #region BPM
        /// <summary>
        /// Get the original BPM of a decoding channel.
        /// </summary>
        /// <param name="Channel">Stream/music/wma/cd/any other supported add-on format using a decoding channel.</param>
        /// <param name="StartSec">Start detecting position in seconds (if less than 0 it uses the current position).</param>
        /// <param name="EndSec">End detecting position in seconds (greater than 0).</param>
        /// <param name="MinMaxBPM">Set min and max bpm, LowWord=Min, HighWord=Max. 0 = defaults to 45/230.</param>
        /// <param name="Flags">One of <see cref="BassFlags.FxBpmBackground"/>, <see cref="BassFlags.FXBpmMult2"/> and <see cref="BassFlags.FxFreeSource"/>.</param>
        /// <param name="Procedure">User defined function to receive the process in percents, use <see langword="null" /> if not in use.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, the original BPM value is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// The BPM detection algorithm works by detecting repeating low-frequency (less than 250Hz) sound patterns and thus works mostly with most rock/pop music with bass or drum beat. 
        /// The BPM detection doesn't work on pieces such as classical music without distinct, repeating bass frequency patterns.
        /// Also pieces with varying tempo, varying bass patterns or very complex bass patterns (jazz, hiphop) may produce odd BPM readings.
        /// </para>
        /// <para>
        /// In cases when the bass pattern drifts a bit around a nominal beat rate (e.g. drummer is again drunken ;-), the BPM algorithm may report incorrect harmonic one-halft to one-thirdth of the correct BPM value.
        /// In such case the system could for example report BPM value of 50 or 100 instead of correct BPM value of 150.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Channel" /> is not valid.</exception>
        /// <exception cref="Errors.Decode">The <paramref name="Channel" /> is not a decoding channel. Make sure the channel was created using the <see cref="BassFlags.Decode"/> flag.</exception>
        /// <exception cref="Errors.SampleFormat">The <paramref name="Channel" />'s format is not supported. Make sure the channel is either Stereo or Mono.</exception>
        /// <exception cref="Errors.Parameter">An illegal parameter was specified.</exception>
        /// <exception cref="Errors.Already">BPM detection, for this <paramref name="Channel" /> is already in use.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_BPM_DecodeGet")]
        public static extern float BPMDecodeGet(int Channel, double StartSec, double EndSec, int MinMaxBPM, BassFlags Flags, BPMProgressProcedure Procedure, IntPtr User = default(IntPtr));

        /// <summary>
        /// Enable getting BPM value by period of time in seconds.
        /// </summary>
        /// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
        /// <param name="Procedure">User defined function to receive the bpm value.</param>
        /// <param name="Period">Detection period in seconds.</param>
        /// <param name="MinMaxBPM">Set min and max bpm, LowWord=Min, HighWord=Max. 0 = defaults to 45/230.</param>
        /// <param name="Flags">Use <see cref="BassFlags.FXBpmMult2"/> or <see cref="BassFlags.Default"/>.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// <list type="table">
        /// <listheader><term><see cref="T:Un4seen.Bass.BASSError">ERROR CODE</see></term><description>Description</description></listheader>
        /// <item><term>BASS_ERROR_HANDLE</term><description></description></item>
        /// <item><term>BASS_ERROR_ILLPARAM</term><description></description></item>
        /// <item><term>BASS_ERROR_ALREADY</term><description></description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter">An illegal parameter was specified.</exception>
        /// <exception cref="Errors.Already"><see cref="BassFlags.FXBpmMult2"/> already used on this handle.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_BPM_CallbackSet")]
        public static extern bool BPMCallbackSet(int Handle, BPMProcedure Procedure, double Period, int MinMaxBPM, BassFlags Flags, IntPtr User = default(IntPtr));

        /// <summary>
        /// Reset the BPM buffers.
        /// </summary>
        /// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// This function flushes the internal buffers of the BPM callback.
        /// The BPM callback is automatically reset by <see cref="Bass.ChannelSetPosition" />, except when called from a <see cref="SyncFlags.Mixtime"/> <see cref="SyncProcedure" />.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_BPM_CallbackReset")]
        public static extern bool BPMCallbackReset(int Handle);
        
        /// <summary>
        /// Frees all resources used by a given handle.
        /// </summary>
        /// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Used together with <see cref="BPMDecodeGet" /> or <see cref="BPMCallbackSet" />.
        /// If <see cref="BassFlags.FxFreeSource"/> was used, this will also free the underlying decoding channel as well.
        /// You can't set/get this flag with <see cref="Bass.ChannelFlags" />/<see cref="Bass.ChannelGetInfo(int, out ChannelInfo)" />.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_BPM_Free")]
        public static extern bool BPMFree(int Handle);

        /// <summary>
        /// Enable getting Beat position in seconds of the decoded channel using a callback function.
        /// </summary>
        /// <param name="Channel">Stream/music/wma/cd/any other supported add-on format using a decoding channel.</param>
        /// <param name="StartSec">Start detecting position in seconds.</param>
        /// <param name="EndSec">End detecting position in seconds (greater than 0).</param>
        /// <param name="Flags">Use one of <see cref="BassFlags.FxBpmBackground"/> and <see cref="BassFlags.FxFreeSource"/>.</param>
        /// <param name="Procedure">User defined function to receive the beat position values.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>This method works pretty much as a mix of <see cref="BPMBeatCallbackSet" /> and <see cref="BPMDecodeGet" />.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Channel" /> is not valid.</exception>
        /// <exception cref="Errors.Decode">The <paramref name="Channel" /> is not a decoding channel. Make sure the channel was created using the <see cref="BassFlags.Decode"/> flag.</exception>
        /// <exception cref="Errors.Parameter">An illegal parameter was specified.</exception>
        /// <exception cref="Errors.Already">Beat detection, for this <paramref name="Channel" /> is already in use.</exception>
        [DllImport(DllName, EntryPoint="BASS_FX_BPM_BeatDecodeGet")]
        public static extern bool BPMBeatDecodeGet(int Channel, double StartSec, double EndSec, BassFlags Flags, BPMBeatProcedure Procedure, IntPtr User = default(IntPtr));

        /// <summary>
        /// Enable getting Beat position in seconds in real-time.
        /// </summary>
        /// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
        /// <param name="Procedure">User defined function to receive the beat position values.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// This method works on real-time (buffered) as well as on decoding channels and might also be used together with Tempo channels.
        /// <para><see cref="BPMBeatFree" /> must be called at the end to free the real-time beat position callback and resources.</para>
        /// <para>Note: You should call <see cref="BPMBeatCallbackReset" /> after you have changed the position of the stream when called from a <see cref="SyncFlags.Mixtime"/> <see cref="SyncProcedure" />.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint="BASS_FX_BPM_BeatCallbackSet")]
        public static extern bool BPMBeatCallbackSet(int Handle, BPMBeatProcedure Procedure, IntPtr User = default(IntPtr));
        
        /// <summary>
        /// Reset the BPM buffers.
        /// </summary>
        /// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// This function flushes the internal buffers of the BPM callback.
        /// The BPM callback is automatically reset by <see cref="Bass.ChannelSetPosition" />, except when called from a <see cref="SyncFlags.Mixtime"/> <see cref="SyncProcedure" />.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint="BASS_FX_BPM_BeatCallbackReset")]
        public static extern bool BPMBeatCallbackReset(int Handle);

        /// <summary>
        /// Set new values for beat detection parameters.
        /// </summary>
        /// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
        /// <param name="Bandwidth">Bandwidth in Hz between 0 and samplerate/2 (-1.0f = leave current, default is 10Hz).</param>
        /// <param name="CenterFreq">The center-frequency in Hz of the band pass filter between 0 and samplerate/2 (-1.0f = leave current, default is 90Hz).</param>
        /// <param name="Beat_rTime">Beat release time in ms. (-1.0f = leave current, default is 20ms).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Beat detection is using a Band Pass Filter.
        /// A band-pass filter is a device that passes frequencies within a certain range and rejects (attenuates) frequencies outside that range.
        /// So the <paramref name="Bandwidth" /> parameter defines the range around a center-frequency to include in the beat detection algo.
        /// The <paramref name="CenterFreq" /> parameter actually defines the center-frequency of the band pass filter.
        /// Once a beat is detected, the <paramref name="Beat_rTime" /> parameter defines the time in ms. in which no other beat will be detected after that just detected beat. 
        /// The background is, that often you have kind-of 'double beats' in a drum set.
        /// So the <paramref name="Beat_rTime" /> should avoid, that a second (quickly repeated beat) beat is detected.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint="BASS_FX_BPM_BeatSetParameters")]
        public static extern bool BPMBeatSetParameters(int Handle, float Bandwidth, float CenterFreq, float Beat_rTime);

        /// <summary>
        /// Gets the current beat detection parameter values.
        /// </summary>
        /// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
        /// <param name="Bandwidth">Current bandwidth in Hz.</param>
        /// <param name="CenterFreq">Current center-frequency in Hz of the band pass filter.</param>
        /// <param name="Beat_rTime">Current beat release time in ms.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Beat detection is using a Band Pass Filter.
        /// A band-pass filter is a device that passes frequencies within a certain range and rejects (attenuates) frequencies outside that range.
        /// So the <paramref name="Bandwidth" /> parameter defines the range around a center-frequency to include in the beat detection algo.
        /// The <paramref name="CenterFreq" /> parameter actually defines the center-frequency of the band pass filter.
        /// Once a beat is detected, the <paramref name="Beat_rTime" /> parameter defines the time in ms. in which no other beat will be detected after that just detected beat. 
        /// The background is, that often you have kind-of 'double beats' in a drum set.
        /// So the <paramref name="Beat_rTime" /> should avoid, that a second (quickly repeated beat) beat is detected.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint="BASS_FX_BPM_BeatGetParameters")]
        public static extern bool BPMBeatGetParameters(int Handle, out float Bandwidth, out float CenterFreq, out float Beat_rTime);
        
        /// <summary>
        /// Free all resources used by a given handle (decode or callback beat).
        /// </summary>
        /// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Used together with <see cref="BPMBeatDecodeGet" /> or <see cref="BPMBeatCallbackSet" />.
        /// <para>
        /// Note: If the <see cref="BassFlags.FxFreeSource"/> flag is used, this will free the source decoding channel as well.
        /// You can't set/get this flag with <see cref="Bass.ChannelFlags" /> and <see cref="Bass.ChannelGetInfo(int, out ChannelInfo)" />.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_BPM_BeatFree")]
        public static extern bool BPMBeatFree(int Handle);
        #endregion
    }
}
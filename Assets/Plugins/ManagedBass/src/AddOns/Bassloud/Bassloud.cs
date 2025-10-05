using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Loud
{
    public static class BassLoud
    {
        const string DllName = "bassloud";

        #region Version
        [DllImport(DllName, EntryPoint = "BASS_Loudness_GetVersion")]
        static extern int GetVersion();

        /// <summary>
        /// Gets the Version of BassFx that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(GetVersion());
        #endregion

        /// <summary>
        /// Retrieves the channel that a loudness measurement is set on.
        /// </summary>
        /// <param name="Handle">The loudness measurement.</param>
        /// <returns>If successful, the loudness measurement's channel handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Loudness_GetChannel")]
        public static extern int GetChannel(int Handle);

        /// <summary>
        /// Retrieves the level of a loudness measurement.
        /// </summary>
        /// <param name="Handle">The loudness measurement handle.</param>
        /// <param name="Mode">The measurement type to retrieve. One of the following. BassFlags.BassLoudnessCurrent, BassFlags.BassLoudnessIntegrated, BassFlags.BassLoudnessRange, BassFlags.BassLoudnessPeak, BassFlags.BassLoudnessTruePeak.</param>
        /// <param name="Level">Pointer to a variable to receive the measurement level.</param>
        /// <returns>If successful, TRUE is returned, else FALSE is returned. Use <see cref="Bass.LastError"/> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Mode"/> is not valid. If requesting a duration with BASS_LOUDNESS_CURRENT then it exceeds what has been enabled.</exception>
        /// <exception cref="Errors.NotAvailable">The requested measurement has not been enabled.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Loudness_GetLevel")]
        public static extern bool GetLevel(int Handle, BassFlags Mode, ref float Level);

        /// <summary>
        /// Retrieves the level of multiple loudness measurements combined.
        /// </summary>
        /// <param name="Handles">An array of loudness measurement handles.</param>
        /// <param name="Count">The number of handles in the array.</param>
        /// <param name="Mode">The measurement type to retrieve. One of the following. BassFlags.BassLoudnessIntegrated, BassFlags.BassLoudnessRange, BassFlags.BassLoudnessPeak, BassFlags.BassLoudnessTruePeak.</param>
        /// <param name="Level">Pointer to a variable to receive the measurement level.</param>
        /// <returns>If successful, TRUE is returned, else FALSE is returned. Use <see cref="Bass.LastError"/> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handles"/> is not valid.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Mode"/> is not valid. If requesting a duration with BASS_LOUDNESS_CURRENT then it exceeds what has been enabled.</exception>
        /// <exception cref="Errors.NotAvailable">The requested measurement has not been enabled. It needs to be enabled on all of the provided handles.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Loudness_GetLevelMulti")]
        public static extern bool GetLevelMulti(int[] Handles, int Count, BassFlags Mode, ref float Level);

        /// <summary>
        /// Moves a loudness measurement to another channel or just changes its DSP priority.
        /// </summary>
        /// <param name="Handle">The loudness measurement handle.</param>
        /// <param name="Channel">The channel to move the measurement to... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Priority">The new DSP priority of the measurements.</param>
        /// <returns>If successful, TRUE is returned, else FALSE is returned. Use <see cref="Bass.LastError"/> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Loudness_SetChannel")]
        public static extern int SetChannel(int Handle, int Channel, int Priority);

        /// <summary>
        /// Starts loudness measurement on a channel.
        /// </summary>
        /// <param name="Handle">The channel handle</param>
        /// <param name="Flags">The measurement mode & flags</param>
        /// <param name="Priority">The DSP priority of the measurements.</param>
        /// <returns>The loudness measurement handle is returned if it is successfully started, else 0 is returned. Use <see cref="Bass.LastError"/> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Loudness_Start", CharSet = CharSet.Unicode)]
        public static extern int Start(int Handle, BassFlags Flags, int Priority);

        /// <summary>
        /// Stops a loudness measurement or all loudness measurements on a channel.
        /// </summary>
        /// <param name="Handle">The channel handle</param>
        /// <returns>If successful, TRUE is returned, else FALSE is returned. Use <see cref="Bass.LastError"/> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Loudness_Stop")]
        public static extern bool Stop(int Handle);
    }
}
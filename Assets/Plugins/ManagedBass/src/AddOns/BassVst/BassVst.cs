using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Vst
{
	/// <summary>
	/// BassVst allows the usage of VST effect plugins as well as VST instruments (VSTi plugins) with BASS.
	/// </summary>
	public static class BassVst
	{
		const string DllName = "bass_vst";
                
        /// <summary>
        /// Creates a new BASS stream based on any VST instrument plugin (VSTi).
        /// </summary>
        /// <param name="Frequency">The sample rate of the VSTi output (e.g. 44100).</param>
        /// <param name="Channels">The number of channels... 1 = mono, 2 = stereo, 4 = quadraphonic, 6 = 5.1, 8 = 7.1.</param>
        /// <param name="DllFile">The fully qualified path and file name to the VSTi plugin (a DLL file name).</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <returns>If successful, the new vst handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// On success, the function returns the new vstHandle that must be given to the other functions.
        /// The returned VST handle can also be given to the typical Bass.Channel* functions.
        /// Use <see cref="ChannelFree" /> to delete and free a VST instrument channel.
        /// </remarks>
        public static int ChannelCreate(int Frequency, int Channels, string DllFile, BassFlags Flags)
		{
			return BASS_VST_ChannelCreate(Frequency, Channels, DllFile, Flags | BassFlags.Unicode);
		}

		[DllImport(DllName, CharSet = CharSet.Unicode)]
		static extern int BASS_VST_ChannelCreate(int Frequency, int Channels, string DllFile, BassFlags Flags);

		/// <summary>
		/// Deletes and frees a VST instrument channel.
		/// </summary>
		/// <param name="VstHandle">The VSTi channel to delete (as created by <see cref="ChannelCreate" />).</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// Note, that you cannot delete VST effects assigned to this channels this way; for this purpose, please call <see cref="ChannelRemoveDSP" />.
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_ChannelFree")]
		public static extern bool ChannelFree(int VstHandle);

		/// <summary>
		/// Removes a VST effect from a channel and destroys the VST instance.
		/// </summary>
		/// <param name="Channels">The channel handle from which to remove the VST effect... a HSTREAM, HMUSIC, or HRECORD.</param>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>If you do not call <see cref="ChannelRemoveDSP" /> explicitly and you have assigned a channel to the effect, the effect is removed automatically when the channel handle is deleted by BASS (like for any other DSP as well).</para>
		/// <para>For various reasons, the underlying DLL is unloaded from memory with a little delay, however, this has also the advantage that subsequent adding/removing of DLLs to channels has no bad performance impact.</para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_ChannelRemoveDSP")]
		public static extern bool ChannelRemoveDSP(int Channels, int VstHandle);

        /// <summary>
        /// Assigns a VST effects (defined by a DLL file name) to any BASS channels.
        /// <para>This overload implements the Unicode overload for the dllFile name, so the BASS_UNICODE flag will automatically be added if not already set.</para>
        /// </summary>
        /// <param name="Channels">The channel handle... a HSTREAM, HMUSIC, or HRECORD. Or 0, if you want to test, if the dll is a valid VST plugin.</param>
        /// <param name="DllFile">The fully qualified path and file name to the VST effect plugin (a DLL file name).</param>
        /// <param name="Flags">A combination of <see cref="BassVstDsp"/></param>
        /// <param name="Priority">Same meaning as for <see cref="Bass.ChannelSetDSP" /> - DSPs with higher priority are called before those with lower.</param>
        /// <returns>On success, the method returns the new vstHandle that must be given to all the other functions, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The VST plugin is implemented via a DSP callback on the channel.
        /// That means when you play the channel (or call <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> if it's a decoding channel), the sample data will be sent to the VST effect at the same time.
        /// If the channel is freed all DSPs are removed automatically, also all VST DSPs are removed as well.
        /// If you want or need to free the VST DSP manually you can call <see cref="ChannelRemoveDSP" />.
        /// <para>For testing if a DLL is a valid VST effect, you can set Channels to 0 - however, do not forget to call <see cref="ChannelRemoveDSP" /> even in this case.</para>
        /// <para>
        /// You may safely assign the same DLL to different channels at the same time - the library makes sure, every channel is processed indepeningly.
        /// But take care to use the correct vstHandles in this case.
        /// </para>
        /// <para>
        /// Finally, you can use any number of VST effects on a channel.
        /// They are processed alongside with all other BASS DSPs in the order of it's priority.
        /// </para>
        /// <para>
        /// To set or get the parameters of a VST effect you might use <see cref="GetParamCount" /> alongside with <see cref="GetParam" /> and <see cref="SetParam" /> to enumerate over the total number of effect parameters.
        /// To retrieve details about an individual parameter you might use <see cref="GetParamInfo(int,int,out BassVstParamInfo)" />.
        /// If the VST effect supports an embedded editor you might also invoke this one with <see cref="EmbedEditor" />.
        /// If the embedded editor also supports localization you might set the language in advance with <see cref="SetLanguage" />.
        /// </para>
        /// <para>If you need to temporarily bypass the VST effect you might call <see cref="SetBypass" /> - <see cref="GetBypass" /> will tell you the current bypass status though.</para>
        /// <para>Use <see cref="GetInfo" /> to get even more details about a loaded VST plugin.</para>
        /// </remarks>
        public static int ChannelSetDSP(int Channels, string DllFile, BassVstDsp Flags, int Priority)
		{
			return BASS_VST_ChannelSetDSP(Channels, DllFile, Flags | BassVstDsp.Unicode, Priority);
		}

		[DllImport(DllName, CharSet = CharSet.Unicode)]
		static extern int BASS_VST_ChannelSetDSP(int Channels, string DllFile, BassVstDsp Flags, int Priority);

		/// <summary>
		/// Many VST effects come along with an graphical parameters editor; with the following function, you can embed these editors to your user interface.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="ParentWindow">The IntPtr to the window handle (HWND) of the parents window in which the editor should be embedded (e.g. use a new modeless dialog or user control).</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
		/// To embed the editor to another window, call this function with parentWindow set to the HWND of the parent window.
		/// To check, if an effect has an editor, see the hasEditor flag set by <see cref="GetInfo" />.
		/// </para>
		/// <para>To "unembed" the editor, call this function with <paramref name="ParentWindow"/> set to <see langword="null" />.</para>
		/// <para>
		/// If you create the editor window independently of a real channel (by skipping the channel parameter when calling <see cref="ChannelSetDSP" />) and the editor displays any spectrums, VU-meters or such, 
		/// the data for this come from the most recent channel using the same effect and the same scope.
		/// The scope can be set by <see cref="SetScope" /> to any ID, the default is 0.
		/// </para>
		/// <para>In order to create a new window in which the editor should be embedded, it is a good idea to call <see cref="GetInfo" /> in order to retrieve the editors height and width.</para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_EmbedEditor")]
		public static extern bool EmbedEditor(int VstHandle, IntPtr ParentWindow);

		/// <summary>
		/// Gets the current bypasses state of the the effect processing.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		[DllImport(DllName, EntryPoint = "BASS_VST_GetBypass")]
		public static extern bool GetBypass(int VstHandle);

		[DllImport(DllName)]
		static extern IntPtr BASS_VST_GetChunk(int VstHandle, bool IsPreset, ref int Length);

		/// <summary>
		/// Gets the VST plug-in state as a plain byte array (memory chunk storage).
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="IsPreset"><see langword="true" /> when saving a single program; <see langword="false" /> for all programs.</param>
		/// <returns>The array of bytes representing the VST plug-in state - or <see langword="null" /> if the VST doesn't support the chunk data mode.</returns>
		/// <remarks>
		/// There are two ways to store the current state of a VST plug-In:
		/// Either trough the parameter interfaces (<see cref="GetParam" /> and <see cref="SetParam" />) or as an opaque memory block (chunk mode).
		/// <para>
		/// You might first queries this method to see, if the VST supports the chunk mode.
		/// If it is not implemented (<see langword="null" /> is returned), the values of all parameters might be used to save the plug-in state.
		/// </para>
		/// <para>
		/// Chunk storage allows to save additional data (besides the parameter state) which is specific to the VST plug-in.
		/// Please note, that if you decide to use the chunk storage, you have to take care of saving and loading parameter states on your own (see <see cref="SetChunk(int,bool,byte[],int)" /> for details)!
		/// </para>
		/// </remarks>
		public static byte[] GetChunk(int VstHandle, bool IsPreset)
		{
			var num = 0;
			var intPtr = BASS_VST_GetChunk(VstHandle, IsPreset, ref num);

		    if (intPtr == IntPtr.Zero || num <= 0)
		        return null;

		    var numArray = new byte[num];
			Marshal.Copy(intPtr, numArray, 0, num);
			return numArray;
		}

		/// <summary>
		/// Gets general information about a VST effect plugin.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="Info">An instance of the <see cref="BassVstInfo" /> where to store the parameter information at.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
		/// VST effects that have no input channels (so called "Instruments") are not loaded by BASS_VST.
		/// So you can assume chansIn and chansOut to be at least 1.
		/// </para>
		/// <para>
		/// Multi-channel streams should work correctly, if supported by a effect.
		/// If not, only the first chansIn channels are processed by the effect, the other ones stay unaffected.  
		/// The opposite, eg. assigning multi-channel effects to stereo channels, should be no problem at all.
		/// </para>
		/// <para>
		/// If mono effects are assigned to stereo channels, the result will be mono, expanded to both channels.
		/// This behaviour can be switched of using the <see cref="BassVstDsp.KeepChannels"/> in <see cref="ChannelSetDSP" />.
		/// </para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_GetInfo")]
		public static extern bool GetInfo(int VstHandle, out BassVstInfo Info);

		/// <summary>
		/// Gets general information about a VST effect plugin.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <returns>If successful, an instance of <see cref="BassVstInfo" /> is returned, else <see langword="null" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
		/// VST effects that have no input channels (so called "Instruments") are not loaded by BASS_VST.
		/// So you can assume chansIn and chansOut to be at least 1.
		/// </para>
		/// <para>
		/// Multi-channel streams should work correctly, if supported by a effect.
		/// If not, only the first chansIn channels are processed by the effect, the other ones stay unaffected.  
		/// The opposite, eg. assigning multi-channel effects to stereo channels, should be no problem at all.
		/// </para>
		/// <para>
		/// If mono effects are assigned to stereo channels, the result will be mono, expanded to both channels.
		/// This behaviour can be switched of using the <see cref="BassVstDsp.KeepChannels"/> in <see cref="ChannelSetDSP" />.
		/// </para>
		/// </remarks>
		public static BassVstInfo BASS_VST_GetInfo(int VstHandle)
		{
            if (!GetInfo(VstHandle, out var info))
		        throw new BassException();

		    return info;
		}

        /// <summary>
        /// Get the value of a single VST effect parameter.
        /// </summary>
        /// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
        /// <param name="ParamIndex">The index of the parameter (must be smaller than <see cref="GetParamCount" />).</param>
        /// <returns><see langword="true" /> on success.</returns>
        /// <remarks>
        /// <para>
        /// All VST effect parameters are in the range from 0.0 to 1.0 (float), however, from the view of a VST effect, they may represent completely different values.
        /// E.g. some might represent a multiplier to some internal constants and will result in number of samples or some might represent a value in dB etc.
        /// </para>
        /// <para>You can use <see cref="GetParamInfo(int,int,out BassVstParamInfo)" /> to get further information about a single parameter, which will also present you with the current value in a readable format.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_VST_GetParam")]
		public static extern float GetParam(int VstHandle, int ParamIndex);

        /// <summary>
        /// Returns the number of editable parameters for the VST effect.
        /// </summary>
        /// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
        /// <returns>The number of editable parameters or if the effect has no editable parameters, 0 is returned.</returns>
        /// <remarks>
        /// To set or get the individual parameters of a VST effect you might use <see cref="GetParamCount" /> alongside with <see cref="GetParam" /> and <see cref="SetParam" /> to enumerate over the total number of effect parameters.
        /// To retrieve details about an individual parameter you might use <see cref="GetParamInfo(int,int,out BassVstParamInfo)" />.
        /// If the VST effect supports an embedded editor you might also invoke this one with <see cref="EmbedEditor" />.
        /// If the embedded editor also supports localization you might set the language in advance with <see cref="SetLanguage" />.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_VST_GetParamCount")]
		public static extern int GetParamCount(int VstHandle);

        /// <summary>
        /// Get some common information about an editable parameter to a <see cref="BassVstParamInfo" /> object.
        /// </summary>
        /// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
        /// <param name="ParamIndex">The index of the parameter (must be smaller than <see cref="GetParamCount" />).</param>
        /// <param name="Info">An instance of the <see cref="BassVstParamInfo" /> where to store the parameter information at.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        [DllImport(DllName, EntryPoint = "BASS_VST_GetParamInfo")]
		public static extern bool GetParamInfo(int VstHandle, int ParamIndex, out BassVstParamInfo Info);

        /// <summary>
        /// Get some common information about an editable parameter to a <see cref="BassVstParamInfo" /> class.
        /// </summary>
        /// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
        /// <param name="ParamIndex">The index of the parameter (must be smaller than <see cref="GetParamCount" />).</param>
        /// <returns>If successful, an instance of the <see cref="BassVstParamInfo" /> is returned, else <see langword="null" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        public static BassVstParamInfo GetParamInfo(int VstHandle, int ParamIndex)
		{
            if (!GetParamInfo(VstHandle, ParamIndex, out var info))
                throw new BassException();

		    return info;
		}

		/// <summary>
		/// Returns the currently selected program for the VST effect.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <returns>The currect selected program number. Valid program numbers are between 0 and <see cref="GetProgramCount" /> minus 1.</returns>
		/// <remarks>
		/// After construction (using <see cref="ChannelSetDSP" />), always the first program (0) is selected.
		/// <para>
		/// With <see cref="SetProgram" /> you can change the selected program.
		/// Functions as <see cref="SetParam" /> will always change the selected program's settings.
		/// </para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_GetProgram")]
		public static extern int GetProgram(int VstHandle);

		/// <summary>
		/// Returns the number of editable programs for the VST effect.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <returns>The number of available programs or 0 if no program is available.</returns>
		/// <remarks>
		/// Many (not all!) effects have more than one "program" that can hold a complete set of parameters each.
		/// Moreover, some of these programs may be initialized to some useful "factory defaults".
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_GetProgramCount")]
		public static extern int GetProgramCount(int VstHandle);

		/// <summary>
		/// Gets the name of any program of a VST effect.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="ProgramIndex">The program number for which to get the name, must be smaller than <see cref="GetProgramCount" />.</param>
		/// <returns>The name of the program given or <see langword="null" /> if not valid.</returns>
		/// <remarks>The names are limited to 24 characters. This function does not change the selected program!</remarks>
		public static string GetProgramName(int VstHandle, int ProgramIndex)
		{
			return Marshal.PtrToStringAnsi(BASS_VST_GetProgramName(VstHandle, ProgramIndex));
		}

		[DllImport(DllName)]
		static extern IntPtr BASS_VST_GetProgramName(int VstHandle, int ProgramIndex);

		/// <summary>
		/// Returns a list of all available program names.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <returns>An array of strings representing the list of available program names. The index corresponds to the program numbers.</returns>
		/// <remarks>This function does not change the selected program!</remarks>
		public static string[] GetProgramNames(int VstHandle)
		{
			var num = GetProgramCount(VstHandle);
		    if (num <= 0)
		        return null;

		    var strArrays = new string[num];

		    for (var i = 0; i < num; i++)
		        strArrays[i] = GetProgramName(VstHandle, i);

		    return strArrays;
		}

		/// <summary>
		/// Returns the parameters of a given program.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="ProgramIndex">The program number for which to query the parameter values, must be smaller than <see cref="GetProgramCount" />.</param>
		/// <returns>An array of float values representing the parameter values of the given program or <see langword="null" /> if the program (VST effect) has no parameters or an error occurred.</returns>
		/// <remarks>
		/// <para>The parameters of the currently selected program can also be queried by <see cref="GetParam" />.</para>
		/// <para>
		/// The function returns the parameters as an array of floats.
		/// The number of elements in the returned array is equal to <see cref="GetParamCount" />.
		/// </para>
		/// <para>This function does not change the selected program!</para>
		/// </remarks>
		public static float[] GetProgramParam(int VstHandle, int ProgramIndex)
		{
			var num = 0;
			var intPtr = BASS_VST_GetProgramParam(VstHandle, ProgramIndex, ref num);
		    if (intPtr == IntPtr.Zero || num <= 0)
		        return null;

		    var singleArray = new float[num];
			Marshal.Copy(intPtr, singleArray, 0, num);
			return singleArray;
		}

		[DllImport(DllName)]
		static extern IntPtr BASS_VST_GetProgramParam(int VstHandle, int ProgramIndex, ref int Length);

		/// <summary>
		/// Sends a MIDI message/event to the VSTi plugin.
		/// </summary>
		/// <param name="VstHandle">The VSTi channel to send a MIDI message to (as created by <see cref="ChannelCreate" />).</param>
		/// <param name="MidiChannel">The Midi channel number to use (0 to 15).</param>
		/// <param name="EventType">The Midi event/status value to use (see MidiEventType for details).</param>
		/// <param name="Param">The data bytes to send with the message to compose a data byte 1 and 2.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// Use one of the MidiEventType commands similar to BassMidi.StreamEvent(int,int,MidiEventType,int).
		/// <para>
		/// Set <paramref name="MidiChannel" /> to 0xFFFF and <paramref name="EventType" /> to the raw command to send. 
		/// The raw command must be encoded as 0x00xxyyzz with xx=MIDI command, yy=MIDI databyte #1, zz=MIDI databyte #2.
		/// <paramref name="Param" /> should be set to 0 in this case.
		/// </para>
		/// <para>
		/// Send SysEx commands by setting <paramref name="MidiChannel" /> to 0xEEEE. 
		/// <paramref name="EventType" /> will denote the type of event to send (see MidiEventType about possible values for <paramref name="Param" /> in such case).
		/// </para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_ProcessEvent")]
		public static extern bool ProcessEvent(int VstHandle, int MidiChannel, int EventType, int Param);

		/// <summary>
		/// Sends a SysEx or MIDI (short)message/event to the VSTi plugin.
		/// </summary>
		/// <param name="VstHandle">The VSTi channel to send a MIDI message to (as created by <see cref="ChannelCreate" />).</param>
		/// <param name="Message">The pointer to your Midi message data to send (byte[]).</param>
		/// <param name="Length">The length of a SysEx message or 0 in case of a normal Midi (short)message.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
		/// To send a Midi (short)message : 
		/// The raw message must be encoded as 0x00xxyyzz with xx=MIDI command, yy=MIDI databyte #1, zz=MIDI databyte #2.
		/// <paramref name="Length" /> should be set to 0 in this case.
		/// </para>
		/// <para>
		/// To send a SysEx message:
		/// <paramref name="Message" /> must be set to a pointer to the bytes to send and <paramref name="Length" /> must be set to the number of bytes to send.
		/// </para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_ProcessEventRaw")]
		public static extern bool ProcessEventRaw(int VstHandle, IntPtr Message, int Length);

        /// <summary>
        /// Sends a SysEx or MIDI (short)message/event to the VSTi plugin.
        /// </summary>
        /// <param name="VstHandle">The VSTi channel to send a MIDI message to (as created by <see cref="ChannelCreate" />).</param>
        /// <param name="Message">The byte array containing your Midi message data to send.</param>
        /// <param name="Length">The length of a SysEx message or 0 in case of a normal Midi (short)message.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// To send a Midi (short)message : 
        /// The raw message must be encoded as 0x00xxyyzz with xx=MIDI command, yy=MIDI databyte #1, zz=MIDI databyte #2.
        /// <paramref name="Length" /> should be set to 0 in this case.
        /// </para>
        /// <para>
        /// To send a SysEx message:
        /// <paramref name="Message" /> must be set to a pointer to the bytes to send and <paramref name="Length" /> must be set to the number of bytes to send.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_VST_ProcessEventRaw")]
        public static extern bool ProcessEventRaw(int VstHandle, byte[] Message, int Length);
        
		/// <summary>
		/// Call this function after position changes or sth. like that.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// Some VST effects will use an internal buffer for effect calculation and handling.
		/// This will reset the internal VST buffers which may remember some "old" data.
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_Resume")]
		public static extern bool Resume(int VstHandle);

		/// <summary>
		/// Bypasses the effect processing (state=<see langword="true" />) or switch back to normal processing (state=<see langword="false" />).
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="State"><see langword="true" /> to bypasses the effect processing; <see langword="false" /> to switch back to normal processing.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// By default bypassing is OFF and the effect will be processed normally.
		/// Use <see cref="GetBypass" /> returns the current state.
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_SetBypass")]
		public static extern bool SetBypass(int VstHandle, bool State);

		/// <summary>
		/// Assign a callback function to a vstHandle.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="Procedure">The user defined callback delegate (see <see cref="VstProcedure" />).</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>Unless defined otherwise, the callback function should always return 0.</remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_SetCallback")]
		public static extern bool SetCallback(int VstHandle, VstProcedure Procedure, IntPtr User);

        /// <summary>
        /// Sets the VST plug-in state with a plain byte array (memory chunk storage).
        /// </summary>
        /// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
        /// <param name="IsPreset"><see langword="true" /> when restoring a single program; <see langword="false" /> for all programs.</param>
        /// <param name="Chunk">The byte array containing the memory chunk storage to set.</param>
        /// <param name="Length">The number of bytes to write.</param>
        /// <returns>The number of bytes written.</returns>
        /// <remarks>
        /// Might be used to restore a VST plug-in state which was previously saved via <see cref="GetChunk" />.
        /// <para>After restoring a plug-in state you might need to retrieve the program names again (see <see cref="GetProgramName" /> and <see cref="GetProgramCount" />) as they might have changed.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_VST_SetChunk")]
		public static extern int SetChunk(int VstHandle, bool IsPreset, byte[] Chunk, int Length);

		/// <summary>
		/// Sets the VST plug-in state with a plain byte array (memory chunk storage).
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="IsPreset"><see langword="true" /> when restoring a single program; <see langword="false" /> for all programs.</param>
		/// <param name="Chunk">The byte array containing the memory chunk storage to set.</param>
		/// <returns>The number of bytes written.</returns>
		/// <remarks>
		/// Might be used to restore a VST plug-in state which was previously saved via <see cref="GetChunk" />.
		/// <para>After restoring a plug-in state you might need to retrieve the program names again (see <see cref="GetProgramName" /> and <see cref="GetProgramCount" />) as they might have changed.</para>
		/// </remarks>
		public static int SetChunk(int VstHandle, bool IsPreset, byte[] Chunk)
		{
		    return SetChunk(VstHandle, IsPreset, Chunk, Chunk.Length);
		}

		/// <summary>
		/// Set the VST language to be used by any plugins.
		/// </summary>
		/// <param name="Language">The desired language as ISO 639.1, e.g. "en", "de", "es"...</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// Some VST effects come along localized.
		/// With this function you can set the desired language as ISO 639.1 -- eg. "en" for english, "de" for german, "es" for spanish and so on.
		/// The default language is english.
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_SetLanguage", CharSet = CharSet.Unicode)]
		public static extern bool SetLanguage(string Language);

        /// <summary>
        /// Set a value of a single VST effect parameter.
        /// </summary>
        /// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
        /// <param name="ParamIndex">The index of the parameter (must be smaller than <see cref="GetParamCount" />).</param>
        /// <param name="NewValue">The new value to set in the range from 0.0 to 1.0 (float). See the documentation of the actual VST implementation for details of the effective value representation.</param>
        /// <returns><see langword="true" /> on success.</returns>
        /// <remarks>
        /// <para>
        /// All VST effect parameters are in the range from 0.0 to 1.0 (float), however, from the view of a VST effect, they may represent completely different values.
        /// E.g. some might represent a multiplier to some internal constants and will result in number of samples or some might represent a value in dB etc.
        /// </para>
        /// <para>So it is a good idea to call <see cref="GetParamInfo(int,int,out BassVstParamInfo)" /> after you modified a parameter, in order to to get further information about the parameter in question, which will also present you with the current value in a readable format.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_VST_SetParam")]
		public static extern bool SetParam(int VstHandle, int ParamIndex, float NewValue);
        
		/// <summary>
		/// Sets (changes) the selected program for the VST effect.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="ProgramIndex">The program number to set (between 0 and <see cref="GetProgramCount" /> minus 1.).</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>You might call <see cref="GetProgramCount" /> to check, if the VST effect has any editable programs available.
		/// <para>
		/// With <see cref="GetProgram" /> you can check, which is the current selected program.
		/// Functions as as <see cref="SetParam" /> will always change the selected program's settings.
		/// </para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_SetProgram")]
		public static extern bool SetProgram(int VstHandle, int ProgramIndex);

		/// <summary>
		/// Sets the name of any program of a VST effect.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="ProgramIndex">The program number for which to set the name, must be smaller than <see cref="GetProgramCount" />.</param>
		/// <param name="Name">The new name to use. Names are limited to 24 characters, BASS_VST truncates the names, if needed.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>This function does not change the selected program!</remarks>
		[DllImport(DllName, EntryPoint = "BASS_VST_SetProgramName", CharSet = CharSet.Unicode)]
		public static extern bool SetProgramName(int VstHandle, int ProgramIndex, string Name);

		[DllImport(DllName)]
		static extern bool BASS_VST_SetProgramParam(int VstHandle, int ProgramIndex, float[] Param, int Length);

		/// <summary>
		/// Set all parameters of any program in a VST effect.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="ProgramIndex">The program number for which to set the parameter values, must be smaller than <see cref="GetProgramCount" />.</param>
		/// <param name="Param">An array with the parameter values to set. The array needs to have as many elements as defined by <see cref="GetParamCount" /> or as returned be <see cref="GetProgramParam" />.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// This function does not change the selected program!
		/// <para>If you use <see cref="SetCallback" />, the <see cref="BassVstAction.ParametersChanged"/> event is only posted if you select a program with parameters different from the prior.</para>
		/// </remarks>
		public static bool SetProgramParam(int VstHandle, int ProgramIndex, float[] Param)
		{
		    return BASS_VST_SetProgramParam(VstHandle, ProgramIndex, Param, Param.Length);
		}

		/// <summary>
		/// Sets the scope of an Editor to a given ID.
		/// </summary>
		/// <param name="VstHandle">The VST effect handle as returned by <see cref="ChannelSetDSP" />.</param>
		/// <param name="Scope">The ID to set the scope to.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		[DllImport(DllName, EntryPoint = "BASS_VST_SetScope")]
		public static extern bool SetScope(int VstHandle, int Scope);
	}
}
namespace ManagedBass.Vst
{
	/// <summary>
	/// VST Dispatcher OpCodes.
	/// <para>For more info see the Steinberg VST SDK documentation.</para>
	/// </summary>
	public enum BassVstDispatcherOpCodes
	{
		/// <summary>
		/// Initialise.
		/// </summary>
		Open,

		/// <summary>
		/// Exit, release all memory and other resources!
		/// </summary>
		Close,
		
        /// <summary>
		/// Program number in "value".
		/// </summary>
		SetProgram,
		
        /// <summary>
		/// Returns the current program number.
		/// </summary>
		GetProgram,
		
        /// <summary>
		/// User changed program name (max 24 char + 0) to as passed in string.
		/// </summary>
		SetProgramName,
		
        /// <summary>
		/// Stuff program name (max 24 char + 0) into string.
		/// </summary>
		GetProgramName,
		
        /// <summary>
		/// Stuff parameter "index" label (max 8 char + 0) into string.
		/// <para>Examples: 'sec', 'dB', 'type'</para>
		/// </summary>
		GetParamLabel,
		
        /// <summary>
		/// Stuff parameter "index" textual representation into string.
		/// <para>Examples: '0.5', '-3', 'PLATE'</para>
		/// </summary>
		GetParamDisplay,
		
        /// <summary>
		/// Stuff parameter "index" label (max 8 char + 0) into string.
		/// <para>Examples: 'Time', 'Gain', 'RoomType'</para>
		/// </summary>
		GetParamName,
		
        /// <summary>
		/// Called if (flags &amp; (effFlagsHasClip | effFlagsHasVu)).
		/// </summary>
		GetVu,
		
        /// <summary>
		/// System: In opt (float value in Hz; for example 44100.0Hz).
		/// </summary>
		SetSampleRate,
		
        /// <summary>
		/// System: In value (this is the maximun size of an audio block, pls check sampleframes in process call).
		/// </summary>
		SetBlockSize,
		
        /// <summary>
		/// System: The user has switched the 'power on' button to value (0 off, else on). This only switches audio  processing; you should flush delay buffers etc.
		/// </summary>
		MainsChanged,
		
        /// <summary>
		/// Editor: Stuff rect (top, left, bottom, right) into ptr.
		/// </summary>
		EditGetRect,
		
        /// <summary>
		/// Editor: System dependant Window pointer in ptr.
		/// </summary>
		EditOpen,
		
        /// <summary>
		/// Editor: No arguments.
		/// </summary>
		EditClose,
		
        /// <summary>
		/// Editor: Draw method, ptr points to rect (MAC Only).
		/// </summary>
		EditDraw,
		
        /// <summary>
		/// Editor: index: x, value: y (MAC Only).
		/// </summary>
		EditMouse,
		
        /// <summary>
		/// Editor: System keycode in value.
		/// </summary>
		EditKey,
		
        /// <summary>
		/// Editor: no arguments. Be gentle!
		/// </summary>
		EditIdle,
		
        /// <summary>
		/// Editor: Window has topped, no arguments.
		/// </summary>
		EditTop,
		
        /// <summary>
		/// Editor: Window goes to background.
		/// </summary>
		EditSleep,
		
        /// <summary>
		/// Returns 'NvEf'.
		/// </summary>
		Identify,
		
        /// <summary>
		/// Host requests pointer to chunk into (void**)ptr, byteSize returned.
		/// </summary>
		GetChunk,
		
        /// <summary>
		/// Plug-in receives saved chunk, byteSize passed.
		/// </summary>
		SetChunk,
		
        /// <summary>
		/// VstEvents* in "ptr".
		/// </summary>
		ProcessEvents,
		
        /// <summary>
		/// Parameter index in "index".
		/// </summary>
		CanBeAutomated,
		
        /// <summary>
		/// Parameter index in "index", string in "ptr".
		/// </summary>
		String2Parameter,
		
        /// <summary>
		/// No arguments. This is for dividing programs into groups (like GM).
		/// </summary>
		GetNumProgramCategories,
		
        /// <summary>
		/// Get program name of category "value", program "index" into "ptr".
		/// <para>Category (that is, "value") may be -1, in which case program indices are enumerated linearily (as usual); otherwise, each category starts over with index 0.</para>
		/// </summary>
		GetProgramNameIndexed,
		
        /// <summary>
		/// Copy current program to destination "index"
		/// <para>Note: implies setParameter connections, configuration.</para>
		/// </summary>
		CopyProgram,
		
        /// <summary>
		/// Input at "index" has been (dis-)connected; "value" == 0: disconnected, else connected.
		/// </summary>
		ConnectInput,
		
        /// <summary>
		/// Onput at "index" has been (dis-)connected; "value" == 0: disconnected, else connected.
		/// </summary>
		ConnectOutput,
		
        /// <summary>
		/// "index", VstPinProperties* in ptr, return != 0 means true.
		/// </summary>
		GetInputProperties,
		
        /// <summary>
		/// "index", VstPinProperties* in ptr, return != 0 means true.
		/// </summary>
		GetOutputProperties,
		
        /// <summary>
		/// No parameter, return value is category.
		/// </summary>
		GetPlugCategory,
		
        /// <summary>
		/// Realtime: for external dsp, see flag bits below.
		/// </summary>
		GetCurrentPosition,
		
        /// <summary>
		/// Realtime: for external dsp, see flag bits below. returns float*.
		/// </summary>
		GetDestinationBuffer,
		
        /// <summary>
		/// Offline: ptr = VstAudioFile array, value = count, index = start flag.
		/// </summary>
		OfflineNotify,
		
        /// <summary>
		/// Offline: ptr = VstOfflineTask array, value = count.
		/// </summary>
		OfflinePrepare,
		
        /// <summary>
		/// Offline: ptr = VstOfflineTask array, value = count.
		/// </summary>
		OfflineRun,
		
        /// <summary>
		/// VstVariableIo* in "ptr".
		/// </summary>
		ProcessVarIo,
		
        /// <summary>
		/// VstSpeakerArrangement* pluginInput in "value"; VstSpeakerArrangement* pluginOutput in "ptr".
		/// </summary>
		SetSpeakerArrangement,
		
        /// <summary>
		/// Block size in "value", sampleRate in "opt"
		/// </summary>
		SetBlockSizeAndSampleRate,
		
        /// <summary>
		/// On/Off in "value" (0 = off, 1 = on).
		/// </summary>
		SetBypass,
		
        /// <summary>
		/// char* name (max 32 bytes) in "ptr".
		/// </summary>
		GetEffectName,
		
        /// <summary>
		/// char* text (max 256 bytes) in "ptr".
		/// </summary>
		GetErrorText,
		
        /// <summary>
		/// Fills "ptr" with a string identifying the vendor (max 64 char).
		/// </summary>
		GetVendorString,
		
        /// <summary>
		/// Fills "ptr" with a string with product name (max 64 char).
		/// </summary>
		GetProductString,
		
        /// <summary>
		/// Returns vendor-specific version.
		/// </summary>
		GetVendorVersion,
		
        /// <summary>
		/// No definition, vendor specific handling.
		/// </summary>
		VendorSpecific,
		
        /// <summary>
		/// "ptr" contains one of the 'plugCanDos' strings (e.g. "bypass").
		/// </summary>
		CanDo,
		
        /// <summary>
		/// Returns tail size; 0 is default (return 1 for 'no tail').
		/// </summary>
		GetTailSize,
		
        /// <summary>
		/// Idle call in response to audioMasterneedIdle. Must return 1 to keep idle calls beeing issued.
		/// </summary>
		Idle,
		
        /// <summary>
		/// GUI: void* in "ptr", not yet defined.
		/// </summary>
		GetIcon,
		
        /// <summary>
		/// GUI: set view position (in window) to x "index" y "value"
		/// </summary>
		SetViewPosition,
		
        /// <summary>
		/// Of param "index", VstParameterProperties* in "ptr".
		/// </summary>
		GetParameterProperties,
		
        /// <summary>
		/// Returns 0: needs keys (default for 1.0 plugs), 1: don't need.
		/// </summary>
		KeysRequired,
		
        /// <summary>
		/// Returns 2 for VST 2; older versions return 0; 2100 for VST 2.1...2400 for VST 2.4.
		/// </summary>
		GetVstVersion,
		
        /// <summary>
		/// Character in "index", virtual in "value", modifiers in "opt", return -1 if not used, return 1 if used.
		/// </summary>
		EditKeyDown,
		
        /// <summary>
		/// Character in "index", virtual in "value", modifiers in "opt", return -1 if not used, return 1 if used.
		/// </summary>
		EditKeyUp,
		
        /// <summary>
		/// Mode in "value": 0: circular, 1:circular relativ, 2:linear.
		/// </summary>
		SetEditKnobMode,
		
        /// <summary>
		/// Passed "ptr" points to MidiProgramName struct.
		/// <para>Struct will be filled with information for 'thisProgramIndex'.
		/// Returns number of used programIndexes, if 0 is returned, no MidiProgramNames supported.</para>
		/// </summary>
		GetMidiProgramName,
		
        /// <summary>
		/// Returns the programIndex of the current program.
		/// <para>Passed "ptr" points to MidiProgramName struct, struct will be filled with information for the current program.</para>
		/// </summary>
		GetCurrentMidiProgram,
		
        /// <summary>
		/// Passed "ptr" points to MidiProgramCategory struct.
		/// <para>Struct will be filled with information for 'thisCategoryIndex'.
		/// Returns number of used categoryIndexes, if 0 is returned, no MidiProgramCategories supported.</para>
		/// </summary>
		GetMidiProgramCategory,
		
        /// <summary>
		/// Returns 1 if the MidiProgramNames or MidiKeyNames had changed on this channel, 0 otherwise. "ptr" ignored.
		/// </summary>
		HasMidiProgramsChanged,
		
        /// <summary>
		/// Passed "ptr" points to MidiKeyName struct.
		/// <para>Struct will be filled with information for 'thisProgramIndex' and 'thisKeyNumber'.
		/// If keyName is "" the standard name of the key will be displayed. If 0 is returned, no MidiKeyNames are defined for 'thisProgramIndex'.
		/// </para>
		/// </summary>
		GetMidiKeyName,
		
        /// <summary>
		/// Called before a new program is loaded.
		/// </summary>
		BeginSetProgram,
		
        /// <summary>
		/// Called when the program is loaded.
		/// </summary>
		EndSetProgram,
		
        /// <summary>
		/// VstSpeakerArrangement** pluginInput in "value". VstSpeakerArrangement** pluginOutput in "ptr".
		/// </summary>
		GetSpeakerArrangement,
		
        /// <summary>
		/// This opcode is only called, if plugin is of type kPlugCategShell. Returns the next plugin's uniqueID.
		/// <para>"ptr" points to a char buffer of size 64, which is to be filled with the name of the plugin including the terminating zero.</para>
		/// </summary>
		ShellGetNextPlugin,
		
        /// <summary>
		/// Called before the start of process call.
		/// </summary>
		StartProcess,
		
        /// <summary>
		/// Called after the stop of process call.
		/// </summary>
		StopProcess,
		
        /// <summary>
		/// Called in offline (non RealTime) Process before process is called, indicates how many sample will be processed.
		/// </summary>
		SetTotalSampleToProcess,
		
        /// <summary>
		/// PanLaw : Type (Linear, Equal Power,.. see enum PanLaw Type) in "value", Gain in "opt": for Linear : [1.0 means 0dB PanLaw], [~0.58 means -4.5dB], [0.5 means -6.02dB].
		/// </summary>
		SetPanLaw,
		
        /// <summary>
		/// Called before a Bank is loaded, "ptr" points to VstPatchChunkInfo structure.
		/// <para>Return -1 if the Bank can not be loaded, return 1 if it can be loaded else 0 (for compatibility).</para>
		/// </summary>
		BeginLoadBank,
		
        /// <summary>
		/// Called before a Program is loaded, "ptr" points to VstPatchChunkInfo structure.
		/// Return -1 if the Program can not be loaded, return 1 if it can be loaded else 0 (for compatibility).
		/// </summary>
		BeginLoadProgram,
		
        /// <summary>
		/// Sets the processing precision in "value" (0=32 bit, 1=64 bit).
		/// </summary>
		SetProcessPrecision,
		
        /// <summary>
		/// Returns the number of used MIDI input channels (1-15).
		/// </summary>
		GetNumMidiInputChannels,
		
        /// <summary>
		/// Returns the number of used MIDI output channels (1-15).
		/// </summary>
		GetNumMidiOutputChannels,
		
        /// <summary>
		/// Returns the number of available OpCodes
		/// </summary>
		NumOpcodes
	}
}
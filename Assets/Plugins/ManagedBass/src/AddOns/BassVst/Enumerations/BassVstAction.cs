namespace ManagedBass.Vst
{
	/// <summary>
	/// VST action parameters as used within the <see cref="VstProcedure" /> callback.
	/// <para>See also <see cref="BassVst.SetCallback" />.</para>
	/// </summary>
	public enum BassVstAction
	{
		/// <summary>
		/// Some parameters are changed by the editor opened by <see cref="BassVst.EmbedEditor" />, NOT called if you call <see cref="BassVst.SetParam" />.
		/// </summary>
		ParametersChanged = 1,

		/// <summary>
		/// The embedded editor window should be resized, the new width/height can be found in param1/param2 and in <see cref="BassVst.GetInfo" />.
		/// </summary>
		EditorResized = 2,
		
        /// <summary>
		/// Can be used to subclass the audioMaster callback (see the VST SDK), param1 is a pointer to a instance of the <see cref="BassVstAudioMasterParam" /> class.
		/// </summary>
		AudioMaster = 3
	}
}
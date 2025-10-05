using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Sfx
{
	/// <summary>
	/// BassSfx is an extention to the BASS audio library, providing a set of functions for rendering Sonique Visualization plugins or Winamp visualization plugins on a provided device context (hDC).
	/// </summary>
	public static class BassSfx
	{
		const string DllName = "bass_sfx";
                
        [DllImport(DllName)]
		static extern BassSfxError BASS_SFX_ErrorGetCode();

		/// <summary>
		/// Retrieves the error code for the most recent BASS_SFX function call.
		/// </summary>
		/// <returns>
		/// If no error occured during the last BASS_SFX function call then BASS_SFX_OK is returned, else one of the <see cref="BassSfxError" /> values is returned. 
		/// See the function description for an explanation of what the error code means.
		/// </returns>
		public static BassSfxError LastError => BASS_SFX_ErrorGetCode();

		/// <summary>
		/// Frees all resources used by SFX.
		/// </summary>
		/// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
		/// <remarks>This function should be called before your program exits.</remarks>
		[DllImport(DllName, EntryPoint = "BASS_SFX_Free")]
		public static extern bool Free();

		[DllImport(DllName)]
		static extern int BASS_SFX_GetVersion();

		/// <summary>
		/// Gets the version number of the bass_sfx.dll that is loaded.
		/// </summary>
		public static Version Version => Extensions.GetVersion(BASS_SFX_GetVersion());

        /// <summary>
        /// Initialize the SFX library. This will initialize the library for use.
        /// </summary>
        /// <param name="hInstance">Your application instance handle.</param>
        /// <param name="hWnd">Your main windows form handle.</param>
        /// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned.</returns>
        /// <remarks>
        /// Call this method prior to any other BASS_SFX methods.
        /// <para>Call <see cref="Free" /> to free all resources and before your program exits.</para>
        /// </remarks>
        /// <exception cref="BassSfxError.Already">Already initialized.</exception>
        /// <exception cref="BassSfxError.Memory">There is insufficient memory.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_Init")]
		public static extern bool Init(IntPtr hInstance, IntPtr hWnd);

        /// <summary>
        /// Calls the 'clicked' function of a Sonique visual plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <param name="X">The x coordinate of the click relative to the top left of your visual window.</param>
        /// <param name="Y">The y coordinate of the click relative to the top left of your visual window.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <remarks>
        /// This function is only valid for Sonique Visualization plugins.
        /// You must have created a plugin object using <see cref="PluginCreate" /> before you can use this function.
        /// </remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginClicked")]
		public static extern bool PluginClicked(int Handle, int X, int Y);

        /// <summary>
        /// Shows the configuration dialog window for the SFX plugin (Winamp only).
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Memory">Memory error.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginConfig")]
		public static extern bool PluginConfig(int Handle);

        /// <summary>
        /// Creates a plugin object for use in the SFX.
        /// </summary>
        /// <param name="File">The filename and path of the plugin to load.</param>
        /// <param name="hWnd">The handle of the window where the plugin is to be rendered.</param>
        /// <param name="Width">The initial width for the plugins rendering window.</param>
        /// <param name="Height">The initial height for the plugins rendering window.</param>
        /// <param name="Flags">Any combination of <see cref="BassSfxFlags"/>.</param>
        /// <returns>If successful, the new SFX plugin handle is returned, else 0 is returned.</returns>
        /// <remarks>
        /// Once you create a plugin using this function you can perform a number of different function calls on it.
        /// The <paramref name="Width" /> and <paramref name="Height" /> parameters are only useful for BassBox, WMP, and Sonique plugins.
        /// Winamp ignores these parameters so you can just pass in 0 for winamp plugins.
        /// Windows Media Player plugins can be created using the full path to a dll file or by using the CLSID GUID from the registry.
        /// <para>Mainly you might use the <see cref="PluginStart" />, <see cref="PluginSetStream" /> or <see cref="PluginRender" /> methods.</para>
        /// </remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.FileOpen">Can't open the plugin file.</exception>
        /// <exception cref="BassSfxError.Format">Unsupported plugin format.</exception>
        /// <exception cref="BassSfxError.GUID">Can't open WMP plugin using specified GUID.</exception>
        /// <exception cref="BassSfxError.Memory">There is insufficient memory.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginCreateW", CharSet = CharSet.Unicode)]
		public static extern int PluginCreate(string File, IntPtr hWnd, int Width, int Height, BassSfxFlags Flags);

        /// <summary>
        /// Modifies and/or retrieves a plugin's flags.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <param name="Flags">Any combination of <see cref="BassSfxFlags"/>.</param>
        /// <param name="Mask">The flags (as above) to modify. Flags that are not included in this are left as they are, so it can be set to 0 (<see cref="BassSfxFlags.Default" />) in order to just retrieve the current flags.</param>
        /// <returns>If successful, the plugin's updated flags are returned, else -1 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginFlags")]
		public static extern BassSfxFlags PluginFlags(int Handle, BassSfxFlags Flags, BassSfxFlags Mask);

        /// <summary>
        /// Free a sonique visual plugin and resources from memory.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <remarks>It is very important to call this function after you are done using a plugin to avoid memory leaks.</remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginFree")]
		public static extern bool PluginFree(int Handle);

        /// <summary>
        /// Gets the name of a loaded SFX plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <returns>If successful, the name of the plugin is returned, else <see langword="null" /> is returned.</returns>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Memory">Memory error.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        public static string PluginGetName(int Handle)
		{
			return Marshal.PtrToStringUni(BASS_SFX_PluginGetNameW(Handle));
		}

		[DllImport(DllName)]
		static extern IntPtr BASS_SFX_PluginGetNameW(int Handle);

        /// <summary>
        /// Get the type of visual plugin loaded.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <returns>If successful, the type of plugin is returned.</returns>
        /// <remarks>You must have created a plugin object using <see cref="PluginCreate" /> before you can use this function.</remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginGetType")]
		public static extern BassSfxPlugin PluginGetType(int Handle);

        /// <summary>
        /// Gets the active module for a visual plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <returns>If successful, the active module (zero based index) is returned, else -1 is returned.</returns>
        /// <remarks>
        /// Visual plugins might provide multiple independent modules.
        /// You might get the number of available modules with <see cref="PluginModuleGetCount" />.
        /// However, you can only start/activate one module at a time for a certain visual plugin.
        /// <para>Note: Sonique plugins only ever have 1 module. Winamp plugins can have multiple modules. So this call is really only useful for Winamp.</para>
        /// </remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginModuleGetActive")]
		public static extern int PluginModuleGetActive(int Handle);

        /// <summary>
        /// Gets the number of modules available in the visual plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <returns>If successful, the number of modules is returned, else -1 is returned.</returns>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginModuleGetCount")]
		public static extern int PluginModuleGetCount(int Handle);

        /// <summary>
        /// Returns the name of a certain module of a loaded visual plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <param name="Module">The module number to get the name from (the first module is 0).</param>
        /// <returns>The name of the module on success or <see langword="null" /> on error (or if no module with that number exists).</returns>
        /// <remarks>
        /// Visual plugins might provide multiple independent modules.
        /// You might get the number of available modules with <see cref="PluginModuleGetCount" />.
        /// However, you can only start/activate one module at a time for a certain visual plugin.
        /// <para>Note: Sonique plugins only ever have 1 module. Winamp plugins can have multiple modules. So this call is really only useful for Winamp.</para>
        /// <para>You can use this method in a setup dialog to list all the available modules of a visual plugin.</para>
        /// </remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Memory">Memory error.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        public static string PluginModuleGetName(int Handle, int Module)
		{
			return Marshal.PtrToStringUni(BASS_SFX_PluginModuleGetNameW(Handle, Module));
		}

		[DllImport(DllName)]
		static extern IntPtr BASS_SFX_PluginModuleGetNameW(int Handle, int Module);

        /// <summary>
        /// Sets the active module for a visual plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <param name="Module">The module number to set active (the first module is 0).</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <remarks>
        /// Visual plugins might provide multiple independent modules.
        /// You might get the number of available modules with <see cref="PluginModuleGetCount" />.
        /// However, you can only start/activate one module at a time for a certain visual plugin.
        /// <para>Note: Sonique plugins only ever have 1 module. Winamp plugins can have multiple modules. So this call is really only useful for Winamp.</para>
        /// </remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Memory">Memory error.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginModuleSetActive")]
		public static extern bool PluginModuleSetActive(int Handle, int Module);

        /// <summary>
        /// Renders a Sonique, BassBox or Windows Media Player visual plugin to a device context.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <param name="Channel">The BASS channel to render, can be a HSTREAM or HMUSIC handle.</param>
        /// <param name="hDC">The device context handle of the control to which you want to render the plugin.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <remarks>Only for use with Sonique, BassBox or Windows Media Player plugins.</remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Memory">Memory error.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginRender")]
		public static extern bool PluginRender(int Handle, int Channel, IntPtr hDC);

        /// <summary>
        /// Resizes a visual plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <param name="Width">The new width of your visual window.</param>
        /// <param name="Height">The new height of your visual window.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <remarks>You must have created a plugin object using <see cref="PluginCreate" /> before you can use this function.</remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginResize")]
		public static extern bool PluginResize(int Handle, int Width, int Height);

        /// <summary>
        /// Resizes and moves a visual plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <param name="Left">The new left location of your visual window.</param>
        /// <param name="Top">The new top location of your visual window.</param>
        /// <param name="Width">The new width of your visual window.</param>
        /// <param name="Height">The new height of your visual window.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <remarks>You must have created a plugin object using <see cref="PluginCreate" /> before you can use this function.</remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginResizeMove")]
		public static extern bool PluginResizeMove(int Handle, int Left, int Top, int Width, int Height);

        /// <summary>
        /// Sets a BASS channel on a SFX plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <param name="Channel">The BASS channel to render, can be a HSTREAM or HMUSIC handle.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginSetStream")]
		public static extern bool PluginSetStream(int Handle, int Channel);

        /// <summary>
        /// Starts a visual plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <remarks>Use <see cref="PluginStop" /> to stop a SFX plugin.</remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginStart")]
		public static extern bool PluginStart(int Handle);

        /// <summary>
        /// Stops a visual plugin.
        /// </summary>
        /// <param name="Handle">The SFX plugin handle (as obtained by <see cref="PluginCreate" />).</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <remarks>Use <see cref="PluginStart" /> to start a SFX plugin.</remarks>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Handle">Invalid SFX handle.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_PluginStop")]
		public static extern bool PluginStop(int Handle);

        /// <summary>
        /// Retrieves information on a registered windows media player plugin.
        /// </summary>
        /// <param name="Index">The plugin to get the information of... 0 = first.</param>
        /// <param name="Info"><see cref="BassSfxPluginInfo" /> instance where to store the plugin information at.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Memory">Memory error.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SFX_WMP_GetPluginW")]
		public static extern bool WMPGetPlugin(int Index, out BassSfxPluginInfo Info);

        /// <summary>
        /// Retrieves information on a registered windows media player plugin.
        /// </summary>
        /// <param name="Index">The plugin to get the information of... 0 = first.</param>
        /// <returns>An instance of the <see cref="BassSfxPluginInfo" /> on success, else <see langword="null" />.</returns>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Memory">Memory error.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        public static BassSfxPluginInfo WMPGetPlugin(int Index)
		{
            if (!WMPGetPlugin(Index, out var info))
				throw new BassException();
			
			return info;
		}

        /// <summary>
        /// Returns the total number of WMP plugins currently available for use.
        /// </summary>
        /// <exception cref="BassSfxError.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="BassSfxError.Memory">Memory error.</exception>
        /// <exception cref="BassSfxError.Unknown">Some other mystery problem!</exception>
        public static int WMPGetPluginCount()
		{
            var i = 0;
			
			while (WMPGetPlugin(i, out var info))
				++i;
			
			return i;
		}
	}
}
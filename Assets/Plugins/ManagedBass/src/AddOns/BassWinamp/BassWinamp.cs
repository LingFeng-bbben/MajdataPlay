using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ManagedBass.Winamp
{
    /// <summary>
    /// BassWinamp is a BASS addon adding support for Winamp Input plugins.
    /// </summary>
    /// <remarks> 
    /// BassWinamp doesn't support Bass plugin system.
    /// </remarks>
    public static class BassWinamp
    {
        const string DllName = "bass_winamp";
        
        /// <summary>
        /// Winamp input timeout (in milliseconds) to wait until timing out, because the plugin is not using the output system.
        /// </summary>
        public static int InputTimeout
        {
            get => Bass.GetConfig(Configuration.WinampInputTimeout);
            set => Bass.Configure(Configuration.WinampInputTimeout, value);
        }

        /// <summary>
		/// Shows the about dialog of the loaded Winamp input plug-in.
		/// </summary>
		/// <param name="Handle">The handle of the winamp input plugin.</param>
		/// <param name="Window">The application's main window... <see cref="IntPtr.Zero"/> = the current foreground window.</param>
		[DllImport(DllName, EntryPoint = "BASS_WINAMP_AboutPlugin")]
        public static extern void AboutPlugin(int Handle, IntPtr Window);

        /// <summary>
		/// Shows the configuration dialog of the loaded Winamp input plug-in.
		/// </summary>
		/// <param name="Handle">The handle of the winamp input plugin.</param>
		/// <param name="Window">The application's main window... <see cref="IntPtr.Zero"/> = the current foreground window.</param>
		[DllImport(DllName, EntryPoint = "BASS_WINAMP_ConfigPlugin")]
        public static extern void ConfigPlugin(int Handle, IntPtr Window);

        [DllImport(DllName)]
        static extern IntPtr BASS_WINAMP_FindPlugins(string directory, WinampFindPluginFlags flags);

        /// <summary>
		/// Gets a list of all winamp input plug-ins in a given directory.
		/// <see cref="WinampFindPluginFlags.CommaList"/> is ignored.
		/// </summary>
		/// <param name="PluginDirectory">The path of the directory to search in.</param>
		/// <param name="Flags">Any combination of <see cref="WinampFindPluginFlags"/>.</param>
		public static string[] FindPlugins(string PluginDirectory, WinampFindPluginFlags Flags)
        {
            return Extensions.ExtractMultiStringAnsi(BASS_WINAMP_FindPlugins(PluginDirectory,
                                                                             Flags & (WinampFindPluginFlags.Input | WinampFindPluginFlags.Recursive)));
        }

        [DllImport(DllName)]
        static extern IntPtr BASS_WINAMP_GetExtentions(int handle);

        /// <summary>
        /// Gets the supported file filter extensions of the given winamp input plug-in.
        /// </summary>
        /// <param name="Handle">The handle of the winamp input plugin.</param>
        /// <returns>Returns a array of strings representing the supported extensions of the winamp input plugin.</returns>
        public static string[] GetExtentions(int Handle) => Extensions.ExtractMultiStringAnsi(BASS_WINAMP_GetExtentions(Handle));

        /// <summary>
		/// Returns weather the Winamp input plugin supports seeking?
		/// </summary>
		/// <param name="Handle">The handle of the winamp input plugin.</param>
		/// <returns><see langword="true" /> if the input plugin support seeking, else <see langword="false" /> is returned.</returns>
		[DllImport(DllName, EntryPoint = "BASS_WINAMP_GetIsSeekable")]
        public static extern bool IsSeekable(int Handle);

        [DllImport(DllName)]
        static extern bool BASS_WINAMP_GetFileInfo(string file, [Out] StringBuilder title, out int lenms);

        /// <summary>
        /// Returns information about a given file.
        /// </summary>
        /// <param name="File">The name of the file to retrieve the information from.</param>
        /// <param name="Title">Returns the title of the given file.</param>
        /// <param name="LengthMs">Returns the length of the given file in milliseconds.</param>
        /// <returns><see langword="true" /> if the information was retrieved successfully, else <see langword="false" /> is returned.</returns>
        public static bool GetFileInfo(string File, out string Title, out int LengthMs)
        {
            var title = new StringBuilder(255);
            
            var result = BASS_WINAMP_GetFileInfo(File, title, out LengthMs);
            
            Title = title.ToString();
            
            return result;
        }

        [DllImport(DllName)]
        static extern IntPtr BASS_WINAMP_GetName(int handle);

        /// <summary>
		/// Gets the name of the winamp input plugin.
		/// </summary>
		/// <param name="Handle">The handle of the winamp input plugin.</param>
		/// <returns>The name of the plugin, or <see langword="null" /> if the call failed.</returns>
		public static string GetName(int Handle)
        {
            return Marshal.PtrToStringAnsi(BASS_WINAMP_GetName(Handle));
        }

        /// <summary>
		/// Returns weather the Winamp input plugin make use of the winamp output?
		/// </summary>
		/// <param name="Handle">The handle of the winamp input plugin.</param>
		/// <returns><see langword="true" /> if the input plugin uses output, else <see langword="false" /> is returned.</returns>
		[DllImport(DllName, EntryPoint = "BASS_WINAMP_GetUsesOutput")]
        public static extern bool GetUsesOutput(int Handle);

        /// <summary>
		/// Gets the version of a loaded Winamp input plug-in.
		/// </summary>
		/// <param name="Handle">The handle of the winamp input plugin.</param>
		/// <returns>The version number. HIWORD = Major, LOWORD = Minor.</returns>
		[DllImport(DllName, EntryPoint = "BASS_WINAMP_GetVersion")]
        public static extern int GetVersion(int Handle);

        /// <summary>
		/// Shows the Winamp input plugin information dialog for a given file (like pressing Alt+3 in Winamp).
		/// </summary>
		/// <param name="File">The name of the file to retrieve the information from.</param>
		/// <param name="Window">The application's main window... <see cref="IntPtr.Zero"/> = the current foreground window.</param>
		/// <returns><see langword="true" /> if the info dialog was called successfully, else <see langword="false" /> is returned.</returns>
		[DllImport(DllName, EntryPoint = "BASS_WINAMP_InfoDlg")]
        public static extern bool InfoDialog(string File, IntPtr Window);

        /// <summary>
		/// Loads a Winamp input plug-in.
		/// </summary>
		/// <param name="File">The file name of the plug-in to load.</param>
		/// <returns>The handle of the loaded winamp input plug-in or 0 if it failed.</returns>
		[DllImport(DllName, EntryPoint = "BASS_WINAMP_LoadPlugin")]
        public static extern int LoadPlugin(string File);

        /// <summary>
		/// Unloads a Winamp input plug-in which had been loaded via <see cref="LoadPlugin" /> before.
		/// </summary>
		/// <param name="Handle">The handle of the winamp input plugin.</param>
		[DllImport(DllName, EntryPoint = "BASS_WINAMP_UnloadPlugin")]
        public static extern void UnloadPlugin(int Handle);

        /// <summary>
		/// Creates a stream from a Winamp input plug-in.
		/// </summary>
		/// <param name="FileName">Filename for which a stream should be created.</param>
		/// <param name="Flags">Any combination of <see cref="BassFlags"/>.</param>
		/// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		[DllImport(DllName, EntryPoint = "BASS_WINAMP_StreamCreate")] // Winamp only supports ASCII filenames.
        public static extern int CreateStream(string FileName, BassFlags Flags);
    }
}
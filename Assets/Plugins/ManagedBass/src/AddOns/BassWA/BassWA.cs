using System;
using System.Runtime.InteropServices;

namespace ManagedBass.WA
{
    /// <summary>
    /// BassWA is a BASS AddOn allowing the usage of Winamp visual plugins within your applications.
    /// </summary>
    public static class BassWA
    {
        const string DllName = "bass_wa";
                
        /// <summary>
        /// Starts the visualization plugin and the selected module within the plugin you provide.
        /// </summary>
        /// <param name="Plugin">Plugin Index Number.</param>
        /// <param name="Module">Module Index Number.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_Start_Vis")]
        public static extern void StartVis(int Plugin, int Module);

        /// <summary>
        /// Stops the plugin you select using the index number.
        /// </summary>
        /// <param name="Plugin">Plugin Index Number.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_Stop_Vis")]
        public static extern void StopVis(int Plugin);

        /// <summary>
        /// Opens the plugin configuration window.
        /// </summary>
        /// <param name="Plugin">Plugin Index number.</param>
        /// <param name="Module">Module Index number.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_Config_Vis")]
        public static extern void ConfigVis(int Plugin, int Module);

        /// <summary>
        /// This function is the same as <see cref="LoadVis"/>, except using this function you don't have to provide a plugin index number, the function just loads all possible visual plugins.
        /// This function takes more resources, but makes plugin starting faster.
        /// </summary>
        [DllImport(DllName, EntryPoint = "BASS_WA_LoadAllVis")]
        public static extern void LoadAllVis();

        /// <summary>
        /// Loads a specified plugin. You have to call this funcion before you try to start the plugin using the <see cref="StartVis"/> function.
        /// </summary>
        /// <param name="Plugin">Plugin Index Number.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_LoadVis")]
        public static extern void LoadVis(int Plugin);

        /// <summary>
        /// Frees the resouces used by a loaded plugin.
        /// </summary>
        /// <param name="Plugin">Plugin Index Number.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_FreeVis")]
        public static extern void FreeVis(int Plugin);

        /// <summary>
        /// Frees the VisInfo resouces, this function has to be called after you are done loading the plugin names using the <see cref="GetWinampPluginInfo"/> function.
        /// </summary>
        [DllImport(DllName, EntryPoint = "BASS_WA_FreeVisInfo")]
        public static extern void FreeVisInfo();

        /// <summary>
        /// Gets the total number of modules that are found in the visual plugin.
        /// </summary>
        /// <param name="Plugin">Plugin Index Number.</param>
        /// <returns>the total number of modules that are found in the visual plugin.</returns>
        [DllImport(DllName, EntryPoint = "BASS_WA_GetModuleCount")]
        public static extern int GetModuleCount(int Plugin);
        
        [DllImport(DllName)]
        static extern int BASS_WA_GetWinampPluginCount();

        /// <summary>
        /// Returns the total number of plugins containd in the plugin directory you provide using the <see cref="LoadVisPlugin"/> function.
        /// </summary>
        public static int WinampPluginCount => BASS_WA_GetWinampPluginCount();

        /// <summary>
        /// Sets the hwnd of the window you are using.
        /// It's used for the message boxes and for the plugin to know which window is parent.
        /// </summary>
        /// <param name="Hwnd">The Hwnd of your program's main window.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_SetHwnd")]
        public static extern void SetHwnd(IntPtr Hwnd);

        /// <summary>
        /// Gets the Hwnd of the currently running plugin.
        /// </summary>
        [DllImport(DllName, EntryPoint = "BASS_WA_GetVisHwnd")]
        public static extern IntPtr GetVisHwnd();

        /// <summary>
        /// Let's you send the elepsed time in the in the song to the plugin.
        /// Some plugins display this kind of information while running.
        /// </summary>
        /// <param name="Elapsed">Elapsed time in seconds.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_SetElapsed")]
        public static extern void SetElapsed(int Elapsed);

        /// <summary>
        /// Let's you send the song's total length to the plugin.
        /// Some plugins display this kind of information while running.
        /// </summary>
        /// <param name="Length">The songs total length in seconds.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_SetLength")]
        public static extern void SetLength(int Length);

        /// <summary>
        /// Let's the plugin know that you are playing some music.
        /// </summary>
        /// <param name="Playing">true for playing, false for not playing.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_IsPlaying")]
        public static extern void IsPlaying(bool Playing);

        /// <summary>
        /// Let's you set the module you like to use.
        /// This function has always to be called, even when using single-module plugins.
        /// </summary>
        /// <param name="Module">Module Index Number.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_SetModule")]
        public static extern void SetModule(int Module);

        /// <summary>
        /// Sets the current playing channel BASS uses.
        /// The function has to be called when the channel changes, this happens for example when you load a new file.
        /// </summary>
        /// <param name="Channel">The current playing channel.</param>
        [DllImport(DllName, EntryPoint = "BASS_WA_SetChannel")]
        public static extern void SetChannel(int Channel);

        /// <summary>
        /// Loads all plugins from a path you provide, the function takes one parameter and returns a boolean value to indicate that the load was succesfull.
        /// </summary>
        /// <param name="Path">The path to the plugins you would BASS_WA to load.</param>
        /// <remarks>The plugin names can later be found using the <see cref="GetWinampPluginInfo"/> function.</remarks>
        [DllImport(DllName, CharSet = CharSet.Ansi, EntryPoint = "BASS_WA_LoadVisPlugin")]
        public static extern bool LoadVisPlugin(string Path);

        /// <summary>
        /// Let's you send the song's title to the plugin.
        /// Some plugins display this kind of information while running.
        /// </summary>
        /// <param name="Title">The song's title.</param>
        [DllImport(DllName, CharSet = CharSet.Ansi, EntryPoint = "BASS_WA_SetSongTitle")]
        public static extern void SetSongTitle(string Title);

        [DllImport(DllName)]
        static extern IntPtr BASS_WA_GetModuleInfo(int Plugin, int Module);

        /// <summary>
        /// Gets the name of a module.
        /// </summary>
        /// <param name="Plugin">Plugin Index Number.</param>
        /// <param name="Module">Module Index Number.</param>
        /// <returns>the name of the <paramref name="Module"/>.</returns>
        public static string GetModuleInfo(int Plugin, int Module)
        {
            return Marshal.PtrToStringAnsi(BASS_WA_GetModuleInfo(Plugin, Module));
        }

        [DllImport(DllName)]
        static extern IntPtr BASS_WA_GetWinampPluginInfo(int Module);

        /// <summary>
        /// Gets the name of the plugin index you provide.
        /// </summary>
        /// <param name="Plugin">Module Index Number.</param>
        /// <returns>the name of <paramref name="Plugin"/>.</returns>
        public static string GetWinampPluginInfo(int Plugin)
        {
            return Marshal.PtrToStringAnsi(BASS_WA_GetWinampPluginInfo(Plugin));
        }
    }
}

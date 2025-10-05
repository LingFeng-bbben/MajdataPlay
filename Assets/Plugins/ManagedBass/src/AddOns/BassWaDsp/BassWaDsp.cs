using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ManagedBass.WaDsp
{
    /// <summary>
    /// BassWaDsp provides a set of functions for calling Winamp DSP plugins.
    /// </summary>
    public static class BassWaDsp
    {
        const string DllName = "bass_wadsp";
                
        /// <summary>
        /// Removes a Winamp DSP from the Bass DSP chain which had been set up with <see cref="ChannelSetDSP" /> before.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <returns><see langword="true" />, if successfully removed, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ChannelRemoveDSP")]
        public static extern bool ChannelRemoveDSP(int Plugin);

        /// <summary>
        /// Assigns a loaded Winamp DSP to a standard BASS channel as a new DSP.
        /// <para>This method is pretty close to the <see cref="Bass.ChannelSetDSP" /> method (which is in fact internally used) but instead of setting up a user DSP method the Winamp DSP will be set up.</para>
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Handle">The BASS channel handle (HSTREAM, HMUSIC, or HRECORD) which to assign to the Winamp DSP.</param>
        /// <param name="Priority">The priority of the new DSP, which determines it's position in the Bass DSP chain - DSPs with higher priority are called before those with lower.</param>
        /// <returns>If succesful, then the new DSP's handle (HDSP) is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// This method can and should only be used with those Winamp DSPs which return exactly as much samples as provided - meaning not modifying the samplerate, tempo, pitch etc.!
        /// <para>
        /// The Winamp DSP and this method can be used with 8-bit, 16-bit or float channels.
        /// Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit and float to 16-bit and back will take place.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> or <paramref name="Handle" /> is not a valid handle.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ChannelSetDSP")]
        public static extern int ChannelSetDSP(int Plugin, int Handle, int Priority);

        /// <summary>
        /// Invokes the config dialog of a loaded and started Winamp DSP.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>The Winamp DSP must have been started via <see cref="Start" /> prior to calling this method.</remarks>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        /// <exception cref="Errors.Start"><see cref="Start" /> has not been called before.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_Config")]
        public static extern bool Config(int Plugin);

        /// <summary>
        /// Frees all resources used by BASS_WADSP.
        /// </summary>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been called before.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_Free")]
        public static extern bool Free();

        /// <summary>
        /// Frees and unloads a Winamp DSP library from memory which has been loaded with <see cref="Load(string,int,int,int,int,WinampWinProcedure)" /> before.
        /// </summary>
        /// <param name="Plugin">The plugin handle to unload (as returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>The Winamp DSP will automatically be stopped, if it was started before.</remarks>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_FreeDSP")]
        public static extern bool FreeDSP(int Plugin);

        /// <summary>
        /// Gets the window handle of the fake Winamp window which has been created internally when a Winamp DSP was loaded with <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <returns>An IntPtr representing the window handle of the fake Winamp window or <see cref="IntPtr.Zero" /> if an error occured. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        /// <exception cref="Errors.Unknown">Some other problem (the internal fake Winamp window could not be created).</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_GetFakeWinampWnd")]
        public static extern IntPtr GetFakeWinampWnd(int Plugin);

        /// <summary>
        /// Returns the currently selected plugin module of a Winamp DSP.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <returns>The selected module number (first = 0), or -1, if an error occured. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        /// <exception cref="Errors.Start"><see cref="Start" /> was not called (no module selected so far).</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_GetModule")]
        public static extern int GetModule(int Plugin);

        /// <summary>
        /// Returns the number of modules contained in the loaded Winamp DSP.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <returns>The number of available modules or -1 if an error occured. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>Winamp DSPs might implement multiple different modules within the same DSP plugin.</remarks>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_GetModuleCount")]
        public static extern int GetModuleCount(int Plugin);

        /// <summary>
        /// Returns the name of a certain module of a loaded Winamp DSP.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Module">The module number to get the name from (the first module is 0).</param>
        /// <returns>The name of the module on success or <see langword="null" /> on error (or if no module with that number exists). Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Winamp DSPs might provide multiple independent modules.
        /// You might get the number of available modules with <see cref="GetModuleCount" />.
        /// However, you can only start one module at a time for a certain Winamp DSP.
        /// </remarks>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Module" /> number is invalid.</exception>
        public static string GetModuleName(int Plugin, int Module)
        {
            return Marshal.PtrToStringAnsi(BASS_WADSP_GetModuleName(Plugin, Module));
        }
        
        [DllImport(DllName)]
        static extern IntPtr BASS_WADSP_GetModuleName(int Plugin, int Module);

        /// <summary>
        /// Returns all module names of a Winamp DSP.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <returns>All names of all modules contained in the Winamp DSP.</returns>
        public static IEnumerable<string> GetModuleNames(int Plugin)
        {
            var i = 0;
            var name = string.Empty;

            while (name != null)
            {
                name = GetModuleName(Plugin, i++);
                
                if (name == null)
                    continue;
         
                yield return name;
            }
        }

        /// <summary>
        /// Returns the name of the loaded Winamp DSP plugin.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <returns>The name of the plugin on success or <see langword="null" /> on error. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        public static string GetName(int Plugin)
        {
            return Marshal.PtrToStringAnsi(BASS_WADSP_GetName(Plugin));
        }
        
        [DllImport(DllName)]
        static extern IntPtr BASS_WADSP_GetName(int Plugin);
        
        [DllImport(DllName)]
        static extern int BASS_WADSP_GetVersion();

        /// <summary>
        /// Gets the version of the bass_wadsp.dll that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_WADSP_GetVersion());

        /// <summary>
        /// Initializes BASS_WADSP, call this right after you have called <see cref="Bass.Init" />.
        /// </summary>
        /// <param name="Win">The main window handle of your application or <see cref="IntPtr.Zero" /> for console applications.</param>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Already"><see cref="Init" /> has already been called and can not be called again.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_Init")]
        public static extern bool Init(IntPtr Win = default(IntPtr));

        /// <summary>
        /// Loads a Winamp DSP library.
        /// </summary>
        /// <param name="DspFile">The fully qualified path and name of the Winamp DSP library you want to load.</param>
        /// <param name="X">The X-coordinate of the fake Winamp window to create.</param>
        /// <param name="Y">The Y-coordinate of the fake Winamp window to create.</param>
        /// <param name="Width">The width of the fake Winamp window to create.</param>
        /// <param name="Height">The height of the fake Winamp window to create.</param>
        /// <param name="Procedure">An optional <see cref="WinampWinProcedure" /> which should be used instead of the internal window process message handler. Or <see langword="null" />, if you want BASS_WADSP to handle it by default.</param>
        /// <returns>The handle of the loaded plugin, which is needed in all further method calls.</returns>
        /// <remarks>
        /// Most Winamp DSP plugins do save their location and visibility state in an own .ini file.
        /// So the parameters specifying the location and size are in most cases only uses for the first time a plugin is used.
        /// So don't worry, if they do not take effect.
        /// <para>In most cases the internal Windows message process handler should be fine and sufficient - however, if you encounter problems you might implement your own.</para>
        /// <para>Make sure to call <see cref="FreeDSP" /> when the Winamp DSP is not needed anymore and should be unloaded and removed from memory.</para>
        /// <para>
        /// Winamp DSPs are designed to run only for one stream.
        /// So if you have multiple streams or multiple players in your application and you want to use the same Winamp DSP you need to create temporary copies of the library files.
        /// Then you should load each individual library copy with this function.
        /// This will enable you to load multiple instances of the same Winamp DSP.
        /// Each loaded instance can then be used for individual streams.
        /// </para>
        /// <para>NOTE: Do not use this method while you have already loaded the same Winamp plugin, as this might result in any unexpected behavior, since some Winamp plugins might crash when they are loaded twice.</para>
        /// </remarks>
        /// <exception cref="Errors.FileOpen">The <paramref name="DspFile" /> can not be found or loaded.</exception>
        /// <exception cref="Errors.FileFormat">The <paramref name="DspFile" /> doesn't seem to be a Winamp DSP library file.</exception>
        /// <exception cref="Errors.Unknown">Some other problem (the internal fake Winamp window could not be created).</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_Load", CharSet = CharSet.Unicode)]
        public static extern int Load(string DspFile, int X, int Y, int Width, int Height, WinampWinProcedure Procedure);

        /// <summary>
        /// Invokes the internal 'ModifySamples' method of the Winamp DSP directly (which is only needed for user defined <see cref="DSPProcedure" /> callbacks).
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Buffer">The IntPtr to the memory block containing the sample data to modify.</param>
        /// <param name="Length">The number of bytes contained in the buffer.</param>
        /// <returns>The number of bytes modified, which should always be the number of bytes specified when calling this method. Or 0, if an error occured.</returns>
        /// <remarks>This method can and should only be used with those Winamp DSPs which return exactly as much samples as provided - meaning not modifying the samplerate, tempo, pitch etc.!
        /// <para>This method can be used with 8-bit, 16-bit or float channels. Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit resp. float to 16-bit and back will take place.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ModifySamplesDSP")]
        public static extern int ModifySamplesDSP(int Plugin, IntPtr Buffer, int Length);

        /// <summary>
        /// Invokes the internal 'ModifySamples' method of the Winamp DSP directly (which is only needed for user defined <see cref="DSPProcedure" /> callbacks).
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Buffer">The array of byte values containing the sample data to modify.</param>
        /// <param name="Length">The number of bytes contained in the buffer.</param>
        /// <returns>The number of bytes modified, which should always be the number of bytes specified when calling this method. Or 0, if an error occured.</returns>
        /// <remarks>This method can and should only be used with those Winamp DSPs which return exactly as much samples as provided - meaning not modifying the samplerate, tempo, pitch etc.!
        /// <para>This method can be used with 8-bit, 16-bit or float channels. Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit resp. float to 16-bit and back will take place.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ModifySamplesDSP")]
        public static extern int ModifySamplesDSP(int Plugin, byte[] Buffer, int Length);

        /// <summary>
        /// Invokes the internal 'ModifySamples' method of the Winamp DSP directly (which is only needed for user defined <see cref="DSPProcedure" /> callbacks).
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Buffer">The array of Int16 values containing the sample data to modify.</param>
        /// <param name="Length">The number of bytes contained in the buffer.</param>
        /// <returns>The number of bytes modified, which should always be the number of bytes specified when calling this method. Or 0, if an error occured.</returns>
        /// <remarks>This method can and should only be used with those Winamp DSPs which return exactly as much samples as provided - meaning not modifying the samplerate, tempo, pitch etc.!
        /// <para>This method can be used with 8-bit, 16-bit or float channels. Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit resp. float to 16-bit and back will take place.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ModifySamplesDSP")]
        public static extern int ModifySamplesDSP(int Plugin, short[] Buffer, int Length);

        /// <summary>
        /// Invokes the internal 'ModifySamples' method of the Winamp DSP directly (which is only needed for user defined <see cref="DSPProcedure" /> callbacks).
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Buffer">The array of Int32 values containing the sample data to modify.</param>
        /// <param name="Length">The number of bytes contained in the buffer.</param>
        /// <returns>The number of bytes modified, which should always be the number of bytes specified when calling this method. Or 0, if an error occured.</returns>
        /// <remarks>This method can and should only be used with those Winamp DSPs which return exactly as much samples as provided - meaning not modifying the samplerate, tempo, pitch etc.!
        /// <para>This method can be used with 8-bit, 16-bit or float channels. Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit resp. float to 16-bit and back will take place.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ModifySamplesDSP")]
        public static extern int ModifySamplesDSP(int Plugin, int[] Buffer, int Length);

        /// <summary>
        /// Invokes the internal 'ModifySamples' method of the Winamp DSP directly (which is only needed for user defined <see cref="DSPProcedure" /> callbacks).
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Buffer">The array of float values containing the sample data to modify.</param>
        /// <param name="Length">The number of bytes contained in the buffer.</param>
        /// <returns>The number of bytes modified, which should always be the number of bytes specified when calling this method. Or 0, if an error occured.</returns>
        /// <remarks>This method can and should only be used with those Winamp DSPs which return exactly as much samples as provided - meaning not modifying the samplerate, tempo, pitch etc.!
        /// <para>This method can be used with 8-bit, 16-bit or float channels. Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit resp. float to 16-bit and back will take place.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ModifySamplesDSP")]
        public static extern int ModifySamplesDSP(int Plugin, float[] Buffer, int Length);

        /// <summary>
        /// Invokes the internal 'ModifySamples' method of the Winamp DSP directly (which is only needed for user defined <see cref="DSPProcedure" /> callbacks or in a user defined <see cref="T:Un4seen.Bass.STREAMPROC" />).
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Buffer">The pointer to the memory block containing the sample data to modify.</param>
        /// <param name="Length">The number of bytes contained in the buffer.</param>
        /// <returns>
        /// The number of bytes modified, which might be different from the number of bytes given. 
        /// A Winamp DSP might return at max. twice the number of bytes but not less than half the number of bytes.
        /// </returns>
        /// <remarks>
        /// This method can be used to support Winamp DSPs which modify the samplerate, pitch etc. - meaning modifying the number of bytes given.
        /// However, this is not a simple task to do so, since Bass does not expect this.
        /// In order to handle a modified number of bytes you might need to implement a complex intermediate 'ring-buffer' in between Bass and the Winamp DSP.
        /// Note: Some Winamp DSPs work with a fixed number of 1152 samples only (meaning 1152 * chans * 2 bytes!) - this might also bring in some additional complexity.
        /// Implementing and correctly handling this intermediate 'buffer' is up to you and not handled by BASS_WADSP - however, this method is the right method to use for such a case, since it returns exactly what the Winamp DSP returned.
        /// <para>So make sure, that the buffer is at least twice as big as the samples it contains when you call this method, so that the Winamp DSP might have enough space to store it's returned samples into it.</para>
        /// <para>
        /// This method can be used with 8-bit, 16-bit or float channels.
        /// Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit and float to 16-bit and back will take place.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ModifySamplesSTREAM")]
        public static extern int ModifySamplesStream(int Plugin, IntPtr Buffer, int Length);

        /// <summary>
        /// Invokes the internal 'ModifySamples' method of the Winamp DSP directly (which is only needed for user defined <see cref="DSPProcedure" /> callbacks or in a user defined <see cref="T:Un4seen.Bass.STREAMPROC" />).
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Buffer">The array of byte values containing the sample data to modify.</param>
        /// <param name="Length">The number of bytes contained in the buffer.</param>
        /// <returns>
        /// The number of bytes modified, which might be different from the number of bytes given. 
        /// A Winamp DSP might return at max. twice the number of bytes but not less than half the number of bytes.
        /// </returns>
        /// <remarks>
        /// This method can be used to support Winamp DSPs which modify the samplerate, pitch etc. - meaning modifying the number of bytes given.
        /// However, this is not a simple task to do so, since Bass does not expect this.
        /// In order to handle a modified number of bytes you might need to implement a complex intermediate 'ring-buffer' in between Bass and the Winamp DSP.
        /// Note: Some Winamp DSPs work with a fixed number of 1152 samples only (meaning 1152 * chans * 2 bytes!) - this might also bring in some additional complexity.
        /// Implementing and correctly handling this intermediate 'buffer' is up to you and not handled by BASS_WADSP - however, this method is the right method to use for such a case, since it returns exactly what the Winamp DSP returned.
        /// <para>So make sure, that the buffer is at least twice as big as the samples it contains when you call this method, so that the Winamp DSP might have enough space to store it's returned samples into it.</para>
        /// <para>
        /// This method can be used with 8-bit, 16-bit or float channels.
        /// Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit and float to 16-bit and back will take place.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ModifySamplesSTREAM")]
        public static extern int ModifySamplesStream(int Plugin, byte[] Buffer, int Length);

        /// <summary>
        /// Invokes the internal 'ModifySamples' method of the Winamp DSP directly (which is only needed for user defined <see cref="DSPProcedure" /> callbacks or in a user defined <see cref="T:Un4seen.Bass.STREAMPROC" />).
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Buffer">The array of Int16 values containing the sample data to modify.</param>
        /// <param name="Length">The number of bytes contained in the buffer.</param>
        /// <returns>
        /// The number of bytes modified, which might be different from the number of bytes given. 
        /// A Winamp DSP might return at max. twice the number of bytes but not less than half the number of bytes.
        /// </returns>
        /// <remarks>
        /// This method can be used to support Winamp DSPs which modify the samplerate, pitch etc. - meaning modifying the number of bytes given.
        /// However, this is not a simple task to do so, since Bass does not expect this.
        /// In order to handle a modified number of bytes you might need to implement a complex intermediate 'ring-buffer' in between Bass and the Winamp DSP.
        /// Note: Some Winamp DSPs work with a fixed number of 1152 samples only (meaning 1152 * chans * 2 bytes!) - this might also bring in some additional complexity.
        /// Implementing and correctly handling this intermediate 'buffer' is up to you and not handled by BASS_WADSP - however, this method is the right method to use for such a case, since it returns exactly what the Winamp DSP returned.
        /// <para>So make sure, that the buffer is at least twice as big as the samples it contains when you call this method, so that the Winamp DSP might have enough space to store it's returned samples into it.</para>
        /// <para>
        /// This method can be used with 8-bit, 16-bit or float channels.
        /// Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit and float to 16-bit and back will take place.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ModifySamplesSTREAM")]
        public static extern int ModifySamplesStream(int Plugin, short[] Buffer, int Length);

        /// <summary>
        /// Invokes the internal 'ModifySamples' method of the Winamp DSP directly (which is only needed for user defined <see cref="DSPProcedure" /> callbacks or in a user defined <see cref="T:Un4seen.Bass.STREAMPROC" />).
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Buffer">The array of Int32 values containing the sample data to modify.</param>
        /// <param name="Length">The number of bytes contained in the buffer.</param>
        /// <returns>
        /// The number of bytes modified, which might be different from the number of bytes given. 
        /// A Winamp DSP might return at max. twice the number of bytes but not less than half the number of bytes.
        /// </returns>
        /// <remarks>
        /// This method can be used to support Winamp DSPs which modify the samplerate, pitch etc. - meaning modifying the number of bytes given.
        /// However, this is not a simple task to do so, since Bass does not expect this.
        /// In order to handle a modified number of bytes you might need to implement a complex intermediate 'ring-buffer' in between Bass and the Winamp DSP.
        /// Note: Some Winamp DSPs work with a fixed number of 1152 samples only (meaning 1152 * chans * 2 bytes!) - this might also bring in some additional complexity.
        /// Implementing and correctly handling this intermediate 'buffer' is up to you and not handled by BASS_WADSP - however, this method is the right method to use for such a case, since it returns exactly what the Winamp DSP returned.
        /// <para>So make sure, that the buffer is at least twice as big as the samples it contains when you call this method, so that the Winamp DSP might have enough space to store it's returned samples into it.</para>
        /// <para>
        /// This method can be used with 8-bit, 16-bit or float channels.
        /// Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit and float to 16-bit and back will take place.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ModifySamplesSTREAM")]
        public static extern int ModifySamplesStream(int Plugin, int[] Buffer, int Length);

        /// <summary>
        /// Invokes the internal 'ModifySamples' method of the Winamp DSP directly (which is only needed for user defined <see cref="DSPProcedure" /> callbacks or in a user defined <see cref="T:Un4seen.Bass.STREAMPROC" />).
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Buffer">The array of float values containing the sample data to modify.</param>
        /// <param name="Length">The number of bytes contained in the buffer.</param>
        /// <returns>
        /// The number of bytes modified, which might be different from the number of bytes given. 
        /// A Winamp DSP might return at max. twice the number of bytes but not less than half the number of bytes.
        /// </returns>
        /// <remarks>
        /// This method can be used to support Winamp DSPs which modify the samplerate, pitch etc. - meaning modifying the number of bytes given.
        /// However, this is not a simple task to do so, since Bass does not expect this.
        /// In order to handle a modified number of bytes you might need to implement a complex intermediate 'ring-buffer' in between Bass and the Winamp DSP.
        /// Note: Some Winamp DSPs work with a fixed number of 1152 samples only (meaning 1152 * chans * 2 bytes!) - this might also bring in some additional complexity.
        /// Implementing and correctly handling this intermediate 'buffer' is up to you and not handled by BASS_WADSP - however, this method is the right method to use for such a case, since it returns exactly what the Winamp DSP returned.
        /// <para>So make sure, that the buffer is at least twice as big as the samples it contains when you call this method, so that the Winamp DSP might have enough space to store it's returned samples into it.</para>
        /// <para>
        /// This method can be used with 8-bit, 16-bit or float channels.
        /// Since all Winamp DSPs will internally only work with 16-bit channels an automatic internal conversion from 8-bit and float to 16-bit and back will take place.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_ModifySamplesSTREAM")]
        public static extern int ModifySamplesStream(int Plugin, float[] Buffer, int Length);

        /// <summary>
        /// Free the temporary plugin info resources.
        /// </summary>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>Should and must be called after <see cref="PluginInfoLoad" /> when the plugin info is not needed anymore.</remarks>
        /// <exception cref="Errors.Init"><see cref="PluginInfoLoad" /> has not been called.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_PluginInfoFree")]
        public static extern bool PluginInfoFree();

        /// <summary>
        /// Returns the number of modules contained in the Winamp DSP which has been loaded into the temporary plugin info workspace.
        /// </summary>
        /// <returns>The number of available modules or -1 if an error occured. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>Winamp DSPs might implement multiple different modules within the same DSP plugin.</remarks>
        /// <exception cref="Errors.Init"><see cref="PluginInfoLoad" /> has not been called.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_PluginInfoGetModuleCount")]
        public static extern int PluginInfoGetModuleCount();

        /// <summary>
        /// Returns the name of a certain module of a Winamp DSP which has been loaded into the temporary plugin info workspace.
        /// </summary>
        /// <param name="Module">The module number to get the name from (the first module is 0).</param>
        /// <returns>The name of the module on success or <see langword="null" /> on error (or if no module with that number exists). Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Winamp DSPs might provide multiple independent modules.
        /// You might get the number of available modules with <see cref="PluginInfoGetModuleCount" />.
        /// <para>You can use this method in a setup dialog to list all the available modules of a Winamp DSP.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="PluginInfoLoad" /> has not been called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Module" /> number is invalid.</exception>
        public static string PluginInfoGetModuleName(int Module)
        {
            return Marshal.PtrToStringAnsi(BASS_WADSP_PluginInfoGetModuleName(Module));
        }
        
        [DllImport(DllName)]
        static extern IntPtr BASS_WADSP_PluginInfoGetModuleName(int Module);

        /// <summary>
        /// Returns all module names of a Winamp DSP which has been loaded into the temporary plugin info workspace.
        /// </summary>
        /// <returns>All names of all modules contained in the Winamp DSP.</returns>
        public static IEnumerable<string> PluginInfoGetModuleNames()
        {
            var i = 0;
            var name = string.Empty;

            while (name != null)
            {
                name = PluginInfoGetModuleName(i++);
                
                if (name == null)
                    continue;

                yield return name;
            }
        }

        /// <summary>
        /// Returns the name of the Winamp DSP plugin which has been loaded into the temporary plugin info workspace.
        /// </summary>
        /// <returns>The name of the plugin on success or <see langword="null" /> on error. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Init"><see cref="PluginInfoLoad" /> has not been called.</exception>
        public static string PluginInfoGetName()
        {
            return Marshal.PtrToStringAnsi(BASS_WADSP_PluginInfoGetName());
        }
        
        [DllImport(DllName)]
        static extern IntPtr BASS_WADSP_PluginInfoGetName();

        /// <summary>
        /// Loads a Winamp DSP library into the temporary plugin info workspace.
        /// </summary>
        /// <param name="DspFile">The fully qualified path and name of the Winamp DSP library you want to load.</param>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The temporary plugin info workspace can be used to retrieve general information about a Winamp DSP without starting it.
        /// <para>This method will be used in the same way <see cref="Load(string,int,int,int,int,WinampWinProcedure)" /> is used.</para>
        /// <para>NOTE: Do not use this method while you have already loaded the same Winamp plugin (e.g. via <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />) as this might result in any unexpected behavior, since some Winamp plugins might crash when they are loaded twice.</para>
        /// </remarks>
        /// <exception cref="Errors.FileOpen">The <paramref name="DspFile" /> can not be found or loaded.</exception>
        /// <exception cref="Errors.FileFormat">The <paramref name="DspFile" /> doesn't seem to be a Winamp DSP library file.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_PluginInfoLoad", CharSet = CharSet.Unicode)]
        public static extern bool PluginInfoLoad(string DspFile);

        /// <summary>
        /// Assigns a channel to a Winamp DSP.
        /// <para>You only need this method, if you do NOT use the default <see cref="ChannelSetDSP" /> method, but use your own DSP callback (see <see cref="DSPProcedure" />).</para>
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Handle">The BASS channel handle (HSTREAM, HMUSIC, or HRECORD) which to assign to the Winamp DSP.</param>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>You must use this method when implementing your own <see cref="DSPProcedure" /> callback before starting to play the channel.</remarks>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> or <paramref name="Handle" /> is not a valid handle.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_SetChannel")]
        public static extern bool SetChannel(int Plugin, int Handle);

        /// <summary>
        /// Set the file name for a loaded Winamp DSP plugin.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="File">The file name to set.</param>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Some Winamp DSPs require or simply display a current playing file name.
        /// Use this method to set this file name.
        /// It will then be used in the internal Window message handler for the related fake Winamp windows.
        /// In most cases it is not required to set any file name at all.
        /// </remarks>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_SetFileName", CharSet = CharSet.Unicode)]
        public static extern bool SetFileName(int Plugin, string File);

        /// <summary>
        /// Set the song title for a loaded Winamp DSP plugin.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Title">The song title to set.</param>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Some Winamp DSPs require or simply display a current song title.
        /// Use this method to set this song title.
        /// It will then be used in the internal Window message handler for the related fake Winamp window.
        /// In most cases it is not required to set any song title at all - however streaming DSP might use this for updating it's metadata.
        /// </remarks>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_SetSongTitle", CharSet = CharSet.Unicode)]
        public static extern bool SetSongTitle(int Plugin, string Title);

        /// <summary>
        /// Starts a Winamp DSP.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <param name="Module">The module number to start (the first module is 0).</param>
        /// <param name="Handle">The BASS channel handle (HSTREAM, HMUSIC, or HRECORD) for which to start the Winamp DSP. Or 0 if not applicable.</param>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Winamp DSPs might provide multiple independent modules.
        /// You might get the number of available modules with <see cref="GetModuleCount" /> or the name of a certain module with <see cref="GetModule" />. 
        /// However, you can only start one module at a time for a certain Winamp DSP.
        /// The stream channel is only needed here because some Winamp DSPs might already request some information, which can be provided in this case.
        /// However, if you don't have created a stream so far, just leave the value to 0 and all is fine.
        /// </remarks>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> or the <paramref name="Handle" /> is not a valid handle.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Module" /> is not a valid module.</exception>
        /// <exception cref="Errors.Already">The plugin/module was already started (you need to call <see cref="Stop" /> before starting it again).</exception>
        /// <exception cref="Errors.Unknown">Some other plugin problem (the plugin could not be initialized).</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_Start")]
        public static extern bool Start(int Plugin, int Module, int Handle);

        /// <summary>
        /// Stops a Winamp DSP.
        /// </summary>
        /// <param name="Plugin">The plugin handle (returned by <see cref="Load(string,int,int,int,int,WinampWinProcedure)" />).</param>
        /// <returns><see langword="true" /> on success, else <see langword="false" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>If the Winamp DSP was already assigned to a channel using <see cref="ChannelSetDSP" /> the DSP will be removed automatically from the channel when it is stopped.</remarks>
        /// <exception cref="Errors.Handle">The <paramref name="Plugin" /> is not a valid plugin handle.</exception>
        /// <exception cref="Errors.Start"><see cref="Start" /> has not been called before.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WADSP_Stop")]
        public static extern bool Stop(int Plugin);
    }
}
using System;

namespace ManagedBass.Sfx
{
	/// <summary>
	/// Visual plugin's flags, to be used with <see cref="BassSfx.PluginFlags" />.
	/// </summary>
	[Flags]
	public enum BassSfxFlags
	{
		/// <summary>
		/// Default flags (use GDI rendering).
		/// </summary>
		Default,

		/// <summary>
		/// Render sonique plugins using OpenGL.
		/// </summary>
		SoniqueOpenGL,

        /// <summary>
        /// Use OpenGL double buffering.
        /// </summary>
        SoniqueOpenGLDoubleBuffer
    }
}
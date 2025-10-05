using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Channel DVP type used with a <see cref="DvpProcedure" />.
	/// </summary>
	[Flags]
	public enum BassDShowDvpType
	{
		/// <summary>
		/// RGB24. Rgb24 is a sRGB format with 24 bits per pixel (BPP). Each color channel (red, green, and blue) is allocated 8 bits per pixel (BPP).
		/// </summary>
		RGB24 = 1,
		
		/// <summary>
		/// RGB32. Rgb32 is a sRGB format with 32  bits per pixel (BPP). Each color channel (red, green, and blue) is allocated 8 bits per pixel (BPP).
		/// </summary>
		RGB32,
		
		/// <summary>
		/// YUYV.
		/// </summary>
		YUYV,
		
		/// <summary>
		/// IYUV.
		/// </summary>
		IYUV,
		
		/// <summary>
		/// YVU9.
		/// </summary>
		YVU9,
				
		/// <summary>
		/// YV12.
		/// </summary>
		YV12,
		
		/// <summary>
		/// NV12.
		/// </summary>
		NV12,
		
		/// <summary>
		/// UYVY,
		/// </summary>
		UYVY
	}
}
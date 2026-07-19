using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.i18n
{
    /// <summary>
    /// Specifies the locale identifier (LCID) associated with a font asset field or property.
    /// </summary>
    /// <remarks>
    /// When <see cref="LCID"/> is an empty string, the associated font asset is treated as
    /// the default font and will be used as the fallback when no locale-specific font asset
    /// is available.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class FontLCIDAttribute : Attribute
    {
        /// <summary>
        /// Gets the locale identifier associated with the font asset.
        /// An empty string indicates that the font asset is the default font.
        /// </summary>
        public string LCID { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontLCIDAttribute"/> class.
        /// </summary>
        /// <param name="lcid">
        /// The locale identifier (for example, "zh-CN" or "ja-JP").
        /// Specify <c>null</c> or an empty string to mark the associated font asset
        /// as the default font.
        /// </param>
        public FontLCIDAttribute(string lcid)
        {
            LCID = lcid ?? string.Empty;
        }
    }
}

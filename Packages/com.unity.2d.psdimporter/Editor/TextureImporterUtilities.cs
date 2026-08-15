using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Common;

namespace UnityEditor.U2D.PSD
{
    internal static class TextureImporterUtilities
    {
        public static TextureImporterPlatformSettings GetPlatformTextureSettings(BuildTarget buildTarget, in List<TextureImporterPlatformSettings> platformSettings)
        {
            string buildTargetName = TexturePlatformSettingsHelper.GetBuildTargetGroupName(buildTarget);
            TextureImporterPlatformSettings settings = null;
            settings = platformSettings.SingleOrDefault(x => x.name == buildTargetName && x.overridden == true);
            settings = settings ?? platformSettings.SingleOrDefault(x => x.name == TexturePlatformSettingsHelper.defaultPlatformName);

            if (settings == null)
            {
                settings = new TextureImporterPlatformSettings();
                settings.name = buildTargetName;
                settings.overridden = false;
            }
            return settings;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Common;
using UnityEngine;

namespace UnityEditor.U2D.PSD
{
    internal class PSDImporterEditorExternalData : ScriptableObject
    {
        [SerializeField]
        public List<TextureImporterPlatformSettings> platformSettings = new List<TextureImporterPlatformSettings>();

        public void Init(PSDImporter importer, IList<TextureImporterPlatformSettings> platformSettingsNeeded)
        {
            TextureImporterPlatformSettings[] importerPlatformSettings = importer.GetAllPlatformSettings();

            for (int i = 0; i < importerPlatformSettings.Length; ++i)
            {
                TextureImporterPlatformSettings tip = importerPlatformSettings[i];
                TextureImporterPlatformSettings setting = platformSettings.FirstOrDefault(x => x.name == tip.name);
                if (setting == null)
                {
                    platformSettings.Add(tip);
                }
            }

            foreach (TextureImporterPlatformSettings ps in platformSettingsNeeded)
            {
                TextureImporterPlatformSettings setting = platformSettings.FirstOrDefault(x => x.name == ps.name);
                if (setting == null)
                {
                    platformSettings.Add(ps);
                }
            }
        }
    }
}


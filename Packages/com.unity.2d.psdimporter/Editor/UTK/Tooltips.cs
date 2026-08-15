using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.U2D.PSD
{
    static class Tooltips
    {
        public static readonly string importToggleToolTip = L10n.Tr("Toggle to import layer from Photoshop file.");
        public static readonly string collapseToggleTooltip = L10n.Tr("Toggle to merge a layer group into a single Sprite.");
        public static readonly string layerHiddenToolTip = L10n.Tr("The layer is hidden in the source file.");
        public static readonly string layerEmptyToolTip = L10n.Tr("The layer is empty in the source file.");
        public static readonly string hiddenLayerNotImportWarning = L10n.Tr("Layer will not be imported because hidden layers are excluded from import.");
        public static readonly string emptyLayerNotImportWarning = L10n.Tr("Layer will not be imported because empty layers are excluded from import.");
        public static readonly string groupSeparatedToolTip = L10n.Tr("Layers Separated. Click to merge them.");
        public static readonly string groupMergedToolTip = L10n.Tr("Layers merged. Click to separate them.");
        public static readonly string groupMixedToolTip = L10n.Tr("Group contains child groups that are merged.");
    }
}


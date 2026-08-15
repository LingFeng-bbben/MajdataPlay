using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Unity.VisualScripting
{
    public static class DefineUtility
    {
        private static IEnumerable<BuildTargetGroup> buildTargetGroups
        {
            get
            {
                return Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>().Where
                        (group =>
                        group != BuildTargetGroup.Unknown &&
                        !typeof(BuildTargetGroup).GetField(group.ToString()).HasAttribute<ObsoleteAttribute>()
                        );
            }
        }

        public static bool ToggleDefine(string define, bool enable)
        {
            if (enable)
            {
                return AddDefine(define);
            }
            else
            {
                return RemoveDefine(define);
            }
        }

        private static string GetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup)
        {
#if UNITY_2023_1_OR_NEWER
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            return PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
#endif
        }

        private static void SetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup, string defines)
        {
#if UNITY_2023_1_OR_NEWER
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
#endif
        }

        public static bool AddDefine(string define)
        {
            var added = false;

            foreach (var group in buildTargetGroups)
            {
                var defines = GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();

                if (!defines.Contains(define))
                {
                    defines.Add(define);
                    SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
                    added = true;
                }
            }

            return added;
        }

        public static bool RemoveDefine(string define)
        {
            var removed = false;

            foreach (var group in buildTargetGroups)
            {
                var defines = GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();

                if (defines.Contains(define))
                {
                    defines.Remove(define);
                    SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
                    removed = true;
                }
            }

            return removed;
        }

#if VISUAL_SCRIPT_INTERNAL
        [MenuItem("Tools/Bolt/Internal/Delete All Script Defines", priority = LudiqProduct.DeveloperToolsMenuPriority + 404)]
#endif
        public static void ClearAllDefines()
        {
            foreach (var group in buildTargetGroups)
            {
                SetScriptingDefineSymbolsForGroup(group, string.Empty);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

internal class LinkerCreator : IPreprocessBuildWithReport
{
    private static string linkerPath => Path.Combine(BoltCore.Paths.persistentGenerated, "link.xml");

    public int callbackOrder { get; }

    private static ManagedStrippingLevel GetManagedStrippingLevel(BuildTargetGroup buildTarget)
    {
#if UNITY_2023_1_OR_NEWER
        var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTarget);
        return PlayerSettings.GetManagedStrippingLevel(namedBuildTarget);
#else
        return PlayerSettings.GetManagedStrippingLevel(buildTarget);
#endif
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (VSUsageUtility.isVisualScriptingUsed)
        {
            try
            {
                if (GetManagedStrippingLevel(EditorUserBuildSettings.selectedBuildTargetGroup) !=
                    ManagedStrippingLevel.Disabled)
                {
                    GenerateLinker();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                DeleteLinker();
            }
        }
    }

    [PostProcessBuild]
    private static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (VSUsageUtility.isVisualScriptingUsed)
        {
            DeleteLinker();
        }
    }

    private static void DeleteLinker()
    {
        PathUtility.DeleteProjectFileIfExists(linkerPath, true);
    }

    // Automatically generates the link.xml file to prevent stripping.
    // Currently only used for plugin assemblies, because blanket preserving
    // all setting assemblies sometimes causes the IL2CPP process to fail.
    // For settings assemblies, the AOT stubs are good enough to fool
    // the static code analysis without needing this full coverage.
    // https://docs.unity3d.com/Manual/iphone-playerSizeOptimization.html
    // However, for FullSerializer, we need to preserve our custom assemblies.
    // This is mostly because IL2CPP will attempt to transform non-public
    // property setters used in deserialization into read-only accessors
    // that return false on PropertyInfo.CanWrite, but only in stripped builds.
    // Therefore, in stripped builds, FS will skip properties that should be
    // deserialized without any error (and that took hours of debugging to figure out).
    public static void GenerateLinker()
    {
        var linker = new XDocument();

        var linkerNode = new XElement("linker");

        if (!PluginContainer.initialized)
            PluginContainer.Initialize();

        foreach (var pluginAssembly in PluginContainer.plugins
                     .SelectMany(plugin => plugin.GetType()
                         .GetAttributes<PluginRuntimeAssemblyAttribute>()
                         .Select(a => a.assemblyName))
                     .Distinct())
        {
            var assemblyNode = new XElement("assembly");
            var fullnameAttribute = new XAttribute("fullname", pluginAssembly);
            var preserveAttribute = new XAttribute("preserve", "all");
            assemblyNode.Add(fullnameAttribute);
            assemblyNode.Add(preserveAttribute);
            linkerNode.Add(assemblyNode);
        }

        linker.Add(linkerNode);

        AddCustomNodesToLinker(linkerNode, PluginContainer.plugins);

        PathUtility.CreateDirectoryIfNeeded(BoltCore.Paths.transientGenerated);

        PathUtility.DeleteProjectFileIfExists(linkerPath, true);

        // Using ToString instead of Save to omit the <?xml> declaration,
        // which doesn't appear in the Unity documentation page for the linker.
        File.WriteAllText(linkerPath, linker.ToString());
    }

    private static void AddCustomNodesToLinker(XElement linkerNode, IEnumerable<Plugin> plugins)
    {
        var types = FindAllCustomTypes();

        var customTypes = types.Where(t => !plugins.Any(p => t.Assembly == p.runtimeAssembly));

        foreach (var type in customTypes)
        {
            var userAssembly = type.Assembly.GetName().Name;

            var assemblyNode = new XElement("assembly");
            var fullnameAttribute = new XAttribute("fullname", userAssembly);
            var preserveAttribute = new XAttribute("preserve", "all");

            assemblyNode.Add(fullnameAttribute);

            var customType = new XElement("type");
            var fullnameType = new XAttribute("fullname", type.FullName);

            customType.Add(fullnameType);
            customType.Add(preserveAttribute);

            assemblyNode.Add(customType);

            linkerNode.Add(assemblyNode);
        }
    }

    private static void ProcessSubGraphs(HashSet<Type> types, SubgraphUnit subgraph)
    {
        foreach (var unit in subgraph.nest.graph.units)
        {
            AddTypeToHashSet(types, unit);
        }
    }

    private static void AddTypeToHashSet(HashSet<Type> types, IUnit unit)
    {
        if (unit.GetType() == typeof(SubgraphUnit))
        {
            ProcessSubGraphs(types, (SubgraphUnit)unit);
        }
        else
        {
            types.Add(unit.GetType());
        }
    }

    private static HashSet<Type> FindGraphsOnAssets()
    {
        var scriptGraphAssets = AssetUtility.GetAllAssetsOfType<ScriptGraphAsset>();

        var types = new HashSet<Type>();

        var index = 0;
        var total = scriptGraphAssets.Count();

        foreach (var scriptGraphAsset in scriptGraphAssets)
        {
            if (EditorUtility.DisplayCancelableProgressBar($"Processing on assets {index}/{total}",
                    $"Asset {scriptGraphAsset.name}", (float)index / (float)total))
            {
                break;
            }

            index++;

            if (scriptGraphAsset.graph != null)
            {
                foreach (var unit in scriptGraphAsset.graph.units)
                {
                    AddTypeToHashSet(types, unit);
                }
            }
        }

        EditorUtility.ClearProgressBar();

        return types;
    }

    private static HashSet<Type> FindGraphsOnScenes(bool includeGraphAssets)
    {
        var activeScenePath = SceneManager.GetActiveScene().path;
        var scenePaths = EditorBuildSettings.scenes.Select(s => s.path).ToArray();

        var index = 0;
        var total = scenePaths.Count();
        var types = new HashSet<Type>();

        foreach (var scenePath in scenePaths)
        {
            index++;

            if (EditorUtility.DisplayCancelableProgressBar($"Processing scenes {index} / {total}", $"Scene {scenePath}",
                    (float)index / (float)total))
            {
                break;
            }

            if (!string.IsNullOrEmpty(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                var scriptMachines = UnityObjectUtility.FindObjectsOfTypeIncludingInactive<ScriptMachine>();

                foreach (var scriptMachine in scriptMachines)
                {
                    if (scriptMachine.nest != null &&
                        (scriptMachine.nest.source == GraphSource.Macro && includeGraphAssets) ||
                        scriptMachine.nest.source == GraphSource.Embed)
                    {
                        foreach (var unit in scriptMachine.graph.units)
                        {
                            AddTypeToHashSet(types, unit);
                        }
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(activeScenePath))
        {
            EditorSceneManager.OpenScene(activeScenePath);
        }

        GC.Collect();

        EditorUtility.ClearProgressBar();

        return types;
    }

    private static HashSet<Type> FindGraphsOnPrefabs(bool includeGraphAssets)
    {
        var types = new HashSet<Type>();

        var files = System.IO.Directory.GetFiles(Application.dataPath, "*.prefab",
            System.IO.SearchOption.AllDirectories);

        var index = 0;
        var total = files.Count();

        var currentScene = EditorSceneManager.GetActiveScene();
        var scenePath = currentScene.path;

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

        foreach (var file in files)
        {
            index++;

            if (EditorUtility.DisplayCancelableProgressBar($"Processing prefabs {index} / {total}", $"Prefab {file}",
                    (float)index / (float)total))
            {
                break;
            }

            var prefabPath = file.Replace(Application.dataPath, "Assets");

            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

            if (prefab != null)
            {
                FindGraphInPrefab(types, prefab, includeGraphAssets);
                prefab = null;

                EditorUtility.UnloadUnusedAssetsImmediate(true);
            }
        }

        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        EditorUtility.UnloadUnusedAssetsImmediate(true);

        GC.Collect();

        EditorUtility.ClearProgressBar();

        return types;
    }

    private static void FindGraphInPrefab(HashSet<Type> types, GameObject gameObject, bool includeGraphAssets)
    {
        var scriptMachines = gameObject.GetComponents<ScriptMachine>();

        foreach (var scriptMachine in scriptMachines)
        {
            if (scriptMachine.nest != null &&
                (scriptMachine.nest.source == GraphSource.Macro && includeGraphAssets) ||
                scriptMachine.nest.source == GraphSource.Embed)
            {
                foreach (var unit in scriptMachine.graph.units)
                {
                    AddTypeToHashSet(types, unit);
                }
            }
        }

        foreach (Transform child in gameObject.transform)
        {
            FindGraphInPrefab(types, child.gameObject, includeGraphAssets);
        }
    }

    private static HashSet<Type> FindAllCustomTypes()
    {
        var types = new HashSet<Type>();

        var settings = (List<bool>)BoltCore.Configuration.GetMetadata("LinkerPropertyProviderSettings").value;

        if (settings[(int)BoltCoreConfiguration.LinkerScanTarget.GraphAssets])
        {
            types.AddRange(FindGraphsOnAssets());
        }

        var includeGraphAssets = !settings[(int)BoltCoreConfiguration.LinkerScanTarget.GraphAssets];

        if (settings[(int)BoltCoreConfiguration.LinkerScanTarget.EmbeddedSceneGraphs])
        {
            types.AddRange(FindGraphsOnScenes(includeGraphAssets));
        }

        if (settings[(int)BoltCoreConfiguration.LinkerScanTarget.EmbeddedPrefabGraphs])
        {
            types.AddRange(FindGraphsOnPrefabs(includeGraphAssets));
        }

        return types;
    }
}

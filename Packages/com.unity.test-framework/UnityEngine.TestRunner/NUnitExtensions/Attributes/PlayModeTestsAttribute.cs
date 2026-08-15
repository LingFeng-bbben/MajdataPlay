using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// Marks an assembly as containing PlayMode tests. When present, the Unity Test Framework
    /// classifies the assembly as a PlayMode test assembly regardless of the assembly's
    /// EditorOnly compilation flag.
    ///
    /// The MSBuild-based Unity compilation pipeline emits this attribute into assemblies whose
    /// project sets <c>&lt;IsPlaymodeTest&gt;true&lt;/IsPlaymodeTest&gt;</c>. Projects compiled
    /// via the legacy asmdef pipeline continue to be classified by their <c>includePlatforms</c>
    /// configuration and do not need this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PlayModeTestsAttribute : Attribute
    {
    }
}

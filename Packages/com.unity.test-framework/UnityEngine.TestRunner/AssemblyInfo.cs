using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("UnityEngine.TestRunner")]

[assembly: InternalsVisibleTo("UnityEditor.TestRunner")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Unity.PerformanceTesting")]
[assembly: InternalsVisibleTo("Unity.PerformanceTesting.Editor")]
[assembly: InternalsVisibleTo("Assembly-CSharp-testable")]
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor-testable")]
[assembly: InternalsVisibleTo("UnityEngine.TestRunner.Tests")]
[assembly: InternalsVisibleTo("UnityEditor.TestRunner.Tests")]
[assembly: InternalsVisibleTo("Unity.PackageManagerUI.Editor")]
[assembly: InternalsVisibleTo("Unity.CrossModule.PlayMode.Tests.Editor")]

[assembly: AssemblyVersion("1.0.0")]

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}

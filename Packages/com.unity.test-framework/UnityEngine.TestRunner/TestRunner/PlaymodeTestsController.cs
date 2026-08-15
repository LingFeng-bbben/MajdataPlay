using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.Utils;

namespace UnityEngine.TestTools.TestRunner
{
    [Serializable]
    [AddComponentMenu("")]
    internal class PlaymodeTestsController : MonoBehaviour
    {
        private IEnumerator m_TestSteps;

        public static PlaymodeTestsController ActiveController { get; set; }
        public Exception RaisedException { get; set; }

        [SerializeField]
        private List<string> m_AssembliesWithTests;
        public List<string> AssembliesWithTests
        {
            get
            {
                return m_AssembliesWithTests;
            }
            set
            {
                m_AssembliesWithTests = value;
            }
        }

        [SerializeField]
        internal TestStartedEvent testStartedEvent = new TestStartedEvent();
        [SerializeField]
        internal TestFinishedEvent testFinishedEvent = new TestFinishedEvent();
        [SerializeField]
        internal RunStartedEvent runStartedEvent = new RunStartedEvent();
        [SerializeField]
        internal RunFinishedEvent runFinishedEvent = new RunFinishedEvent();

        //DO NOT change this string, third party code is using this string to identify the test runner game object.
        internal const string kPlaymodeTestControllerName = "Code-based tests runner";

        [SerializeField]
        public PlaymodeTestsControllerSettings settings = new PlaymodeTestsControllerSettings();
        [NonSerialized]
        public bool RunInfrastructureHasRegistered = false;

        internal UnityTestAssemblyRunner m_Runner;
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticsOnLoad()
        {
            ActiveController = null;
        }
#endif
        public IEnumerator Start()
        {
            UnityTestExecutionContext.CurrentContext = new UnityTestExecutionContext()
            {
                FeatureFlags = settings.featureFlags,
                RetryCount = settings.retryCount,
                RepeatCount = settings.repeatCount,
                Automated = settings.automated
            };
            ActiveController = this;
            //Skip 2 frame because Unity.
            yield return null;
            yield return null;

            if (Application.isEditor && !RunInfrastructureHasRegistered)
            {
                yield return null;
            }

            StartCoroutine(Run());
        }

        internal static bool IsControllerOnScene()
        {
            return GameObject.Find(kPlaymodeTestControllerName) != null;
        }

        internal static PlaymodeTestsController GetController()
        {
            return GameObject.Find(kPlaymodeTestControllerName).GetComponent<PlaymodeTestsController>();
        }

        public IEnumerator TestRunnerCoroutine()
        {
            while (true)
            {
                try
                {
                    if (!m_TestSteps.MoveNext())
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    RaisedException = e;
                    throw;
                }

                yield return m_TestSteps.Current;
            }

            if (m_Runner.IsTestComplete)
            {
                runFinishedEvent.Invoke(m_Runner.Result);

                yield return null;
            }
        }

        public IEnumerator Run()
        {
            if (!RunInfrastructureHasRegistered)
            {
                // Wait for the infrastructure to be ready
                yield return null;
            }

            CoroutineTestWorkItem.monoBehaviourCoroutineRunner = this;
            gameObject.hideFlags |= HideFlags.DontSave;

            if (settings.sceneBased)
            {
                SceneManager.LoadScene(1, LoadSceneMode.Additive);
                yield return null;
            }

            PreinitializeNUnitOSPlatformIfNeeded();

            var testListUtil = new PlayerTestAssemblyProvider(new AssemblyLoadProxy(), m_AssembliesWithTests);
            m_Runner = new UnityTestAssemblyRunner(new UnityTestAssemblyBuilder(settings.orderedTestNames, settings.randomOrderSeed), new PlaymodeWorkItemFactory(), UnityTestExecutionContext.CurrentContext);

            var loadedTests = m_Runner.Load(testListUtil.GetUserAssemblies().Select(a => a.Assembly).ToArray(), TestPlatform.PlayMode, UnityTestAssemblyBuilder.GetNUnitTestBuilderSettings(TestPlatform.PlayMode));
            loadedTests.ParseForNameDuplicates();
            runStartedEvent.Invoke(m_Runner.LoadedTest);

            var testListenerWrapper = new TestListenerWrapper(testStartedEvent, testFinishedEvent);
            m_TestSteps = m_Runner.Run(testListenerWrapper, settings.BuildNUnitFilter()).GetEnumerator();

            yield return TestRunnerCoroutine();
        }

        public void Cleanup()
        {
            if (m_Runner != null)
            {
                m_Runner.StopRun();
                m_Runner = null;
            }
            if (Application.isEditor)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        // PLAT-19863: Workaround for NUnit's OSPlatform.CurrentPlatform throwing
        // DllNotFoundException("libc") on Switch2 (and any future platform without libc).
        // NUnit unconditionally invokes [DllImport("libc")] uname() during OS detection,
        // which Switch2 does not provide, causing every TestCaseAttribute.BuildFrom call
        // to mark its fixture as NotRunnable -> "Total tests: 0".
        //
        // We pre-populate NUnit's static Lazy<OSPlatform> via reflection with
        // PlatformID 7 ("Other"; cast from int because the named constant requires
        // .NET 5.0+) so NUnit never invokes the libc PInvoke, and so host-specific
        // [Platform("Win"|"Linux"|"MacOSX"|"XBox")] tests skip on Switch2 (Win32NT
        // would make [Platform("Win")] tests run on Switch2).
        //
        // Failure handling is split into two cases:
        //   A. The OSPlatform type itself is not present in the player's nunit.framework.dll.
        //      This is a legitimate, expected state: IL2CPP managed code stripping does a
        //      transitive reachability pass over user assemblies, and if no reachable code
        //      calls OSPlatform.CurrentPlatform then both the type and the libc PInvoke
        //      call site get stripped together. In that situation the workaround is not
        //      needed (the libc DllImport is unreachable) and we silently return so that
        //      unrelated test projects (e.g. Tests/EditModeAndPlayModeTests/2D/SpriteMask)
        //      are not crashed by us.
        //   B. The OSPlatform type is present but its internals (the static
        //      Lazy<OSPlatform> field, the (PlatformID, Version) ctor, or the .NET
        //      runtime's Lazy<>._value/_state private fields) no longer match what we
        //      expect. This is a true regression signal indicating an NUnit or runtime
        //      upgrade where the workaround is needed but cannot apply, so we throw
        //      InvalidOperationException with a stack trace pointing here.
        //
        // Semantic regressions (reflection succeeds and the value is set, but NUnit
        // now consumes OSPlatform.CurrentPlatform via a path that still triggers the
        // libc PInvoke, or [Platform] attribute gating breaks for some other reason)
        // are caught by the Switch2NUnitOSPlatformSanity test project under
        // PlatformDependent/Switch2/Testing/EditorAndPlaymodeTests/.
        private static void PreinitializeNUnitOSPlatformIfNeeded()
        {
            if (Application.platform != RuntimePlatform.Switch2)
            {
                return;
            }

            var nunitAssembly = typeof(NUnit.Framework.TestAttribute).Assembly;
            var osPlatformType = nunitAssembly.GetType("NUnit.Framework.Internal.OSPlatform");
            if (osPlatformType == null)
            {
                // Case A: type stripped by IL2CPP. Workaround is not needed because the
                // libc PInvoke call site was stripped together with the type.
                return;
            }

            FieldInfo lazyField = null;
            foreach (var field in osPlatformType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Lazy<>))
                {
                    lazyField = field;
                    break;
                }
            }
            if (lazyField == null)
            {
                throw new InvalidOperationException(
                    "[Switch2 libc workaround] No static Lazy<OSPlatform> field found on NUnit.Framework.Internal.OSPlatform. " +
                    "NUnit may have been upgraded; update PreinitializeNUnitOSPlatformIfNeeded.");
            }

            var lazyInstance = lazyField.GetValue(null)
                ?? throw new InvalidOperationException(
                    "[Switch2 libc workaround] Static Lazy<OSPlatform> field returned null. " +
                    "NUnit may have been upgraded; update PreinitializeNUnitOSPlatformIfNeeded.");

            var ctor = osPlatformType.GetConstructor(new[] { typeof(PlatformID), typeof(Version) })
                ?? throw new InvalidOperationException(
                    "[Switch2 libc workaround] NUnit.Framework.Internal.OSPlatform(PlatformID, Version) ctor not found. " +
                    "NUnit may have been upgraded; update PreinitializeNUnitOSPlatformIfNeeded.");
            var safeOSPlatform = ctor.Invoke(new object[] { (PlatformID)7, new Version(0, 0) });

            var lazyType = lazyInstance.GetType();
            var valueField = lazyType.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException(
                    "[Switch2 libc workaround] Lazy<>._value field not found. " +
                    ".NET runtime may have changed Lazy<> internals; update PreinitializeNUnitOSPlatformIfNeeded.");
            var stateField = lazyType.GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException(
                    "[Switch2 libc workaround] Lazy<>._state field not found. " +
                    ".NET runtime may have changed Lazy<> internals; update PreinitializeNUnitOSPlatformIfNeeded.");

            valueField.SetValue(lazyInstance, safeOSPlatform);
            stateField.SetValue(lazyInstance, null);
        }
    }
}

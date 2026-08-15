using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using Component = UnityEngine.Component;

namespace Unity.VisualScripting
{
    /// <summary>
    ///    Component that handles the variables that you can use in the Script Graphs of your project.
    /// </summary>
    /// <remarks>
    ///    <para>This component is automatically added to a GameObject when a <see cref="ScriptMachine"/> is
    ///    added, or when <see cref="SceneVariables"/> (Script) are created using the “Visual Scripting Scene Variables”
    ///    menu in the Hierarchy.</para>
    ///
    ///    <para>The <see cref="Variables"/> component manages the object variables for any GameObject
    ///    that you add it to. You can access these variables from the Object Variables section of the Blackboard
    ///    of any <see cref="ScriptMachine"/> associated with the GameObject.</para>
    ///
    ///    <para>The <see cref="Variables"/> component also manages the scene variables for any “VisualScripting SceneVariables”
    ///    GameObject that you add it to. You can access these scene variables from the Scene Variables
    ///    section of the Blackboard of any <see cref="ScriptMachine"/> you add to this same scene.
    ///    </para>
    ///
    ///    <para><b>Note</b>: It's best not to add a <see cref="ScriptMachine"/> component to a GameObject that also
    ///    includes the <see cref="SceneVariables"/> (Script). If you do this, the Blackboard displays Scene
    ///    Variables that are also present in Object Variables and this can cause confusion.</para>
    ///
    ///    <para>For more information on how to interact with Variables, refer to the <a href="../manual/vs-variables.html">User Manual</a>.</para>
    /// </remarks>
    /// <example>
    ///     <para>The following example shows how to programmatically change the value of a variable used in a Visual Scripting graph.
    ///     Every time you press the Space key, we double the value of the velocity variable.
    ///     <b>Note</b>: You can try this example in a Script Graph.</para>
    ///
    ///     <code source="../../../DocCodeExamples/VariableExamples.cs" region="PlayerController" title="PlayerController"/>
    /// </example>
    [AddComponentMenu("Visual Scripting/Variables")]
    [DisableAnnotation]
    [IncludeInSettings(false)]
    [VisualScriptingHelpURL(typeof(Variables))]
    public class Variables : LudiqBehaviour, IAotStubbable
    {
        /// <summary>
        ///    Retrieves a collection of the variables set in the Variables component.
        /// </summary>
        [Serialize, Inspectable]
        public VariableDeclarations declarations { get; internal set; } =
            new VariableDeclarations() { Kind = VariableKind.Object };

        /// <summary>
        ///    Retrieves a collection of graph variables for a given graph.
        /// </summary>
        /// <param name="pointer">The reference to a graph.</param>
        /// <returns>If the graph is instantiated, returns the graph variables of that instantiated graph.
        /// Otherwise, returns the graph variables from the definition of the given graph (i.e.: from the graph asset definition)</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="pointer" /> is <c>null</c>.
        /// </exception>
        public static VariableDeclarations Graph(GraphPointer pointer)
        {
            Ensure.That(nameof(pointer)).IsNotNull(pointer);

            if (pointer.hasData)
            {
                return GraphInstance(pointer);
            }
            else
            {
                return GraphDefinition(pointer);
            }
        }

        /// <summary>
        ///    Retrieves a collection of graph variables of an instantiated graph.
        /// </summary>
        /// <param name="pointer">The reference to a graph.</param>
        /// <returns>A collection of graph variables of an instantiated graph.</returns>
        /// <exception cref="Unity.VisualScripting.GraphPointerException">
        /// If the graph data cannot be read. Which probably means that the graph is not instantiated.
        /// </exception>
        public static VariableDeclarations GraphInstance(GraphPointer pointer)
        {
            return pointer.GetGraphData<IGraphDataWithVariables>().variables;
        }

        /// <summary>
        ///    Retrieves a collection of graph variables from the definition of a graph.
        /// </summary>
        /// <param name="pointer">The reference to a graph.</param>
        /// <returns>A collection of graph variables of a given graph.</returns>
        public static VariableDeclarations GraphDefinition(GraphPointer pointer)
        {
            return GraphDefinition((IGraphWithVariables)pointer.graph);
        }

        /// <summary>
        ///    Retrieves a collection of graph variables from the definition of a graph.
        /// </summary>
        /// <param name="graph">The reference of a graph</param>
        /// <returns>A collection of graph variables of a given graph.</returns>
        public static VariableDeclarations GraphDefinition(IGraphWithVariables graph)
        {
            return graph.variables;
        }

        /// <summary>
        ///    Retrieves a collection of the object variables of a given <see cref="GameObject"/>.
        /// </summary>
        /// <param name="go">The <see cref="GameObject"/> whose object variables will be returned.</param>
        /// <returns>A collection of the object variables contained in the Variables component of the <see cref="GameObject"/> that was passed as a parameter.</returns>
        /// <remarks>If the <see cref="GameObject"/> doesn't have a Variables component, it is supplied with one by default and the returned collection is empty.</remarks>
        public static VariableDeclarations Object(GameObject go) => go.GetOrAddComponent<Variables>().declarations;

        /// <summary>
        ///    Retrieves a collection of the object variables of a given <see cref="GameObject"/>.
        /// </summary>
        /// <param name="component">The <see cref="Component"/> whose <see cref="GameObject"/>'s object variables are returned.</param>
        /// <returns>A collection of object variables contained in the Variables component of the <see cref="Component"/>'s <see cref="GameObject"/>.</returns>
        /// <remarks>If the GameObject does not have a Variables component, a Variables component is added to the <see cref="GameObject"/> and the returned collection is empty</remarks>
        public static VariableDeclarations Object(Component component) => Object(component.gameObject);

        /// <summary>
        ///    Retrieves a collection of scene variables for a given <see cref="UnityEngine.SceneManagement.Scene">Scene</see>.
        /// </summary>
        /// <param name="scene">The <see cref="UnityEngine.SceneManagement.Scene">Scene</see> whose scene variables are returned.</param>
        /// <returns>A collection of scene variables contained in the Variables component associated with the <see cref="SceneVariables"/> (Script).</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="scene" /> is <c>null</c>.
        /// </exception>
        public static VariableDeclarations Scene(Scene? scene) => SceneVariables.For(scene);

        /// <summary>
        ///    Retrieves a collection of scene variables for a given <see cref="UnityEngine.SceneManagement.Scene">Scene</see>.
        /// </summary>
        /// <param name="go">A <see cref="GameObject"/> whose <see cref="UnityEngine.SceneManagement.Scene">Scene</see> will be accessed to get its variables.</param>
        /// <returns>A collection of scene variables contained in the Variables component associated with the <see cref="SceneVariables"/> (Script).</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the `go.scene` is <c>null</c>.
        /// </exception>
        public static VariableDeclarations Scene(GameObject go) => Scene(go.scene);

        /// <summary>
        ///    Retrieves a collection of scene variables for a given <see cref="UnityEngine.SceneManagement.Scene">Scene</see>.
        /// </summary>
        /// <param name="component">A <see cref="Component"/> whose <see cref="GameObject"/>'s <see cref="UnityEngine.SceneManagement.Scene">Scene</see>'s scene variables will be returned.</param>
        /// <returns>A collection of scene variables contained in the Variables component associated with the <see cref="SceneVariables"/> (Script).</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the `component.go` or `component.go.scene` is <c>null</c>.
        /// </exception>
        public static VariableDeclarations Scene(Component component) => Scene(component.gameObject);

        /// <summary>
        ///    Retrieve a collection of the scene variables of the active scene.
        /// </summary>
        public static VariableDeclarations ActiveScene => Scene(SceneManager.GetActiveScene());

        /// <summary>
        ///    Retrieve a collection of the application variables.
        /// </summary>
        public static VariableDeclarations Application => ApplicationVariables.current;

        /// <summary>
        ///    Retrieve a collection of the saved variables.
        /// </summary>
        public static VariableDeclarations Saved => SavedVariables.current;

        /// <summary>
        ///    Check if a Variables component exists on the <see cref="GameObject"/> passed as a parameter.
        /// </summary>
        /// <param name="go">The <see cref="GameObject"/> we want to check for the Variables component.</param>
        /// <returns>True if the <see cref="GameObject"/> has a Variables component. Otherwise, returns false.</returns>
        public static bool ExistOnObject(GameObject go) => go.GetComponent<Variables>() != null;

        /// <summary>
        ///    Check if a Variables component exists on the <see cref="Component"/>'s <see cref="GameObject"/> passed as a parameter.
        /// </summary>
        /// <param name="component">A <see cref="Component"/> for which we want to know if the <see cref="GameObject"/> has a Variables component.</param>
        /// <returns>True if the <see cref="Component"/>'s <see cref="GameObject"/> has a Variables component. Otherwise, returns false.</returns>
        public static bool ExistOnObject(Component component) => ExistOnObject(component.gameObject);

        /// <summary>
        ///    Check if there is a SceneVariables component instantiated to find out if the scene contains scene variables.
        /// </summary>
        /// <param name="scene">A <see cref="UnityEngine.SceneManagement.Scene">Scene</see> we want to check for scene variables.</param>
        /// <returns>True if the <see cref="UnityEngine.SceneManagement.Scene">Scene</see> is not null and contains scene variables. Otherwise, returns false.</returns>
        public static bool ExistInScene(Scene? scene) => scene != null && SceneVariables.InstantiatedIn(scene.Value);

        /// <summary>
        ///    Check if there is a SceneVariables component instantiated to find out if the active scene contains scene variables.
        /// </summary>
        /// <remarks>Returns true if the active scene contains scene variables. Otherwise, returns false.</remarks>
        public static bool ExistInActiveScene => ExistInScene(SceneManager.GetActiveScene());

        [ContextMenu("Show Data...")]
        protected override void ShowData()
        {
            base.ShowData();
        }

        /// <summary>
        ///    Don't use this method. It is for Unity Visual Scripting internal usage only.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<object> GetAotStubs(HashSet<object> visited)
        {
            // Include the constructors for AOT serialization
            // https://support.ludiq.io/communities/5/topics/3952-x
            foreach (var declaration in declarations)
            {
                var type = declaration.value?.GetType();
                if (type == null)
                    continue;

                // UVSB-2576: When the type is or inherits from AudioMixer (eg: AudioMixerController inherits from AudioMixer), its constructor shouldn't be added to AotStubs:
                // - AudioMixer is a singleton that refers to a specific asset and shouldn't be instantiated.
                // - AudioMixerController has a public constructor but is an Editor type and shouldn't be part of AotStubs.
                const string audioMixerTypeName = "UnityEngine.Audio.AudioMixer";
                const string audioControllerTypeName = "UnityEditor.Audio.AudioMixerController";
                if (!string.IsNullOrEmpty(type.FullName) && (type.FullName.Contains(audioMixerTypeName) || type.FullName.Contains(audioControllerTypeName)))
                    continue;

                var defaultConstructor = type.GetPublicDefaultConstructor();
                if (defaultConstructor != null)
                {
                    yield return defaultConstructor;
                }
            }
        }
    }
}

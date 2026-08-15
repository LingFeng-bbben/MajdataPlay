namespace UnityEngine.TestTools
{
    /// <summary>
    /// Implement this interface if you want to define a set of actions to run as a pre-build step.
    /// </summary>
    public interface IPrebuildSetupWithTestData
    {
        /// <summary>
        /// Implement this method to call actions automatically before the build process.
        /// </summary>
        /// <param name="testData">
        /// The test data associated with the test run that is about to start.
        /// </param>
        /// <example>
        /// <code>
        /// [TestFixture]
        /// public class CreateSpriteTest : IPrebuildSetupWithTestData
        /// {
        ///     Texture2D m_Texture;
        ///     Sprite m_Sprite;
        ///
        ///     public void Setup(TestData testData)
        ///     {
        ///         #if UNITY_EDITOR
        ///         var spritePath = "Assets/Resources/Circle.png";
        ///
        ///         var ti = UnityEditor.AssetImporter.GetAtPath(spritePath) as UnityEditor.TextureImporter;
        ///
        ///         ti.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
        ///
        ///         ti.SaveAndReimport();
        ///         #endif
        ///     }
        ///
        ///     [SetUp]
        ///     public void SetUpTest()
        ///     {
        ///         m_Texture = Resources.Load&lt;Texture2D&gt;("Circle");
        ///     }
        ///
        ///     [Test]
        ///     public void WhenNullTextureIsPassed_CreateShouldReturnNullSprite()
        ///     {
        ///         // Check with Valid Texture.
        ///
        ///         LogAssert.Expect(LogType.Log, "Circle Sprite Created");
        ///
        ///         Sprite.Create(m_Texture, new Rect(0, 0, m_Texture.width, m_Texture.height), new Vector2(0.5f, 0.5f));
        ///
        ///         Debug.Log("Circle Sprite Created");
        ///
        ///         // Check with NULL Texture. Should return NULL Sprite.
        ///         m_Sprite = Sprite.Create(null, new Rect(0, 0, m_Texture.width, m_Texture.height), new Vector2(0.5f, 0.5f));
        ///
        ///         Assert.That(m_Sprite, Is.Null, "Sprite created with null texture should be null");
        ///     }
        /// }
        /// </code>
        /// <para>> **Tip**: Use `#if UNITY_EDITOR` if you want to access Editor only APIs, but the setup/cleanup is inside a **Play Mode** assembly.</para>
        /// </example>
        void Setup(TestData testData);
    }
}

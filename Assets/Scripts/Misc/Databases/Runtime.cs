using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace MajdataPlay.Databases
{
    /// <summary>
    /// Stores runtime configuration data used throughout the application.
    /// <para>This asset is created and managed as a ScriptableObject.</para>
    /// </summary>
    [CreateAssetMenu(fileName = "RuntimeDatabase")]
    public sealed class Runtime : ScriptableObject
    {
        /// <summary>
        /// Colors assigned to each difficulty level.
        /// <para>The array index represents the difficulty identifier.</para>
        /// </summary>
        public ReadOnlySpan<Color> DifficultyColors { get => _difficultyColors; }

        [SerializeField]
        [FormerlySerializedAs("difficultyColors")]
        Color[] _difficultyColors;
    }
}

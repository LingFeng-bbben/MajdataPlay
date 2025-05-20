using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    internal abstract class MajComponent : MonoBehaviour, IMajComponent
    {
        /// <summary>
        /// This property is used to control the life cycle of components derived from MajComponent
        /// <para>it will not modify the activeSelf property of GameObject</para>
        /// </summary>
        public bool Active { get; protected set; } = false;
        public string Tag
        {
            get => _tag;
            set
            {
                _gameObject.tag = value;
                _tag = value;
            }
        }
        /// <summary>
        /// Provides a cached GameObject instance
        /// </summary>
        public GameObject GameObject => _gameObject;
        /// <summary>
        /// Provides a cached Transform instance
        /// </summary>
        public Transform Transform => _transform;

        string _tag = string.Empty;
        GameObject _gameObject;
        Transform _transform;

        protected virtual void Awake()
        {
            _gameObject = gameObject;
            _transform = transform;
            _tag = _gameObject.tag;
        }
        /// <summary>
        /// Sets whether the camera renders this GameObject
        /// </summary>
        /// <param name="state"></param>
        public virtual void SetActive(bool state)
        {
            switch (state)
            {
                case true:
                    _gameObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    _gameObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
            Active = state;
        }
    }
}

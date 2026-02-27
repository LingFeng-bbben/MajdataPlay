using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public int InstanceID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_instanceID == -1)
                {
                    _instanceID = GetInstanceID();
                }
                return _instanceID;
            }
        }
        public string Tag
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _tag;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _gameObject.tag = value;
                _tag = value;
            }
        }
        /// <summary>
        /// Provides a cached GameObject instance
        /// </summary>
        public GameObject GameObject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _gameObject;
            }
        }
        /// <summary>
        /// Provides a cached Transform instance
        /// </summary>
        public Transform Transform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _transform;
            }
        }

        int _instanceID = -1;
        string _tag = string.Empty;
        GameObject _gameObject;
        Transform _transform;

        protected virtual void Awake()
        {
            _gameObject = gameObject;
            _transform = transform;
            _tag = _gameObject.tag;
            _instanceID = GetInstanceID();
        }
        /// <summary>
        /// Sets whether the camera renders this GameObject
        /// </summary>
        /// <param name="state"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetActive(bool state)
        {
            if(state)
            {
                _gameObject.layer = MajEnv.DEFAULT_LAYER;
            }
            else
            {
                _gameObject.layer = MajEnv.HIDDEN_LAYER;
            }
            Active = state;
        }
    }
}

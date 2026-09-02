using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MajdataPlay.Editor.Monitors
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class AnimatorMonitor : MonoBehaviour
    {
        private static readonly List<AnimatorMonitor> Instances = new();

        private Animator _animator;

        private int _frame;
        private bool _updated;
        private float _estimatedTimeMs;

        private AnimatorStateInfo[] _previousStates;
        private AnimatorStateInfo[] _currentStates;

        public Animator Animator => _animator;
        public bool UpdatedThisFrame => _updated;
        public float EstimatedTimeMs => _estimatedTimeMs;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            Register();
        }

        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        private void Register()
        {
            if (!Instances.Contains(this))
                Instances.Add(this);
        }

        internal static IReadOnlyList<AnimatorMonitor> GetInstances()
        {
            return Instances;
        }

        private void LateUpdate()
        {
            if (_animator == null || !_animator.isActiveAndEnabled)
            {
                _updated = false;
                _estimatedTimeMs = 0;
                return;
            }

            int layerCount = _animator.layerCount;

            EnsureStateCapacity(layerCount);

            long start = System.Diagnostics.Stopwatch.GetTimestamp();

            bool changed = false;

            for (int i = 0; i < layerCount; i++)
            {
                AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(i);
                _currentStates[i] = state;

                if (_frame > 0)
                {
                    AnimatorStateInfo previous = _previousStates[i];

                    if (previous.fullPathHash != state.fullPathHash ||
                        previous.shortNameHash != state.shortNameHash ||
                        Mathf.Abs(previous.normalizedTime - state.normalizedTime) > 0.000001f)
                    {
                        changed = true;
                    }
                }
            }

            long end = System.Diagnostics.Stopwatch.GetTimestamp();

            _estimatedTimeMs =
                (end - start) * 1000.0f /
                System.Diagnostics.Stopwatch.Frequency;

            _updated = changed;

            Array.Copy(
                _currentStates,
                _previousStates,
                layerCount);

            _frame++;
        }

        private void EnsureStateCapacity(int count)
        {
            if (_previousStates != null &&
                _previousStates.Length >= count)
                return;

            _previousStates = new AnimatorStateInfo[count];
            _currentStates = new AnimatorStateInfo[count];
        }

        internal static void ClearAll()
        {
            Instances.Clear();
        }
    }
}

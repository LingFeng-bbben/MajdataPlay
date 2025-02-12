using MajdataPlay.Attributes;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    internal abstract class NoteLongDrop : NoteDrop
    {
        public float Length
        {
            get => _length;
            set => _length = value;
        }

        [ReadOnlyField]
        [SerializeField]
        protected float _playerIdleTime = 0;
        [ReadOnlyField]
        [SerializeField]
        protected float _length = 1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float GetRemainingTime() => MathF.Max(Length - GetTimeSpanToJudgeTiming(), 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float GetRemainingTimeWithoutOffset() => MathF.Max(Length - GetTimeSpanToArriveTiming(), 0);
    }
}
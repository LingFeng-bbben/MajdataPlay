using System;
using System.Runtime.CompilerServices;
using MajdataPlay.Editor;
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
        protected float _playerReleaseTime = 0;
        [ReadOnlyField]
        [SerializeField]
        protected float _length = 1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float GetRemainingTime() => MathF.Max(Length - GetTimeSpanToJudgeTiming(), 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float GetRemainingTimeWithoutOffset() => MathF.Max(Length - GetTimeSpanToArriveTiming(), 0);
    }
}
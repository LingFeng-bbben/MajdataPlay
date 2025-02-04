using MajdataPlay.Attributes;
using System;
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

        protected float GetRemainingTime() => MathF.Max(Length - GetTimeSpanToJudgeTiming(), 0);
        protected float GetRemainingTimeWithoutOffset() => MathF.Max(Length - GetTimeSpanToArriveTiming(), 0);
    }
}
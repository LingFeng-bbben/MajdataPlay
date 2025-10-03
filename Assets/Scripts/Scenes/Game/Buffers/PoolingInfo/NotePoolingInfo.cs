using MajdataPlay.Scenes.Game.Notes.Behaviours;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Buffers
{
    internal abstract class NotePoolingInfo
    {
        public int StartPos { get; init; }
        /// <summary>
        /// 当Note转为NoteStatus.Scaling的时间点
        /// </summary>
        public float AppearTiming { get; init; }
        public float Timing { get; init; }
        public int NoteSortOrder { get; init; }
        public float Speed { get; init; }
        public bool IsEach { get; init; }
        public bool IsBreak { get; init; }
        public bool IsEX { get; init; }
        public bool UsingSV { get; init; }
        /// <summary>
        /// 该Info绑定的实例
        /// </summary>
        public NoteDrop? Instance { get; set; }

    }
}

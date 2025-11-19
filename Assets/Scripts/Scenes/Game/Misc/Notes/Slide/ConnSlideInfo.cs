using MajdataPlay.Scenes.Game.Notes;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Slide
{
    public class ConnSlideInfo
    {
        /// <summary>
        /// Indicates the total duration of the Slide Group
        /// </summary>
        public float TotalLength { get; set; }
        ///// <summary>
        ///// Indicates the total length of the Slide Group
        ///// </summary>
        //public float TotalSlideLen { get; set; }
        /// <summary>
        /// Indicates the length of the judgment queue of Slide Group
        /// </summary>
        public long TotalJudgeQueueLen { get; set; }
        /// <summary>
        /// Indicates whether the Slide is at the head of the Group
        /// </summary>
        public bool IsGroupPartHead
        {
            get => IsConnSlide && _isGroupPartHead;
            set => _isGroupPartHead = value;
        }
        /// <summary>
        /// Indicates whether the Slide is in the Group, similar to the property IsConnSlide
        /// </summary>
        public bool IsGroupPart { get; set; }
        /// <summary>
        /// Indicates the start timing of the first Slide of the Group
        /// </summary>
        public float StartTiming { get; set; }
        /// <summary>
        /// Indicates whether the Slide is at the end of the Group
        /// </summary>
        public bool IsGroupPartEnd
        {
            get => IsConnSlide && _isGroupPartEnd;
            set => _isGroupPartEnd = value;
        }
        /// <summary>
        /// Get the Slide instance in front of this Slide
        /// </summary>
        public IConnectableSlide? Parent { get; set; } = null;
        /// <summary>
        /// null
        /// </summary>
        public bool DestroyAfterJudge
        {
            get => IsGroupPartEnd;
        }
        /// <summary>
        /// Indicates whether the current Slide is a ConnSlide
        /// </summary>
        public bool IsConnSlide { get => IsGroupPart; }
        /// <summary>
        /// Get whether the previous slide is finished
        /// </summary>
        public bool ParentFinished
        {
            get
            {
                if (Parent is null)
                    throw new NullReferenceException();
                else
                    return Parent.IsFinished;
            }
        }
        /// <summary>
        /// If there is 1 unfinished judgment area left in the judgment queue, return True
        /// </summary>
        public bool ParentPendingFinish
        {
            get
            {
                if (Parent is null)
                    throw new NullReferenceException();
                else
                    return Parent.IsPendingFinish;
            }
        }
        bool _isGroupPartEnd = false;
        bool _isGroupPartHead = false;

    }
}

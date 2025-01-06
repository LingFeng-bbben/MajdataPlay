using MajdataPlay.Game.Notes;
using MajdataPlay.Interfaces;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Types
{
    public class ConnSlideInfo
    {
        /// <summary>
        /// 表示Slide Group的总时值
        /// </summary>
        public float TotalLength { get; set; }
        /// <summary>
        /// 表示Slide Group的总长度
        /// </summary>
        public float TotalSlideLen { get; set; }
        /// <summary>
        /// 表示Slide Group的判定队列长度
        /// </summary>
        public long TotalJudgeQueueLen { get; set; }
        /// <summary>
        /// 指示该Slide是否位于Group的头部
        /// </summary>
        public bool IsGroupPartHead
        {
            get => IsConnSlide && _isGroupPartHead;
            set => _isGroupPartHead = value;
        }
        /// <summary>
        /// 指示该Slide是否位于Group内
        /// </summary>
        public bool IsGroupPart { get; set; }
        /// <summary>
        /// 指示Group第一条Slide的启动时刻
        /// </summary>
        public float StartTiming { get; set; }
        /// <summary>
        /// 指示该Slide是否位于Group的尾部
        /// </summary>
        public bool IsGroupPartEnd
        {
            get => IsConnSlide && _isGroupPartEnd;
            set => _isGroupPartEnd = value;
        }
        /// <summary>
        /// 获取位于该Slide前方的Slide的GameObject对象
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
        /// 指示当前Slide是否为Connect Slide
        /// </summary>
        public bool IsConnSlide { get => IsGroupPart; }
        /// <summary>
        /// 获取前方Slide是否完成
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
        /// 获取前方Slide是否准备完成
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

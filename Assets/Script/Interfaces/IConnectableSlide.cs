using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Interfaces
{
    public interface IConnectableSlide: IStatefulNote, IEndableNote
    {
        ConnSlideInfo ConnectInfo { get; }
        GameObject GameObject { get; }
        bool IsDestroyed { get; }
        /// <summary>
        /// 如果判定队列已经完成，返回True，反之False
        /// </summary>
        bool IsFinished { get; }
        /// <summary>
        /// 如果判定队列剩余1个未完成判定区，返回True
        /// </summary>
        bool IsPendingFinish { get; }
        /// <summary>
        /// 返回判定队列中未完成判定区的数量
        /// </summary>
        int QueueRemaining { get; }
        /// <summary>
        /// Connection Slide
        /// <para>强制完成该Slide</para>
        /// </summary>
        void ForceFinish();
    }
}

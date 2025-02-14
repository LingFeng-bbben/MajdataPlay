using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Game.Types
{
    public interface IConnectableSlide : IStatefulNote, IEndableNote
    {
        ConnSlideInfo ConnectInfo { get; }
        GameObject GameObject { get; }
        bool IsEnded { get; }
        /// <summary>
        /// If all judgment areas have been completed, return True, otherwise False
        /// </summary>
        bool IsFinished { get; }
        /// <summary>
        /// If there is 1 unfinished judgment area left in the judgment queue, return True
        /// </summary>
        bool IsPendingFinish { get; }
        /// <summary>
        /// Returns the number of unfinished judgment areas in the judgment queue
        /// </summary>
        int QueueRemaining { get; }
        Quaternion FinalStarAngle { get; }
        /// <summary>
        /// Connection Slide
        /// <para>Force finish this Slide</para>
        /// </summary>
        void ForceFinish();
    }
}

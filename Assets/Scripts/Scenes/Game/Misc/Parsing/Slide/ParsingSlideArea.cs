using MajdataPlay.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace MajdataPlay.Scenes.Game.Parsing.Slide
{
    /// <summary>
    /// slide 判定段信息
    /// </summary>
    internal readonly struct ParsingSlideArea
    {
        /// <summary>
        /// 判定段按下以后应当消除的箭头总数，起点已被扣除，最后一个区会比实际箭头数多
        /// </summary>
        public readonly int ArrowProgressPush;

        /// <summary>
        /// 判定段完成以后应当消除的箭头总数，起点已被扣除，最后一个区会比实际箭头数多
        /// </summary>
        public readonly int ArrowProgressFinish;

        /// <summary>
        /// 判定段包含的第一个判定区
        /// </summary>
        public readonly SensorArea SensorA;

        /// <summary>
        /// 判定段包含的第二个判定区
        /// </summary>
        public readonly SensorArea? SensorB;

        public ParsingSlideArea(int progressPush, int progressFinish, SensorArea sensorA, SensorArea? sensorB)
        {
            ArrowProgressPush = progressPush;
            ArrowProgressFinish = progressFinish;
            SensorA = sensorA;
            SensorB = sensorB;
        }
    }
}

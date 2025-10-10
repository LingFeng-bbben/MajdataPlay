using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Scenes.Game
{
    public interface INoteTimeProvider
    {
        float ThisFrameSec { get; }
        float FakeThisFrameSec { get; }
        List<Tuple<float, float>> SVList { get; } //time, sveloc
        List<Func<float, float>> PositionFunctions { get; }

        //计算SV函数
        public void CalcSVPos()
        {
            // 初始化变量
            float lastPosition = 0f;
            float lastTime = 0f;
            float lastSpeed = 1f;

            PositionFunctions.Clear();
            //第一个预留为SV*1
            PositionFunctions.Add((t) => t);
            if (SVList.Count == 1)
            {
                if (SVList[0].Item1 > 0)
                {
                    PositionFunctions.Add((t) => t);
                    lastPosition = SVList[0].Item1;
                    lastTime = SVList[0].Item1;
                }
                PositionFunctions.Add((t) => lastPosition + SVList[0].Item2 * (t - lastTime));
                MajDebug.LogInfo($"Single Segment Case: Start = {lastPosition}, Speed = {SVList[0].Item2}");
                return;
            }
            for (int i = 0; i < SVList.Count - 1; i++)
            {
                float segmentDuration = SVList[i].Item1 - lastTime; // 上一个区间的持续时间
                lastPosition += lastSpeed * segmentDuration; // 计算上一个区间结束时的累积位置
                float speed = SVList[i].Item2; // 当前区间的速度
                lastSpeed = speed; // 更新速度
                lastTime = SVList[i].Item1; // 更新上一个时间点
                                            // 创建分段函数：Position(t) = Position_i + Speed_i * (t - SVTime[i])
                MajDebug.LogInfo($"Segment Case {i}: startTime = {lastTime}, Start = {lastPosition}, Speed = {lastSpeed}");
                float lP = lastPosition;
                float lS = lastSpeed;
                float lT = lastTime;
                float segmentFunction(float t)
                {
                    return lP + lS * (t - lT);
                }
                PositionFunctions.Add(segmentFunction);

            }
            lastPosition += lastSpeed * (SVList[^1].Item1 - lastTime);
            lastTime = SVList[^1].Item1;
            lastSpeed = SVList[^1].Item2;
            float llP = lastPosition;
            float llS = lastSpeed;
            float llT = lastTime;
            PositionFunctions.Add((t) => llP + llS * (t - llT));
            MajDebug.LogInfo($"Segment Case Last: StartTime = {lastTime}, Start = {lastPosition}, Speed = {lastSpeed}");
        }
        public float GetPositionAtTime(float AudioT)
        {
            if (SVList.Count == 0) //无SV修改
                return AudioT;
            if (AudioT < SVList[0].Item1) //在第一个SV修改之前
                return AudioT;
            if (AudioT >= SVList[^1].Item1) //在最后一个SV修改之后
                return PositionFunctions[SVList.Count](AudioT);
            for (int i = 0; i < SVList.Count; i++) //在两个SV修改之间
            {
                if (AudioT < SVList[i].Item1)
                    return PositionFunctions[i](AudioT);
            }
            return PositionFunctions[SVList.Count](AudioT); //理论上不会到这里
        }
    }
}

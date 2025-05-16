using System;
#nullable enable
namespace MajdataPlay.IO
{
    public static class SensorAreaExtensions
    {
        /// <summary>
        /// Gets a touch panel area with a specified difference from the current touch panel area
        /// </summary>
        /// <param name="source">current touch panel area</param>
        /// <param name="diff">specified difference</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">throw exception when the touch panel is not in A1-E8</exception>
        public static SensorArea Diff(this SensorArea source, int diff)
        {
            if (source > SensorArea.E8)
                throw new InvalidOperationException($"\"{source}\" is not a valid touch panel area");
            diff = diff % 8;
            if (diff == 0)
                return source;
            else if (diff < 0)
                diff = 8 + diff;
            //var isReverse = diff < 0;
            var result = (source.GetIndex() - 1 + diff) % 8;
            var group = source.GetGroup();
            switch (group)
            {
                case SensorGroup.A:
                    return (SensorArea)result;
                case SensorGroup.B:
                    result += 8;
                    return (SensorArea)result;
                case SensorGroup.C:
                    return source;
                case SensorGroup.D:
                    result += 17;
                    return (SensorArea)result;
                // SensorGroup.E
                default:
                    result += 25;
                    return (SensorArea)result;
            }
        }
        /// <summary>
        /// Get the group where the touch panel area is located
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">throw exception when the touch panel is not in A1-E8</exception>
        public static SensorGroup GetGroup(this SensorArea source)
        {
            if (source > SensorArea.E8)
                throw new InvalidOperationException($"\"{source}\" is not a valid touch panel area");
            var i = (int)source;
            if (i <= 7)
                return SensorGroup.A;
            else if (i <= 15)
                return SensorGroup.B;
            else if (i <= 16)
                return SensorGroup.C;
            else if (i <= 24)
                return SensorGroup.D;
            else
                return SensorGroup.E;
        }
        /// <summary>
        /// Get the index of the touch panel area within the group
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">throw exception when the touch panel is not in A1-E8</exception>
        public static int GetIndex(this SensorArea source)
        {
            var group = source.GetGroup();
            return group switch
            {
                SensorGroup.A => (int)source + 1,
                SensorGroup.B => (int)source - 7,
                SensorGroup.C => 1,
                SensorGroup.D => (int)source - 16,
                SensorGroup.E => (int)source - 24,
                _ => throw new InvalidOperationException($"\"{source}\" is not a valid touch panel area")
            };
        }
        public static SensorArea Mirror(this SensorArea source, SensorArea baseLine)
        {
            if (source == SensorArea.C || source.IsCollinearWith(baseLine))
                return source;

            var thisIndex = source.GetIndex();
            var baseIndex = baseLine.GetIndex();
            var thisGroup = source.GetGroup();
            var baseGroup = baseLine.GetGroup();

            var AorB = thisGroup is SensorGroup.A or SensorGroup.B && baseGroup is SensorGroup.A or SensorGroup.B;
            var DorE = thisGroup is SensorGroup.D or SensorGroup.E && baseGroup is SensorGroup.D or SensorGroup.E;

            if (AorB || DorE)
            {
                var diff = baseIndex - thisIndex;

                if (thisGroup != baseGroup)
                {
                    var _baseLine = thisGroup switch
                    {
                        SensorGroup.A => (SensorArea)(baseIndex - 1),
                        SensorGroup.B => (SensorArea)(baseIndex - 1 + 8),
                        SensorGroup.D => (SensorArea)(baseIndex - 1 + 17),
                        SensorGroup.E => (SensorArea)(baseIndex - 1 + 25),
                        _ => throw new NotSupportedException("cnm")
                    };
                    return _baseLine.Diff(diff);
                }
                else
                    return baseLine.Diff(diff);
            }
            else
            {
                switch (baseLine)
                {
                    case SensorArea.D1:
                    case SensorArea.E1:
                        return source switch
                        {
                            SensorArea.A1 => SensorArea.A8,
                            SensorArea.A2 => SensorArea.A7,
                            SensorArea.A3 => SensorArea.A6,
                            SensorArea.A4 => SensorArea.A5,
                            SensorArea.A5 => SensorArea.A4,
                            SensorArea.A6 => SensorArea.A3,
                            SensorArea.A7 => SensorArea.A2,
                            SensorArea.A8 => SensorArea.A1,
                            SensorArea.B1 => SensorArea.B8,
                            SensorArea.B2 => SensorArea.B7,
                            SensorArea.B3 => SensorArea.B6,
                            SensorArea.B4 => SensorArea.B5,
                            SensorArea.B5 => SensorArea.B4,
                            SensorArea.B6 => SensorArea.B3,
                            SensorArea.B7 => SensorArea.B2,
                            SensorArea.B8 => SensorArea.B1,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.D2:
                    case SensorArea.E2:
                        return source switch
                        {
                            SensorArea.A2 => SensorArea.A1,
                            SensorArea.A3 => SensorArea.A8,
                            SensorArea.A4 => SensorArea.A7,
                            SensorArea.A5 => SensorArea.A6,
                            SensorArea.A6 => SensorArea.A5,
                            SensorArea.A7 => SensorArea.A4,
                            SensorArea.A8 => SensorArea.A3,
                            SensorArea.A1 => SensorArea.A2,
                            SensorArea.B2 => SensorArea.B1,
                            SensorArea.B3 => SensorArea.B8,
                            SensorArea.B4 => SensorArea.B7,
                            SensorArea.B5 => SensorArea.B6,
                            SensorArea.B6 => SensorArea.B5,
                            SensorArea.B7 => SensorArea.B4,
                            SensorArea.B8 => SensorArea.B3,
                            SensorArea.B1 => SensorArea.B2,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.D3:
                    case SensorArea.E3:
                        return source switch
                        {
                            SensorArea.A3 => SensorArea.A2,
                            SensorArea.A4 => SensorArea.A1,
                            SensorArea.A5 => SensorArea.A8,
                            SensorArea.A6 => SensorArea.A7,
                            SensorArea.A7 => SensorArea.A6,
                            SensorArea.A8 => SensorArea.A5,
                            SensorArea.A1 => SensorArea.A4,
                            SensorArea.A2 => SensorArea.A3,
                            SensorArea.B3 => SensorArea.B2,
                            SensorArea.B4 => SensorArea.B1,
                            SensorArea.B5 => SensorArea.B8,
                            SensorArea.B6 => SensorArea.B7,
                            SensorArea.B7 => SensorArea.B6,
                            SensorArea.B8 => SensorArea.B5,
                            SensorArea.B1 => SensorArea.B4,
                            SensorArea.B2 => SensorArea.B3,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.D4:
                    case SensorArea.E4:
                        return source switch
                        {
                            SensorArea.A4 => SensorArea.A3,
                            SensorArea.A5 => SensorArea.A2,
                            SensorArea.A6 => SensorArea.A1,
                            SensorArea.A7 => SensorArea.A8,
                            SensorArea.A8 => SensorArea.A7,
                            SensorArea.A1 => SensorArea.A6,
                            SensorArea.A2 => SensorArea.A5,
                            SensorArea.A3 => SensorArea.A4,
                            SensorArea.B4 => SensorArea.B3,
                            SensorArea.B5 => SensorArea.B2,
                            SensorArea.B6 => SensorArea.B1,
                            SensorArea.B7 => SensorArea.B8,
                            SensorArea.B8 => SensorArea.B7,
                            SensorArea.B1 => SensorArea.B6,
                            SensorArea.B2 => SensorArea.B5,
                            SensorArea.B3 => SensorArea.B4,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.D5:
                    case SensorArea.E5:
                        return source switch
                        {
                            SensorArea.A5 => SensorArea.A4,
                            SensorArea.A6 => SensorArea.A3,
                            SensorArea.A7 => SensorArea.A2,
                            SensorArea.A8 => SensorArea.A1,
                            SensorArea.A1 => SensorArea.A8,
                            SensorArea.A2 => SensorArea.A7,
                            SensorArea.A3 => SensorArea.A6,
                            SensorArea.A4 => SensorArea.A5,
                            SensorArea.B5 => SensorArea.B4,
                            SensorArea.B6 => SensorArea.B3,
                            SensorArea.B7 => SensorArea.B2,
                            SensorArea.B8 => SensorArea.B1,
                            SensorArea.B1 => SensorArea.B8,
                            SensorArea.B2 => SensorArea.B7,
                            SensorArea.B3 => SensorArea.B6,
                            SensorArea.B4 => SensorArea.B5,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.D6:
                    case SensorArea.E6:
                        return source switch
                        {
                            SensorArea.A6 => SensorArea.A5,
                            SensorArea.A7 => SensorArea.A4,
                            SensorArea.A8 => SensorArea.A3,
                            SensorArea.A1 => SensorArea.A2,
                            SensorArea.A2 => SensorArea.A1,
                            SensorArea.A3 => SensorArea.A8,
                            SensorArea.A4 => SensorArea.A7,
                            SensorArea.A5 => SensorArea.A6,
                            SensorArea.B6 => SensorArea.B5,
                            SensorArea.B7 => SensorArea.B4,
                            SensorArea.B8 => SensorArea.B3,
                            SensorArea.B1 => SensorArea.B2,
                            SensorArea.B2 => SensorArea.B1,
                            SensorArea.B3 => SensorArea.B8,
                            SensorArea.B4 => SensorArea.B7,
                            SensorArea.B5 => SensorArea.B6,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.D7:
                    case SensorArea.E7:
                        return source switch
                        {
                            SensorArea.A7 => SensorArea.A6,
                            SensorArea.A8 => SensorArea.A5,
                            SensorArea.A1 => SensorArea.A4,
                            SensorArea.A2 => SensorArea.A3,
                            SensorArea.A3 => SensorArea.A2,
                            SensorArea.A4 => SensorArea.A1,
                            SensorArea.A5 => SensorArea.A8,
                            SensorArea.A6 => SensorArea.A7,
                            SensorArea.B7 => SensorArea.B6,
                            SensorArea.B8 => SensorArea.B5,
                            SensorArea.B1 => SensorArea.B4,
                            SensorArea.B2 => SensorArea.B3,
                            SensorArea.B3 => SensorArea.B2,
                            SensorArea.B4 => SensorArea.B1,
                            SensorArea.B5 => SensorArea.B8,
                            SensorArea.B6 => SensorArea.B7,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.D8:
                    case SensorArea.E8:
                        return source switch
                        {
                            SensorArea.A8 => SensorArea.A7,
                            SensorArea.A1 => SensorArea.A6,
                            SensorArea.A2 => SensorArea.A5,
                            SensorArea.A3 => SensorArea.A4,
                            SensorArea.A4 => SensorArea.A3,
                            SensorArea.A5 => SensorArea.A2,
                            SensorArea.A6 => SensorArea.A1,
                            SensorArea.A7 => SensorArea.A8,
                            SensorArea.B8 => SensorArea.B7,
                            SensorArea.B1 => SensorArea.B6,
                            SensorArea.B2 => SensorArea.B5,
                            SensorArea.B3 => SensorArea.B4,
                            SensorArea.B4 => SensorArea.B3,
                            SensorArea.B5 => SensorArea.B2,
                            SensorArea.B6 => SensorArea.B1,
                            SensorArea.B7 => SensorArea.B8,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.A1:
                    case SensorArea.B1:
                        return source switch
                        {
                            SensorArea.D1 => SensorArea.D2,
                            SensorArea.D2 => SensorArea.D1,
                            SensorArea.D3 => SensorArea.D8,
                            SensorArea.D4 => SensorArea.D7,
                            SensorArea.D5 => SensorArea.D6,
                            SensorArea.D6 => SensorArea.D5,
                            SensorArea.D7 => SensorArea.D4,
                            SensorArea.D8 => SensorArea.D3,
                            SensorArea.E1 => SensorArea.E2,
                            SensorArea.E2 => SensorArea.E1,
                            SensorArea.E3 => SensorArea.E8,
                            SensorArea.E4 => SensorArea.E7,
                            SensorArea.E5 => SensorArea.E6,
                            SensorArea.E6 => SensorArea.E5,
                            SensorArea.E7 => SensorArea.E4,
                            SensorArea.E8 => SensorArea.E3,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.A2:
                    case SensorArea.B2:
                        return source switch
                        {
                            SensorArea.D3 => SensorArea.D2,
                            SensorArea.D4 => SensorArea.D1,
                            SensorArea.D5 => SensorArea.D8,
                            SensorArea.D6 => SensorArea.D7,
                            SensorArea.D7 => SensorArea.D6,
                            SensorArea.D8 => SensorArea.D5,
                            SensorArea.D1 => SensorArea.D4,
                            SensorArea.D2 => SensorArea.D3,
                            SensorArea.E3 => SensorArea.E2,
                            SensorArea.E4 => SensorArea.E1,
                            SensorArea.E5 => SensorArea.E8,
                            SensorArea.E6 => SensorArea.E7,
                            SensorArea.E7 => SensorArea.E6,
                            SensorArea.E8 => SensorArea.E5,
                            SensorArea.E1 => SensorArea.E4,
                            SensorArea.E2 => SensorArea.E3,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.A3:
                    case SensorArea.B3:
                        return source switch
                        {
                            SensorArea.D4 => SensorArea.D3,
                            SensorArea.D5 => SensorArea.D2,
                            SensorArea.D6 => SensorArea.D1,
                            SensorArea.D7 => SensorArea.D8,
                            SensorArea.D8 => SensorArea.D7,
                            SensorArea.D1 => SensorArea.D6,
                            SensorArea.D2 => SensorArea.D5,
                            SensorArea.D3 => SensorArea.D4,
                            SensorArea.E4 => SensorArea.E3,
                            SensorArea.E5 => SensorArea.E2,
                            SensorArea.E6 => SensorArea.E1,
                            SensorArea.E7 => SensorArea.E8,
                            SensorArea.E8 => SensorArea.E7,
                            SensorArea.E1 => SensorArea.E6,
                            SensorArea.E2 => SensorArea.E5,
                            SensorArea.E3 => SensorArea.E4,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.A4:
                    case SensorArea.B4:
                        return source switch
                        {
                            SensorArea.D5 => SensorArea.D4,
                            SensorArea.D6 => SensorArea.D3,
                            SensorArea.D7 => SensorArea.D2,
                            SensorArea.D8 => SensorArea.D1,
                            SensorArea.D1 => SensorArea.D8,
                            SensorArea.D2 => SensorArea.D7,
                            SensorArea.D3 => SensorArea.D6,
                            SensorArea.D4 => SensorArea.D5,
                            SensorArea.E5 => SensorArea.E4,
                            SensorArea.E6 => SensorArea.E3,
                            SensorArea.E7 => SensorArea.E2,
                            SensorArea.E8 => SensorArea.E1,
                            SensorArea.E1 => SensorArea.E8,
                            SensorArea.E2 => SensorArea.E7,
                            SensorArea.E3 => SensorArea.E6,
                            SensorArea.E4 => SensorArea.E5,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.A5:
                    case SensorArea.B5:
                        return source switch
                        {
                            SensorArea.D6 => SensorArea.D5,
                            SensorArea.D7 => SensorArea.D4,
                            SensorArea.D8 => SensorArea.D3,
                            SensorArea.D1 => SensorArea.D2,
                            SensorArea.D2 => SensorArea.D1,
                            SensorArea.D3 => SensorArea.D8,
                            SensorArea.D4 => SensorArea.D7,
                            SensorArea.D5 => SensorArea.D6,
                            SensorArea.E6 => SensorArea.E5,
                            SensorArea.E7 => SensorArea.E4,
                            SensorArea.E8 => SensorArea.E3,
                            SensorArea.E1 => SensorArea.E2,
                            SensorArea.E2 => SensorArea.E1,
                            SensorArea.E3 => SensorArea.E8,
                            SensorArea.E4 => SensorArea.E7,
                            SensorArea.E5 => SensorArea.E6,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.A6:
                    case SensorArea.B6:
                        return source switch
                        {
                            SensorArea.D7 => SensorArea.D6,
                            SensorArea.D8 => SensorArea.D5,
                            SensorArea.D1 => SensorArea.D4,
                            SensorArea.D2 => SensorArea.D3,
                            SensorArea.D3 => SensorArea.D2,
                            SensorArea.D4 => SensorArea.D1,
                            SensorArea.D5 => SensorArea.D8,
                            SensorArea.D6 => SensorArea.D7,
                            SensorArea.E7 => SensorArea.E6,
                            SensorArea.E8 => SensorArea.E5,
                            SensorArea.E1 => SensorArea.E4,
                            SensorArea.E2 => SensorArea.E3,
                            SensorArea.E3 => SensorArea.E2,
                            SensorArea.E4 => SensorArea.E1,
                            SensorArea.E5 => SensorArea.E8,
                            SensorArea.E6 => SensorArea.E7,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.A7:
                    case SensorArea.B7:
                        return source switch
                        {
                            SensorArea.D8 => SensorArea.D7,
                            SensorArea.D1 => SensorArea.D6,
                            SensorArea.D2 => SensorArea.D5,
                            SensorArea.D3 => SensorArea.D4,
                            SensorArea.D4 => SensorArea.D3,
                            SensorArea.D5 => SensorArea.D2,
                            SensorArea.D6 => SensorArea.D1,
                            SensorArea.D7 => SensorArea.D8,
                            SensorArea.E8 => SensorArea.E7,
                            SensorArea.E1 => SensorArea.E6,
                            SensorArea.E2 => SensorArea.E5,
                            SensorArea.E3 => SensorArea.E4,
                            SensorArea.E4 => SensorArea.E3,
                            SensorArea.E5 => SensorArea.E2,
                            SensorArea.E6 => SensorArea.E1,
                            SensorArea.E7 => SensorArea.E8,
                            _ => throw new NotSupportedException()
                        };
                    case SensorArea.A8:
                    case SensorArea.B8:
                        return source switch
                        {
                            SensorArea.D1 => SensorArea.D8,
                            SensorArea.D2 => SensorArea.D7,
                            SensorArea.D3 => SensorArea.D6,
                            SensorArea.D4 => SensorArea.D5,
                            SensorArea.D5 => SensorArea.D4,
                            SensorArea.D6 => SensorArea.D3,
                            SensorArea.D7 => SensorArea.D2,
                            SensorArea.D8 => SensorArea.D1,
                            SensorArea.E1 => SensorArea.E8,
                            SensorArea.E2 => SensorArea.E7,
                            SensorArea.E3 => SensorArea.E6,
                            SensorArea.E4 => SensorArea.E5,
                            SensorArea.E5 => SensorArea.E4,
                            SensorArea.E6 => SensorArea.E3,
                            SensorArea.E7 => SensorArea.E2,
                            SensorArea.E8 => SensorArea.E1,
                            _ => throw new NotSupportedException()
                        };
                    default:
                        throw new NotSupportedException();
                }
            }

        }
        public static bool IsCollinearWith(this SensorArea source, SensorArea target)
        {
            var thisGroup = source.GetGroup();
            var targetGroup = target.GetGroup();
            if (thisGroup is SensorGroup.C || targetGroup is SensorGroup.C)
                return true;

            var thisIndex = source.GetIndex();
            var targetIndex = target.GetIndex();

            if (thisGroup is SensorGroup.A or SensorGroup.B && targetGroup is SensorGroup.A or SensorGroup.B)
                return thisIndex == targetIndex || Math.Abs(thisIndex - targetIndex) == 4;
            else if (thisGroup is SensorGroup.D or SensorGroup.E && targetGroup is SensorGroup.D or SensorGroup.E)
                return thisIndex == targetIndex || Math.Abs(thisIndex - targetIndex) == 4;
            else
                return false;
        }
        public static bool IsLeftOf(this SensorArea source, SensorArea target)
        {
            if (source == SensorArea.C || target == SensorArea.C)
                throw new InvalidOperationException("cnm");
            else if (source.IsCollinearWith(target))
                return false;

            var opposite = target.Diff(4);
            var thisIndex = source.GetIndex();
            var aIndex = target.GetIndex();
            var bIndex = opposite.GetIndex();
            var min = Math.Min(aIndex, bIndex);
            var max = Math.Max(aIndex, bIndex);

            var thisGroup = source.GetGroup();
            var targetGroup = target.GetGroup();

            var AorB = thisGroup is SensorGroup.A or SensorGroup.B && targetGroup is SensorGroup.A or SensorGroup.B;
            var DorE = thisGroup is SensorGroup.D or SensorGroup.E && targetGroup is SensorGroup.D or SensorGroup.E;
            if (AorB || DorE)
            {
                if (thisIndex > min && thisIndex < max)
                    return false;
                else
                    return true;
            }
            else
            {
                if (targetGroup is SensorGroup.A or SensorGroup.B)
                {
                    if (thisIndex > min && thisIndex <= max)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (thisIndex >= min && thisIndex < max)
                        return false;
                    else
                        return true;
                }
            }
        }
        public static bool IsRightOf(this SensorArea source, SensorArea target)
        {
            if (source == SensorArea.C || target == SensorArea.C)
                throw new InvalidOperationException("cnm");
            else if (source.IsCollinearWith(target))
                return false;
            else
                return !source.IsLeftOf(target);
        }
    }
}
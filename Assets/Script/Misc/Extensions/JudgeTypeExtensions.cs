using MajdataPlay.Types;
using System.Runtime.CompilerServices;
#nullable enable
namespace MajdataPlay.Extensions
{
    public static class JudgeTypeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMissOrTooFast(this JudgeGrade source)
        {
            return source is (JudgeGrade.Miss or JudgeGrade.TooFast);
        }
    }
}
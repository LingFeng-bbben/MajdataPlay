using MajdataPlay.Game.Notes;
using System.Runtime.CompilerServices;
#nullable enable
namespace MajdataPlay.Game
{
    public static class JudgeGradeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMissOrTooFast(this JudgeGrade source)
        {
            return source is JudgeGrade.Miss or JudgeGrade.TooFast;
        }
    }
}
using MajdataPlay.Scenes.Game.Notes;
using System.Runtime.CompilerServices;
#nullable enable
namespace MajdataPlay.Scenes.Game
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
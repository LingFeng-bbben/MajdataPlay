using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;

namespace MajdataPlay.Game
{
    public class LoadJustSprite : MonoBehaviour
    {
        public int _0curv1str2wifi;

        public int indexOffset;
        public int judgeOffset = 0;

        public void SetResult(JudgeGrade result)
        {
            var displayCP = MajInstances.Setting.Display.DisplayCriticalPerfect;
            switch (result)
            {
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.Perfect:
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.FastPerfect2:
                    if (displayCP)
                        SetJustCP();
                    else
                        SetJustP();
                    break;
                case JudgeGrade.FastGreat2:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat:
                    SetFastGr();
                    break;
                case JudgeGrade.FastGood:
                    SetFastGd();
                    break;
                case JudgeGrade.LateGood:
                    SetLateGd();
                    break;
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat2:
                case JudgeGrade.LateGreat:
                    SetLateGr();
                    break;
                case JudgeGrade.TooFast:
                    SetTooFast();
                    break;
                default:
                    SetMiss();
                    break;
            }
        }
        public int SetR()
        {
            indexOffset = 0;
            RefreshSprite();
            return _0curv1str2wifi;
        }
        public int SetL()
        {
            indexOffset = 3;
            RefreshSprite();
            return _0curv1str2wifi;
        }
        public void SetJustCP()
        {
            judgeOffset = 0;
            RefreshSprite();
        }
        public void SetJustP()
        {
            judgeOffset = 6;
            RefreshSprite();
        }
        public void SetFastP()
        {
            judgeOffset = 12;
            RefreshSprite();
        }
        public void SetFastGr()
        {
            judgeOffset = 18;
            RefreshSprite();
        }
        public void SetFastGd()
        {
            judgeOffset = 24;
            RefreshSprite();
        }
        public void SetLateP()
        {
            judgeOffset = 30;
            RefreshSprite();
        }
        public void SetLateGr()
        {
            judgeOffset = 36;
            RefreshSprite();
        }
        public void SetLateGd()
        {
            judgeOffset = 42;
            RefreshSprite();
        }
        public void SetMiss()
        {
            judgeOffset = 48;
            RefreshSprite();
        }
        public void SetTooFast()
        {
            judgeOffset = 54;
            RefreshSprite();
        }
        private void RefreshSprite()
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = MajInstances.SkinManager.SelectedSkin.Just[_0curv1str2wifi + indexOffset + judgeOffset];
        }
    }
}
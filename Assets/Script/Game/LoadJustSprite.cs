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

        public void SetResult(JudgeType result)
        {
            var displayCP = MajInstances.Setting.Display.DisplayCriticalPerfect;
            switch (result)
            {
                case JudgeType.LatePerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.Perfect:
                case JudgeType.FastPerfect1:
                case JudgeType.FastPerfect2:
                    if (displayCP)
                        SetJustCP();
                    else
                        SetJustP();
                    break;
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                    SetFastGr();
                    break;
                case JudgeType.FastGood:
                    SetFastGd();
                    break;
                case JudgeType.LateGood:
                    SetLateGd();
                    break;
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.LateGreat:
                    SetLateGr();
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
        private void RefreshSprite()
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = MajInstances.SkinManager.SelectedSkin.Just[_0curv1str2wifi + indexOffset + judgeOffset];
        }
    }
}
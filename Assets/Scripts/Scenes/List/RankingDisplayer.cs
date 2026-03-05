using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MajdataPlay
{
    public class RankingDisplayer : MonoBehaviour
    {
        public GameObject NamePannel;
        public GameObject ScorePannel;
        public TMP_Text[] PlayerNames;
        public TMP_Text[] Scores;
        public string[] NameTemplates;
        public string ScoresTemplate;
        // Start is called before the first frame update
        void Start()
        {
            HidePannels();
        }

        public void HidePannels()
        {
            NamePannel.SetActive(false);
            ScorePannel.SetActive(false);
        }
        public void SetScores(ReadOnlySpan<MajNetSongScore> scores)
        {
            if(scores.IsEmpty)
            {
                HidePannels();
                return;
            }
            NamePannel.SetActive(true);
            ScorePannel.SetActive(true);

            for (var i = 0; i < PlayerNames.Length; i++)
            {
                PlayerNames[i].text = string.Empty;
                Scores[i].text = string.Empty;
            }

            for (var i = 0; i < scores.Length && i < 3; i++)
            {
                ref readonly var score = ref scores[i];
                PlayerNames[i].text = string.Format(NameTemplates[i], score.Player.Username);
                var @int = MathF.Truncate(score.Acc);
                var @float = score.Acc - @int;
                Scores[i].text = string.Format(ScoresTemplate, @int, @float);
            }
        }
    }
}

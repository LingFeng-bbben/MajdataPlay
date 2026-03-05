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
            hidePannels();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void hidePannels()
        {
            NamePannel.SetActive(false);
            ScorePannel.SetActive(false);
        }
        void showScores(/* scores */)
        {
            NamePannel.SetActive(true);
            ScorePannel.SetActive(true);
            //foreach scores...
        }
    }
}

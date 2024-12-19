using MajSimaiDecode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XCharts.Runtime;

public class ChartAnalyzer : MonoBehaviour
{
    LineChart lineChart;
    void Start()
    {
        lineChart = GetComponent<LineChart>();
    }

    public void AnalyzeMaidata(SimaiProcess data, float totalLength)
    {
        lineChart.ClearData();
        for (float time=0;time<totalLength;time += 0.5f)
        {
            var timingPoints = data.notelist.FindAll(o=> o.time>time-0.75f&&o.time<=time+0.75f).ToList();
            float y0 = 0,y1 =0,y2=0;
            foreach (var timingPoint in timingPoints)
            {
                foreach (var note in timingPoint.noteList)
                {
                    if (note.noteType == SimaiNoteType.Tap || note.noteType == SimaiNoteType.Hold)
                    {
                        y0++;
                    }
                    else if (note.noteType == SimaiNoteType.Slide)
                    {
                        y1 += 3;
                    }
                    else if (note.noteType == SimaiNoteType.Touch || note.noteType == SimaiNoteType.TouchHold)
                    {
                        y2++;
                    }
                }

            }

            var x = time/totalLength;
            lineChart.series[0].AddXYData(x, y0);
            lineChart.series[1].AddXYData(x, y1);
            lineChart.series[2].AddXYData(x, y2);
        }
    }
}

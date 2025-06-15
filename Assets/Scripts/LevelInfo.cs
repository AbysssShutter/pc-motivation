using System;
using System.Collections.Generic;
using System.Diagnostics;

[Serializable]
public class LevelInfo
{
    public string levelName;
    public float levelBpm;
    public string trackPath;
    public string waveformPath;
    public string backgroundPath = null;
    public bool isPlayable = false;
    public float summary;
    public float generalEstimation;
    public int notesTotal;
    public int notesRegistered;
    public List<int> pressRecord = null;

    public string CalculateAndSaveScore(int currentNotesRegistered, List<int> currentPressRecord)
    {
        float currentSummary = currentPressRecord[0] * 0.5f +
            currentPressRecord[1] * 1 +
            currentPressRecord[2] * -0.5f +
            currentPressRecord[3] * -1;
        float currentEstim = 0;
        if (currentSummary / currentNotesRegistered > 0)
        {
            currentEstim = currentSummary / currentNotesRegistered * 10;
        }

        if (((pressRecord == null || pressRecord.Count < 1) && currentSummary / currentNotesRegistered > 0)
            || currentSummary > summary && currentNotesRegistered >= notesRegistered)
        {
            generalEstimation = currentEstim;
            summary = currentSummary;
            pressRecord = currentPressRecord;
            notesRegistered = currentNotesRegistered;
        }
        return "currentSumm:" + currentSummary + "befSum:" + summary; 
    }
}

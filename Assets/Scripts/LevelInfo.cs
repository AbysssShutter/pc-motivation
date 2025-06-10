using System;
using System.Collections.Generic;

[Serializable]
public class LevelInfo
{
    public string levelName;
    public float levelBpm;
    public string trackPath;
    public string waveformPath;
    public string backgroundPath = null;
    public bool isPlayable = false;
    public int summary;
    public int notesTotal;
    public int notesRegistered;
    public List<int> pressRecord;

    public void CalculateAndSaveScore(int currentNotesRegistered, List<int> currentPressRecord)
    {
        int currentSummary = (int)Math.Round(currentPressRecord[0] * -0.5f +
            currentPressRecord[1] * 2 +
            currentPressRecord[2] * 0.5f +
            currentPressRecord[3] * -1);

        if (pressRecord.Count < 1)
        {
            summary = currentSummary;
            pressRecord = currentPressRecord;
            notesRegistered = currentNotesRegistered;
        }
        else if (currentSummary > summary && currentNotesRegistered > notesRegistered)
        {
            summary = currentSummary;
            pressRecord = currentPressRecord;
            notesRegistered = currentNotesRegistered;
        }
    }
}

using UnityEngine;
using TMPro;

public class TrackScript : MonoBehaviour
{
    private string trackPath;
    [SerializeField] private TextMeshProUGUI trackNameDisplay;
    public void SetTrackName(string name)
    {
        trackNameDisplay.text = name;
    }
    public void SetTrackPath(string path)
    {
        trackPath = path;
    }
    public void SetTrackAsPlaying()
    {
        FindFirstObjectByType<MusicPlayer>().SetNewTrack(trackPath, trackNameDisplay.text);
    }
    public string GetTrackPath()
    {
        return trackPath;
    }
}

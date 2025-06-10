using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LevelDataDisplay : MonoBehaviour
{
    private string levelName;
    private float levelBPM;
    [SerializeField] private TextMeshProUGUI levelNameDisplay;
    [SerializeField] private TextMeshProUGUI levelBPMDisplay;
    [SerializeField] private Button playButton;

    public void PlayLevel() {
        PlayerPrefs.SetString("CurrentLevelName", levelName);
        SceneManager.LoadScene("PlayScene");
    }

    public void EditLevel() {
        PlayerPrefs.SetString("CurrentLevelName", levelName);
        SceneManager.LoadScene("EditScene");
    }

    public void DeleteLevel()
    {
        SaveLoadInfo.DeleteLevelCompletely(levelName);
        FindFirstObjectByType<LevelsDataManager>().RefreshLevelsList();
    }

    public void SetLevelDisplayData(string name, float bpm, bool isPlayable)
    {
        levelName = name;
        levelBPM = bpm;
        levelNameDisplay.SetText(levelName);
        levelBPMDisplay.SetText(levelBPM.ToString());
        if (isPlayable)
        {
            playButton.interactable = true;
        }
        else
        {
            playButton.interactable = false;
        }
    }
}

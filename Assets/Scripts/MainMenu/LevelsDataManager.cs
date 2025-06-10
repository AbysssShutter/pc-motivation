using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using AnotherFileBrowser.Windows;
using System;
using System.Threading.Tasks;

public class LevelsDataManager : MonoBehaviour
{
    [SerializeField] private GameObject levelDataTemplate;
    [SerializeField] private GameObject dataContainer;
    public AudioImporter importer;

    //Объекты экрана добавления уровня
    [SerializeField] private Animator newLevelScreenAnimator;
    [SerializeField] private GameObject newLevelPanel;
    [SerializeField] private TMP_InputField newLevelName;
    [SerializeField] private TMP_InputField newLevelBPM;
    [SerializeField] private TextMeshProUGUI newLevelTrackPath;
    [SerializeField] private GameObject errorAnnouncement;
    [SerializeField] private Button newLevelTrackSelect;
    [SerializeField] private Button confirmNewLevel;
    void OnEnable()
    {
        RefreshLevelsList();
    }

    public void CheckNewLevelParams()
    {
        if (newLevelName.text.Length > 0)
        {
            if (SaveLoadInfo.DoesThisLevelNameExist(newLevelName.text))
            {
                //Добавить анимацию
            }
            else
            {
                if (newLevelBPM.text.Length > 0
                    && newLevelTrackPath.text.Length > 0
                    && newLevelTrackPath.text != "Song Path...")
                {
                    confirmNewLevel.interactable = true;
                }
            }
        }
        else
        {
            //Добавить активацию анимации
            confirmNewLevel.interactable = false;
        }
    }

    public void ConfirmNewLevel()
    {
        LevelInfo newLevel = new LevelInfo();
        newLevel.levelName = newLevelName.text;
        newLevel.levelBpm = int.Parse(newLevelBPM.text);
        newLevel.trackPath = newLevelTrackPath.text;
        newLevel.waveformPath = SaveLoadInfo.GenerateAndSaveWaveform(newLevel.levelName, newLevel.trackPath);
        
        SaveLoadInfo.SaveLevelInfo(newLevel);

        newLevelName.text = "";
        newLevelBPM.text = "";
        newLevelTrackPath.text = "Song Path...";
        //Добавить активацию анимации
        confirmNewLevel.interactable = false;
        OpenCloseNewLevelTab();

        FindFirstObjectByType<MusicPlayer>().RefreshTrackList();
        RefreshLevelsList();
    }

    public void SetTrackText()
    {
        BrowserProperties bp = new BrowserProperties();
        bp.filter = "Audio Files |*.mp3";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, result =>
        {
            newLevelTrackPath.text = result;
        });
        CheckNewLevelParams();
    }

    public void CreateNewLevel()
    {
        newLevelName.text = "";
        newLevelBPM.text = "";
        newLevelTrackPath.text = "Song Path...";
        confirmNewLevel.interactable = false;
        OpenCloseNewLevelTab();
    }

    public void RefreshLevelsList()
    {
        foreach (Transform child in dataContainer.transform)
        {
            Destroy(child.gameObject);
        }
        List<LevelInfo> levelInfo = SaveLoadInfo.GetAllLevelsHighData();
        if (levelInfo != null)
        {
            foreach (LevelInfo level in levelInfo)
            {
                GameObject newLevelTemplate = Instantiate(levelDataTemplate);
                newLevelTemplate.GetComponent<LevelDataDisplay>().SetLevelDisplayData(level.levelName, level.levelBpm, level.isPlayable);
                newLevelTemplate.transform.SetParent(dataContainer.transform, false);
            }
        }
    }

    private void OpenCloseNewLevelTab()
    {
        newLevelScreenAnimator.SetBool("idle", !newLevelScreenAnimator.GetBool("idle"));
    }

    private void ShowNameError()
    {
        newLevelScreenAnimator.SetBool("nameError", true);
    }
}

//Потестить, что будет, если папки с уровнями не существует на момент
//загрузки списка уровней
//Потестить, что будет если название папки уровня отличается от названия уровня в файле
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using AnotherFileBrowser.Windows;
using DG.Tweening;

public class LevelsDataManager : MonoBehaviour
{
    [SerializeField] private GameObject levelDataTemplate;
    [SerializeField] private GameObject dataContainer;
    public AudioImporter importer;

    //Объекты экрана добавления уровня
    [SerializeField] private GameObject newLevelPanel;
    [SerializeField] private TMP_InputField newLevelName;
    [SerializeField] private TMP_InputField newLevelBPM;
    [SerializeField] private TextMeshProUGUI newLevelTrackPath;
    [SerializeField] private GameObject errorAnnouncement;
    [SerializeField] private Button newLevelTrackSelect;
    [SerializeField] private Button confirmNewLevel;

    [SerializeField] private TMP_Text summaryScore;
    [SerializeField] private TMP_Text missed;
    [SerializeField] private TMP_Text perfect;
    [SerializeField] private TMP_Text got;
    [SerializeField] private TMP_Text meh;
    [SerializeField] private TMP_Text totalNotes;
    [SerializeField] private TMP_Text pressedNotes;
    [SerializeField] private Image appBg;
    private Tween tween;
    private Tween panelTween;
    private Tween errorTween;
    private bool isPanelOpened = false;
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
                ConfirmButtonAction(false);
                StartCoroutine(ShowNameError());
            }
            else
            {
                if (newLevelBPM.text.Length > 0
                    && newLevelTrackPath.text.Length > 0
                    && newLevelTrackPath.text != "Song Path...")
                {
                    ConfirmButtonAction(true);
                }
            }
        }
        else
        {
            ConfirmButtonAction(false);
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
        ConfirmButtonAction(false);
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
        ConfirmButtonAction(false);
        OpenCloseNewLevelTab();
    }

    public void RefreshLevelsList()
    {
        FindFirstObjectByType<MusicPlayer>().RefreshTrackList();
        foreach (Transform child in dataContainer.transform)
        {
            Destroy(child.gameObject);
        }
        System.Collections.Generic.List<LevelInfo> levelInfo = SaveLoadInfo.GetAllLevelsHighData();
        if (levelInfo != null)
        {
            foreach (LevelInfo level in levelInfo)
            {
                GameObject newLevelTemplate = Instantiate(levelDataTemplate);
                newLevelTemplate.GetComponent<LevelDataDisplay>().SetLevelDisplayData(
                    level.levelName,
                    level.levelBpm,
                    level.isPlayable,
                    level.generalEstimation,
                    level.notesTotal,
                    level.notesRegistered,
                    level.pressRecord,
                    level.backgroundPath,
                    level.trackPath);
                newLevelTemplate.transform.SetParent(dataContainer.transform, false);
            }
        }
    }


    public void DeselectAll()
    {
        foreach (LevelDataDisplay child in dataContainer.GetComponentsInChildren<LevelDataDisplay>())
        {
            child.RevertColor();
        }
    }

    public void SetLevelCompletionInfo(float estimation,
                                                            int notesTotal,
                                                            int notesRegistered,
                                                            System.Collections.Generic.List<int> pressRecord,
                                                            string bgPath)
    {
        if (estimation > 8)
        {
            summaryScore.SetText("S");
        }
        else if (estimation > 6 && estimation < 8)
        {
            summaryScore.SetText("A");
        }
        else if (estimation > 4 && estimation < 6)
        {
            summaryScore.SetText("B");
        }
        else if (estimation > 2 && estimation < 4)
        {
            summaryScore.SetText("C");
        }
        else if (estimation > 0 && estimation < 2)
        { 
            summaryScore.SetText("D");
        }
        totalNotes.text = notesTotal.ToString();
        pressedNotes.text = notesRegistered.ToString();

        perfect.text = pressRecord[1].ToString();
        got.text = pressRecord[0].ToString();
        meh.text = pressRecord[2].ToString();
        missed.text = pressRecord[3].ToString();
        ChangeBGTo(bgPath);
    }
    public void SetMinimalCompletionInfo(string bgPath, int notesTotal)
    {
        summaryScore.text = "?";
        pressedNotes.text = "?";
        perfect.text = "?";
        got.text = "?";
        meh.text = "?";
        missed.text = "?";

        totalNotes.text = notesTotal.ToString();

        ChangeBGTo(bgPath);
    }

    public void ResetCompletionInfo()
    {
        summaryScore.text = "?";
        pressedNotes.text = "?";
        perfect.text = "?";
        got.text = "?";
        meh.text = "?";
        missed.text = "?";
        totalNotes.text = "?";

        DOTween.Kill(tween);
        tween = appBg.DOColor(new Color(195, 195, 195, 0), 2f);
    }

    private void ChangeBGTo(string bgPath)
    {
        DOTween.Kill(tween);
        if (bgPath == null || bgPath.Length < 1)
        {
            tween = appBg.DOColor(new Color(0.8f, 0.8f, 0.8f, 0), 1f);
        }
        else
        {
            if (appBg.color.a == 0)
            {
                tween = appBg.DOColor(new Color(0.8f, 0.8f, 0.8f, 0), 1f);
            }
            appBg.DOColor(new Color(0.6f, 0.6f, 0.6f, 0.7f), 1f);
            appBg.sprite = SaveLoadInfo.LoadImageFromPath(bgPath);
        }
    }

    private void OpenCloseNewLevelTab()
    {
        DOTween.Kill(panelTween);
        if (!isPanelOpened)
        {
            panelTween = newLevelPanel.GetComponent<RectTransform>().DOAnchorPosY(150, 1f);
        }
        else
        {
            panelTween = newLevelPanel.GetComponent<RectTransform>().DOAnchorPosY(-150, 1f);
        }
        isPanelOpened = !isPanelOpened;
    }

    private IEnumerator ShowNameError()
    {
        errorTween = errorAnnouncement.GetComponent<RectTransform>().DOAnchorPosY(20, 1f);
        yield return errorTween.WaitForCompletion();
        yield return new WaitForSeconds(1.5f);
        errorTween = errorAnnouncement.GetComponent<RectTransform>().DOAnchorPosY(-25, 1f);
    }

    private void ConfirmButtonAction(bool toShow)
    {
        confirmNewLevel.interactable = toShow;
        if (toShow)
        {
            confirmNewLevel.GetComponent<RectTransform>().DOAnchorPosY(25, 1f);
        }
        else
        {
            confirmNewLevel.GetComponent<RectTransform>().DOAnchorPosY(-25, 1f);
        }
    }

    public void SetSelectedLevelTrack(string path, string name)
    {
        FindFirstObjectByType<MusicPlayer>().SetNewTrack(path, name);
    }
}
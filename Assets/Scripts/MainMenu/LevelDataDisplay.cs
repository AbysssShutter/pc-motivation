using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

public class LevelDataDisplay : MonoBehaviour, IPointerDownHandler
{
    private string levelName;
    private float levelBPM;
    private string trackPath;
    private float estimation = 0;
    private int notesTotal = 0;
    private int notesRegistered = 0;
    private System.Collections.Generic.List<int> pressRecord = null;
    private bool isPlayedBefore;
    private string levelBackgroundPath;
    private Color defaultColor;
    private Color selectedColor = new Color(0.85f, 0.85f, 0.85f);
    private LevelsDataManager manager;
    [SerializeField] private TextMeshProUGUI levelNameDisplay;
    [SerializeField] private TextMeshProUGUI levelBPMDisplay;
    [SerializeField] private Button playButton;

    void Start()
    {
        defaultColor = GetComponent<Image>().color;
        manager = FindFirstObjectByType<LevelsDataManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        manager.DeselectAll();
        StopAllCoroutines();
        StartCoroutine(FadeColorFromTo(GetComponent<Image>().color, selectedColor));
        if (isPlayedBefore)
        {
            manager.SetLevelCompletionInfo(
                estimation,
                notesTotal,
                notesRegistered,
                pressRecord,
                levelBackgroundPath);
        }
        else
        {
            manager.SetMinimalCompletionInfo(levelBackgroundPath, notesTotal);
        }
        manager.SetSelectedLevelTrack(trackPath, levelName);
    }

    public void RevertColor()
    {
        StopAllCoroutines();
        StartCoroutine(FadeColorFromTo(GetComponent<Image>().color, defaultColor));
    }

    public void PlayLevel()
    {
        DOTween.Clear();
        PlayerPrefs.SetString("CurrentLevelName", levelName);
        SceneManager.LoadScene("PlayScene");
    }

    public void EditLevel()
    {
        DOTween.Clear();
        PlayerPrefs.SetString("CurrentLevelName", levelName);
        SceneManager.LoadScene("EditScene");
    }

    public void DeleteLevel()
    {
        SaveLoadInfo.DeleteLevelCompletely(levelName);
        manager.RefreshLevelsList();
    }

    public void SetLevelDisplayData(string name,
                                                        float bpm,
                                                        bool isPlayable,
                                                        float estim,
                                                        int notestotal,
                                                        int notesregistered,
                                                        System.Collections.Generic.List<int> notereg,
                                                        string bgPath,
                                                        string track)
    {
        levelName = name;
        levelBPM = bpm;
        estimation = estim;
        notesTotal = notestotal;
        notesRegistered = notesregistered;
        pressRecord = notereg;
        levelBackgroundPath = bgPath;
        trackPath = track;
        levelNameDisplay.SetText(levelName);
        levelBPMDisplay.SetText(levelBPM.ToString());

        if (notereg == null || notereg.Count < 1)
        {
            isPlayedBefore = false;
        }
        else
        {
            isPlayedBefore = true;
        }

        if (isPlayable)
        {
            playButton.interactable = true;
        }
        else
        {
            playButton.interactable = false;
        }
    }
    private IEnumerator FadeColorFromTo(Color fadeFrom, Color fadeTo)
    {

        float time = 0f;
        while (time < 0.5f)
        {
            GetComponent<Image>().color = Color.Lerp(fadeFrom, fadeTo, time / 0.5f);
            time += Time.deltaTime;
            yield return null;
        }
    }
}

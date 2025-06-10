using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AnotherFileBrowser.Windows;

public class EditorUIController : MonoBehaviour
{
    private const float NOTEANIMDURATION = 1.5f;
    private string userSelectedNoteType = null;
    private MusicController musicController;
    [SerializeField] private GameObject levelBorders;
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject mesh;
    [SerializeField] private GameObject addMenu;
    [SerializeField] private TMP_InputField levelNameEditor;
    [SerializeField] private TMP_Text notesAmount;
    //Спрайты курсора
    [SerializeField] private Texture2D newNoteCursor;
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Image levelBackground;

    [SerializeField] private Slider trackSlider;
    [SerializeField] private Image sliderBackground;
    //Блок элементов интерфейса управления данными о ноте
    [SerializeField] private GameObject noteInfoPanel;
    [SerializeField] private Slider fadeTimeSlider;
    [SerializeField] private Button deleteNoteButton;
    [SerializeField] private TextMeshProUGUI fadeTimeText;


    public void InitiateUI(string waveformPath, string backgroundPath, string levelName, int notesTotalAmount)
    {
        levelNameText.text = levelName;
        levelNameEditor.text = levelName;
        musicController = FindFirstObjectByType<MusicController>();
        SetNotesAmount(notesTotalAmount);
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);

        sliderBackground.sprite = SaveLoadInfo.LoadImageFromPath(waveformPath);
        if (backgroundPath != null)
        {
            SetUpBackground(backgroundPath);
        }

        trackSlider.maxValue = musicController.GetSafeTrackLength();
        trackSlider.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<float>((newTrackPosition) =>
        {
            noteInfoPanel.SetActive(false);
            musicController.UpdateMusicInfoOnTimingChange(newTrackPosition);
        }));
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && userSelectedNoteType != null)
        {
            if (!mesh.activeSelf)
            {
                FindFirstObjectByType<EditModeController>().CreateNewNoteOnCursor(userSelectedNoteType, false);
                Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
                userSelectedNoteType = null;
            }
        }
        else if (Input.GetMouseButtonDown(1) && userSelectedNoteType != null)
        {
            userSelectedNoteType = null;
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }
        if (Input.GetMouseButtonDown(1) && noteInfoPanel.activeSelf)
        {
            noteInfoPanel.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            ChangeCurrentNote("BASIC");
        } 
    }

    public void ChangeCurrentNote(string noteName)
    {
        switch (noteName)
        {
            case "BASIC":
                userSelectedNoteType = "BASIC";
                break;
                // Добавить вариации курсора для каждого вида нот
                // При добавлении новых нот обновить метод
        }
        //Cursor.SetCursor(newNoteCursor, Vector2.zero, CursorMode.Auto);
    }

    public void UserNoteDataControl(int index, float fadeTime, GameObject note)
    {
        deleteNoteButton.onClick.RemoveAllListeners();
        fadeTimeSlider.onValueChanged.RemoveAllListeners();
        if (!noteInfoPanel.activeSelf)
        {
            noteInfoPanel.SetActive(true);
        }
        deleteNoteButton.onClick.AddListener(() =>
        {
            noteInfoPanel.SetActive(false);
            Destroy(note);
            musicController.DeleteNoteDataByIndex(index);
        });
        fadeTimeSlider.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<float>((newFadeTime) =>
        {
            fadeTimeText.SetText(newFadeTime.ToString("0.00"));

            note.GetComponent<BasicNoteScript>().SetFadeTimeForPlayMode(NOTEANIMDURATION / newFadeTime);
            musicController.UpdateNoteInfoOnFadeChange(newFadeTime, index);
        }));
        fadeTimeSlider.value = fadeTime;
    }

    public void SetSliderInactive()
    {
        trackSlider.interactable = false;
    }

    public void SetSliderActive()
    {
        trackSlider.interactable = true;
    }

    public void RefreshSliderPositionInPlayMode(float timing)
    {
        trackSlider.SetValueWithoutNotify(timing);
    }

    public void BackToMainScreen()
    {
        musicController.SaveInfoFromLevel();
        SceneManager.LoadScene("MenuScene");
    }

    public void SelectLevelBackground()
    {
        BrowserProperties bp = new BrowserProperties();
        bp.filter = "Image |*.png";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, result =>
        {
            string newBGPath = SaveLoadInfo.SaveNewLevelBackground(musicController.GetCurrentLevelName(), result);
            musicController.UpdateBackgroundInfo(newBGPath);
            SetUpBackground(newBGPath);
        });
    }

    private void SetUpBackground(string path)
    {
        levelBackground.sprite = SaveLoadInfo.LoadImageFromPath(path);
        levelBackground.color = new Color(140.0f, 140.0f, 140.0f);
    }

    public void EnableDisableBorders(bool state)
    {
        levelBorders.SetActive(state);
    }

    public void EnableDisableMesh(bool state)
    {
        mesh.SetActive(state);
    }

    public void EnableDisableAddMenu()
    {
        addMenu.SetActive(!addMenu.activeSelf);
    }

    public void MeshTriggered(Vector2 meshPos)
    {
        if (userSelectedNoteType != null)
        {
            FindFirstObjectByType<EditModeController>().CreateNewNoteOnCursor(userSelectedNoteType, true, meshPos);
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            userSelectedNoteType = null;
        }
    }

    public void LevelNameUpdated(string name)
    {
        musicController.UpdateLevelName(name);
    }

    public void SetNotesAmount(int amount)
    {
        notesAmount.text = amount.ToString();
    }

    public void OpenClosePauseMenu(bool open)
    {
        if (open)
        {
            musicController.PausePlayModeOnUIInteraction();
            pauseMenu.SetActive(true);
            //анимация
        }
        else
        {
            //анимация
            pauseMenu.SetActive(false);
        }
    }
}
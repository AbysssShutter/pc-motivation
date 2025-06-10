using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;

public class PlayUIController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private TMP_Text levelNameShow;
    [SerializeField] private Image levelBackground;

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image loadingCircle;

    [SerializeField] private GameObject countdownScreen;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Image countdownBack;
    private int countdown = 3;

    private PlayMusicController playMusicController;

    public void InitiateUI(string backgroundPath, string levelName)
    {
        playMusicController = FindFirstObjectByType<PlayMusicController>();
        levelNameShow.text = levelName;
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);

        if (backgroundPath != null)
        {
            SetUpBackground(backgroundPath);
        }
        StartCoroutine(LoadingScreen("end"));
    }

    public void BackToMainScreen()
    {
        playMusicController.SaveCompletionInfo();
        SceneManager.LoadScene("MenuScene");
    }

    private void SetUpBackground(string path)
    {
        levelBackground.sprite = SaveLoadInfo.LoadImageFromPath(path);
        levelBackground.color = new Color(140.0f, 140.0f, 140.0f);
    }

    public IEnumerator LoadingScreen(string status)
    {
        if (status == "begin")
        {
            loadingScreen.SetActive(true);
            loadingCircle.transform.DORotate(new Vector3(360, 0, 0), 1f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);
        }
        else
        {
            loadingScreen.SetActive(false);
            loadingScreen.transform.DOKill();
        }
        yield return null;
    }

    public IEnumerator CountdownToGameUnpause()
    {
        countdownScreen.SetActive(true);
        countdownText.text = countdown.ToString();
        countdownText.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            countdownText.DOFade(0f, 0.3f).SetUpdate(true).SetDelay(0.3f);
        });

        yield return new WaitForSecondsRealtime(1f);
        countdown--;
        if (countdown > 1)
        {
            StartCoroutine(CountdownToGameUnpause());
        }
        else if (countdown == 1)
        {
            StartCoroutine(CountdownToGameUnpause());
        }
        else if (countdown == 0)
        {
            countdownBack.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                countdownScreen.SetActive(false);
                countdown = 3;
            });
            playMusicController.StartOrPausePlayMode();
        }
    }

    public void OpenClosePauseMenu(bool open)
    {
        if (open)
        {
            playMusicController.PausePlayModeOnUIInteraction();
            pauseMenu.SetActive(true);
            //анимация
        }
        else
        {
            //анимация
            pauseMenu.SetActive(false);
            StartCoroutine(CountdownToGameUnpause());
        }
    }
}
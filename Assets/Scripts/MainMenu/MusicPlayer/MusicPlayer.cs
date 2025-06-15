using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;
using DG.Tweening;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private TMP_Text CurrentTrackName;
    private System.Collections.Generic.List<string> trackList;
    private System.Collections.Generic.List<string> trackNames;
    private int currTrackIndex;
    private float generalVolume;
    private bool isOpened = false;
    public AudioImporter importer;
    [SerializeField] private GameObject trackPreviewTemplate;
    [SerializeField] private GameObject dataContainer;
    [SerializeField] private GameObject playlist;
    [SerializeField] private GameObject tracksView;
    [SerializeField] private Button extendPlaylistButton;
    [SerializeField] private AudioMixer masterMixer;
    private Tween textAnim;
    private Tween playlistAnim;
    [SerializeField] private GameObject cutscene;
    [SerializeField] private GameObject cutsceneElems;
    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("NotFirstLoad");
        PlayerPrefs.DeleteKey("CurrentLevelName");
    }
    void Start()
    {
        SetMusicVolume(0.2f);
        if (PlayerPrefs.HasKey("NotFirstLoad"))
        {
            SetFirstTrackOfListAndReset();
            cutscene.SetActive(false);
            cutsceneElems.SetActive(false);
        }
        else
        {
            PlayerPrefs.SetString("NotFirstLoad", "true");
        }
        RefreshTrackList();
    }

    private void OnLoaded(AudioClip audioClip)
    {
        musicPlayer.clip = audioClip;
        StartCoroutine(AudioCrossfadeIn());
        musicPlayer.Play();
    }
    public void SetFirstTrackOfListAndReset()
    {
        RefreshTrackList();
        currTrackIndex = 0;
        if (trackList != null && trackList.Count > 0)
        {
            SetNewTrack(trackList[currTrackIndex], trackNames[currTrackIndex], true);
        } 
    }
    public void SetNewTrack(string trackPath, string trackName, bool skipFadeOut = false)
    {
        if (skipFadeOut)
        {
            importer.Loaded += OnLoaded;
            importer.Import(trackPath);
        }
        else
        {
            StopAllCoroutines();
            masterMixer.SetFloat("MusicPlayerVolume", -80f);
            StartCoroutine(AudioCrossfadeOut(trackPath));
        }
        currTrackIndex = trackList.FindIndex(x => x == trackPath);

        DOTween.Kill(textAnim);
        string text = "";
        textAnim = DOTween.To(() => text, x => text = x, trackName, trackName.Length / 15f).OnUpdate(() =>
        {
            CurrentTrackName.SetText(text);
        });
    }
    public void PlayPauseTrack()
    {
        StopAllCoroutines();
        if (musicPlayer.isPlaying)
        {
            StartCoroutine(AudioCrossfadeOut("", true));
        }
        else
        {
            StartCoroutine(AudioCrossfadeIn(true));
        }
    }
    public void SetNextTrack()
    {
        if (trackList.Count <= currTrackIndex + 1)
        {
            currTrackIndex = 0;
        }
        else
        {
            currTrackIndex++;
        }
        SetNewTrack(trackList[currTrackIndex], trackNames[currTrackIndex]);   
    }
    public void SetPrevTrack()
    {
        if (currTrackIndex - 1 < 0)
        {
            currTrackIndex = trackList.Count - 1;
        }
        else
        {
            currTrackIndex--;
        }
        SetNewTrack(trackList[currTrackIndex], trackNames[currTrackIndex]);
    }
    public void RefreshTrackList()
    {
        playlist.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 90);
        tracksView.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 0);
        SaveLoadInfo.GetTrackList(out trackList, out trackNames);
        if (trackList != null)
        {
            int tracksCount = 0;
            foreach (Transform child in dataContainer.transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < trackList.Count; i++)
            {
                if (tracksCount < 4)
                {
                    playlist.GetComponent<RectTransform>().sizeDelta = new Vector2(400, playlist.GetComponent<RectTransform>().sizeDelta[1] + 60);
                    tracksView.GetComponent<RectTransform>().sizeDelta = new Vector2(400, tracksView.GetComponent<RectTransform>().sizeDelta[1] + 60);
                    tracksCount++;
                }
                GameObject trackPreview = Instantiate(trackPreviewTemplate);
                trackPreview.GetComponent<TrackScript>().SetTrackName(trackNames[i]);
                trackPreview.GetComponent<TrackScript>().SetTrackPath(trackList[i]);
                trackPreview.transform.SetParent(dataContainer.transform, false);
            }
            extendPlaylistButton.interactable = true;
        }
        else
        {
            extendPlaylistButton.interactable = false;
            CurrentTrackName.text = "No Tracks...";
        }
    }
    public void OpenClosePlaylist()
    {
        DOTween.Kill(playlistAnim);
        if (!isOpened)
        {
            playlistAnim = playlist.GetComponent<RectTransform>().DOAnchorPosY(-35, 1f);
        }
        else
        {
            playlistAnim = playlist.GetComponent<RectTransform>().DOAnchorPosY(300, 1f);
        }
        isOpened = !isOpened;
    }

    public void ClosePlaylist()
    {
        DOTween.Kill(playlistAnim);
        playlistAnim = playlist.GetComponent<RectTransform>().DOAnchorPosY(300, 1f);
        isOpened = false;
    }
    public void SetMusicVolume(float volume)
    {
        generalVolume = Mathf.Log10(volume) * 20;
        masterMixer.SetFloat("MusicPlayerVolume", generalVolume);
    }

    private IEnumerator AudioCrossfadeIn(bool intoResume = false)
    {
        if (intoResume)
        {
            musicPlayer.UnPause();
        }
        float time = 0f;
        float fadeTime = 1f;
        while (time < 1)
        {
            masterMixer.SetFloat("MusicPlayerVolume", Mathf.Lerp(-80f, generalVolume, time / fadeTime));
            time += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AudioCrossfadeOut(string trackPath, bool intoPause = false)
    {
        float time = 0f;
        float fadeTime = 0.5f;
        float test = 0f;
        masterMixer.GetFloat("MusicPlayerVolume", out test);
        if (test != -80f)
        {
            while (time < 1)
            {
                masterMixer.SetFloat("MusicPlayerVolume", Mathf.Lerp(generalVolume, -80f, time / fadeTime));
                time += Time.deltaTime;
                yield return null;
            }
        }
        if (intoPause)
        {
            musicPlayer.Pause();
        }
        else
        {
            importer.Loaded += OnLoaded;
            importer.Import(trackPath);
        }
    }
}

using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private Animator playlistAnimator;
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private TextMeshProUGUI CurrentTrackName;
    private List<string> trackList;
    private List<string> trackNames;
    private int currTrackIndex;
    public AudioImporter importer;
    [SerializeField] private GameObject trackPreviewTemplate;
    [SerializeField] private GameObject dataContainer;
    [SerializeField] private GameObject playlist;
    [SerializeField] private GameObject tracksView;
    [SerializeField] private Button extendPlaylistButton;
    [SerializeField] private AudioMixer masterMixer;
    void Start()
    {
        /*
        Перенести блок кода в обобщенный скрипт главного меню когда он будет
        */
        PlayerPrefs.DeleteKey("CurrentLevelName");
        //Конец блока
        
        masterMixer.SetFloat("MusicPlayerVolume", Mathf.Log10(0.33f) * 20);
        RefreshTrackList();
    }
    private void OnLoaded(AudioClip audioClip)
    {
        musicPlayer.clip = audioClip;
        musicPlayer.Play();
    }
    public void SetNewTrack(string trackPath, string trackName)
    {
        importer.Loaded += OnLoaded;
        importer.Import(trackPath);

        currTrackIndex = trackList.FindIndex(x => x == trackPath);
        CurrentTrackName.text = trackName;
    }
    public void PlayPauseTrack()
    {
        if (musicPlayer.isPlaying)
        {
            musicPlayer.Pause();
        }
        else
        {
            musicPlayer.Play();
        }
    }
    public void SetNextTrack()
    {
        currTrackIndex++;
        SetNewTrack(trackList[currTrackIndex], trackNames[currTrackIndex]);
    }
    public void SetPrevTrack()
    {
        currTrackIndex--;
        SetNewTrack(trackList[currTrackIndex], trackNames[currTrackIndex]);
    }
    public void RefreshTrackList()
    {
        //Добавить вызов метода при добавлении и удалении уровней
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
        playlistAnimator.SetBool("idle", !playlistAnimator.GetBool("idle"));
    }
    public void SetMusicVolume(float volume)
    {
        masterMixer.SetFloat("MusicPlayerVolume", Mathf.Log10(volume) * 20);
    }
}

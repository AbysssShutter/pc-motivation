using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusicController : MonoBehaviour
{
    //Положение самого первого бита в песне в секундах (задается пользователем)
    public float firstBeatOffset;

    //Количество секунд в каждом ударе 
    private float secPerBeat;
    private float beatPerSec;

    //Текущее положение с начала трека  в секундах
    private float songPosition = 0f;

    //Текущее положение с начала трека в битах
    private float songPositionInBeats = 0f;

    //Текущее время аудио системы в секундах
    private float dspSongTime;

    //AudioSource объекта проигрывающего музыку, на котором висит этот скрипт
    //Для кого я писал все эти комментарии?
    [SerializeField] private AudioSource musicPlayer;
    LevelInfo levelInfo = new LevelInfo();
    NotesInfo notesInfo = new NotesInfo();
    private int nextIndex = 0;
    private bool playMode = false;
    /* Переменная, сохраняющая результаты взаимодействия 
    пользователя с нотой в игровом режиме. 
    Первые три индекса соответствуют определенной анимации ноты:
    0 - NoteAppears => Слишком рано
    1 - NoteTrueIdle => 100% попадание
    2 - NoteCrack => Слишком поздно
    Последний индекс засчитывается в случаях, когда с нотой не было 
    произведено никаких взаимодействий
    3 - NoteLost => "Промах"
    */
    private List<int> pressRecord = new List<int>() { 0, 0, 0, 0 };
    private int notesRegistered;
    private PlayModeController playModeController;
    private PlayUIController playUIController;
    public AudioImporter importer;
    private void OnLoaded(AudioClip audioClip)
    {
        musicPlayer.clip = audioClip;
        // Костыль, убрать по возможности
        musicPlayer.Play();
        musicPlayer.Pause();

        secPerBeat = 60f / levelInfo.levelBpm;
        beatPerSec = levelInfo.levelBpm / 60f;

        playUIController.InitiateUI(levelInfo.backgroundPath, levelInfo.levelName);
        StartCoroutine(playUIController.CountdownToGameUnpause());
    }

    void Start()
    {
        playModeController = FindFirstObjectByType<PlayModeController>();
        playUIController = FindFirstObjectByType<PlayUIController>();
        StartCoroutine(playUIController.LoadingScreen("begin"));
        LoadInfoToLevel();
    }

    void Update()
    {
        if (playMode)
        {
            //determine how many seconds since the song started
            songPosition = (float)(AudioSettings.dspTime - dspSongTime - firstBeatOffset);
            //determine how many beats since the song started
            songPositionInBeats = songPosition / secPerBeat;
            if (nextIndex <= notesInfo.noteBeat.Count - 1 && (notesInfo.noteBeat[nextIndex] - (notesInfo.noteFadeTime[nextIndex] * beatPerSec)) < songPositionInBeats)
            {
                playModeController.SpawnNoteInPlayMode(notesInfo.notePos[nextIndex].GetVector(), notesInfo.noteType[nextIndex], notesInfo.noteFadeTime[nextIndex]);
                nextIndex++;
            }
        }
    }

    public void StartOrPausePlayMode()
    {
        playMode = !playMode;
        if (playMode)
        {
            musicPlayer.UnPause();

            nextIndex = 0;
            GameObject[] notesOnScreen = GameObject.FindGameObjectsWithTag("Note");
            foreach (GameObject note in notesOnScreen)
            {
                note.GetComponent<BasicNoteScript>().ResumeNoteForEditMode();
                nextIndex++;
            }

            if (songPosition == 0f)
            {
                dspSongTime = (float)AudioSettings.dspTime;
            }
            else if (songPosition != 0f)
            {
                if (notesInfo.noteBeat.FindIndex(note => note > songPositionInBeats) != -1)
                {
                    nextIndex += notesInfo.noteBeat.FindIndex(note => note > songPositionInBeats);
                }
                else
                {
                    nextIndex = notesInfo.noteBeat.Count;
                }
            }
        }
        else
        {
            musicPlayer.Pause();
            GameObject[] notesOnScreen = GameObject.FindGameObjectsWithTag("Note");
            foreach (GameObject note in notesOnScreen)
            {
                note.GetComponent<BasicNoteScript>().PauseNoteForEditMode();
            }
        }
    }

    private void LoadInfoToLevel()
    {
        levelInfo = SaveLoadInfo.LoadLevelInfo(PlayerPrefs.GetString("CurrentLevelName"));
        notesInfo = SaveLoadInfo.LoadNotesInfo(PlayerPrefs.GetString("CurrentLevelName"));
        importer.Loaded += OnLoaded;
        importer.Import(levelInfo.trackPath);
        PlayerPrefs.DeleteKey("CurrentLevelName");
    }

    public string GetCurrentLevelName()
    {
        return levelInfo.levelName;
    }

    public void PausePlayModeOnUIInteraction()
    {
        if (playMode)
        {
            StartOrPausePlayMode();
        }
    }

    public void SaveCompletionInfo()
    {
        levelInfo.CalculateAndSaveScore(notesRegistered, pressRecord);
        SaveLoadInfo.UpdateLevelInfo(levelInfo);
    }

    public void RecordNotePress(string clipName)
    {
        switch (clipName)
        {
            case "NoteAppears":
                pressRecord[0] += 1;
                break;
            case "NoteTrueIdle":
                pressRecord[1] += 1;
                break;
            case "NoteCrack":
                pressRecord[2] += 1;
                break;
            case "NoteLost":
                pressRecord[3] += 1;
                break;
        }
        notesRegistered += 1;
    }
}


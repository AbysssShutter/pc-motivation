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
    private float step = 0f;
    
    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("NotFirstLoad");
        PlayerPrefs.DeleteKey("CurrentLevelName");
    }
    private void OnLoaded(AudioClip audioClip)
    {
        musicPlayer.clip = audioClip;
        // Костыль, убрать по возможности
        musicPlayer.Play();
        musicPlayer.Pause();

        secPerBeat = 60f / levelInfo.levelBpm;
        beatPerSec = levelInfo.levelBpm / 60f;

        playUIController.InitiateUI(levelInfo.backgroundPath, levelInfo.levelName);
        playModeController.SetLevelBPM(levelInfo.levelBpm);
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
            if (nextIndex <= notesInfo.noteBeat.Count - 1 && notesInfo.noteBeat[nextIndex] - (notesInfo.noteFadeTime[nextIndex] * beatPerSec) < songPositionInBeats)
            {
                playModeController.SpawnNoteInPlayMode(notesInfo.notePos[nextIndex].GetVector(), notesInfo.noteType[nextIndex], notesInfo.noteFadeTime[nextIndex]);
                if (nextIndex + 1 != notesInfo.noteBeat.Count)
                {
                    step = (notesInfo.noteBeat[nextIndex + 1] + (notesInfo.noteFadeTime[nextIndex + 1] - 0.5f) * beatPerSec
                            - notesInfo.noteBeat[nextIndex]
                            ) * secPerBeat;

                    if (step < 7 && step > 0)
                    {
                        playModeController.DrawLineFromOneNoteToOther(notesInfo.notePos[nextIndex], notesInfo.notePos[nextIndex + 1], step);
                    }
                }
                nextIndex++;
            }
            else if (nextIndex >= notesInfo.noteBeat.Count && musicPlayer.clip.length - songPosition > 10f)
            {
                playUIController.UnlockSkipButton();
            }
            else if (musicPlayer.clip.length - songPosition <= 1f)
            {
                playUIController.BackToMainScreen();
            }
        }
        playUIController.RefreshSliderPositionInPlayMode(songPosition);
    }

    public void StartOrPausePlayMode()
    {
        playMode = !playMode;
        if (playMode)
        {
            musicPlayer.UnPause();

            GameObject[] notesOnScreen = GameObject.FindGameObjectsWithTag("Note");
            foreach (GameObject note in notesOnScreen)
            {
                note.GetComponent<BasicNoteScript>().ResumeNoteForEditMode();
            }
            GameObject[] linesOnScreen = GameObject.FindGameObjectsWithTag("NoteLine");
            foreach (GameObject line in linesOnScreen)
            {
                line.GetComponent<LineDrawer>().isPaused = false;
            }

            dspSongTime = (float)AudioSettings.dspTime - songPosition;
        }
        else
        {
            musicPlayer.Pause();
            GameObject[] notesOnScreen = GameObject.FindGameObjectsWithTag("Note");
            foreach (GameObject note in notesOnScreen)
            {
                note.GetComponent<BasicNoteScript>().PauseNoteForEditMode();
            }
            GameObject[] linesOnScreen = GameObject.FindGameObjectsWithTag("NoteLine");
            foreach (GameObject line in linesOnScreen)
            {
                line.GetComponent<LineDrawer>().isPaused = true;
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
            case "Got it!":
                pressRecord[0] += 1;
                break;
            case "Perfect!":
                pressRecord[1] += 1;
                break;
            case "Meh":
                pressRecord[2] += 1;
                break;
            case "Missed":
                pressRecord[3] += 1;
                break;
        }
        notesRegistered += 1;
    }

    public float GetSafeTrackLength()
    {
        return musicPlayer.clip.length - 0.1f;
    }
}


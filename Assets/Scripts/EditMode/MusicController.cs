using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
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
    private EditModeController editModeController;
    private EditorUIController editorUIController;
    public AudioImporter importer;

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

        editorUIController.InitiateUI(levelInfo.waveformPath, levelInfo.backgroundPath, levelInfo.levelName, levelInfo.notesTotal);
        secPerBeat = 60f / levelInfo.levelBpm;
        beatPerSec = levelInfo.levelBpm / 60f;
        editModeController.SetLevelBPM(levelInfo.levelBpm);
    }

    void Start()
    {
        LoadInfoToLevel();

        editModeController = FindFirstObjectByType<EditModeController>();
        editorUIController = FindFirstObjectByType<EditorUIController>();
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
                editModeController.SpawnNoteInPlayMode(notesInfo.notePos[nextIndex].GetVector(), notesInfo.noteType[nextIndex], notesInfo.noteFadeTime[nextIndex]);
                nextIndex++;
            }
            editorUIController.RefreshSliderPositionInPlayMode(songPosition);
        }
    }

    public void UpdateMusicInfoOnTimingChange(float timing)
    {
        dspSongTime = (float)AudioSettings.dspTime - timing;
        songPosition = timing;
        musicPlayer.time = timing;
        songPositionInBeats = songPosition / secPerBeat;

        List<float> beatsAhead = new();
        List<int> noteIndexes = new();
        List<float> remainingTime = new();
        foreach (float notePastCurrBeat in notesInfo.noteBeat.FindAll(sb => sb <= songPositionInBeats + (beatPerSec * 5f) && sb >= songPositionInBeats))
        {
            noteIndexes.Add(notesInfo.noteBeat.IndexOf(notePastCurrBeat));
            beatsAhead.Add((float)Math.Round((notePastCurrBeat - songPositionInBeats) / beatPerSec, 2));
        }
        editModeController.RefreshNotesOnTimingUpdate(beatsAhead, noteIndexes);
    }

    public void UpdateNoteInfoOnFadeChange(float newFadeTime, int index)
    {
        notesInfo.noteFadeTime[index] = newFadeTime;
    }

    public float GetSafeTrackLength()
    {
        return musicPlayer.clip.length - 0.1f;
    }

    public int AddNewNoteInfoInRightPlace(Vector2 currMousePos, string type)
    {
        int newIndex = notesInfo.noteBeat.FindIndex(note => note > songPositionInBeats);
        if (newIndex == -1)
        {
            notesInfo.noteBeat.Add(songPositionInBeats);
            notesInfo.noteFadeTime.Add(3f);
            notesInfo.notePos.Add(new SirVector(currMousePos));
            notesInfo.noteType.Add(type);
            editorUIController.SetNotesAmount(notesInfo.noteBeat.Count);
            return notesInfo.noteBeat.Count - 1;
        }
        else
        {
            notesInfo.noteBeat.Insert(newIndex, songPositionInBeats);
            notesInfo.noteFadeTime.Insert(newIndex, 3f);
            notesInfo.notePos.Insert(newIndex, new SirVector(currMousePos));
            notesInfo.noteType.Insert(newIndex, type);
            editorUIController.SetNotesAmount(notesInfo.noteBeat.Count);
            return newIndex;
        }
    }

    public void DeleteNoteDataByIndex(int index)
    {
        notesInfo.noteBeat.RemoveAt(index);
        notesInfo.noteFadeTime.RemoveAt(index);
        notesInfo.notePos.RemoveAt(index);
        notesInfo.noteType.RemoveAt(index);
    }

    public float GetNoteFadeTimeByIndex(int index)
    {
        return notesInfo.noteFadeTime[index];
    }

    public Vector2 GetNotePositionByIndex(int index)
    {
        return notesInfo.notePos[index].GetVector();
    }

    public string GetNoteTypeByIndex(int index)
    {
        return notesInfo.noteType[index];
    }

    public void StartOrPausePlayMode()
    {
        playMode = !playMode;
        if (playMode)
        {
            musicPlayer.UnPause();
            editorUIController.SetSliderInactive();

            nextIndex = 0;
            GameObject[] notesOnScreen = GameObject.FindGameObjectsWithTag("Note");
            foreach (GameObject note in notesOnScreen)
            {
                note.GetComponent<BasicNoteScript>().ResumeNoteForEditMode();
                note.GetComponent<BasicNoteScript>().SetEditModeTo(false);
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
                dspSongTime = (float)AudioSettings.dspTime - songPosition;
            }
        }
        else
        {
            musicPlayer.Pause();
            GameObject[] notesOnScreen = GameObject.FindGameObjectsWithTag("Note");
            foreach (GameObject note in notesOnScreen)
            {
                note.GetComponent<BasicNoteScript>().PauseNoteForEditMode();
                note.GetComponent<BasicNoteScript>().SetEditModeTo(true);
            }
            editorUIController.SetSliderActive();
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

    public void SaveInfoFromLevel()
    {
        if (notesInfo.noteBeat.Count > 10)
        {
            levelInfo.isPlayable = true;
        }
        else
        {
            levelInfo.isPlayable = false;
        }
        levelInfo.notesTotal = notesInfo.noteBeat.Count;
        SaveLoadInfo.UpdateLevelData(notesInfo, levelInfo);
    }

    public string GetCurrentLevelName()
    {
        return levelInfo.levelName;
    }

    public void UpdateBackgroundInfo(string path)
    {
        levelInfo.backgroundPath = path;
    }

    public void PausePlayModeOnUIInteraction()
    {
        if (playMode)
        {
            StartOrPausePlayMode();
        }
    }

    public void UpdateLevelName(string newName)
    {
        levelInfo.levelName = newName;
    }
}


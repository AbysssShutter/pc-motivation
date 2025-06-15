using System.Collections.Generic;
using UnityEngine;

public class EditModeController : MonoBehaviour
{
    private const float NOTEANIMDURATION = 1.5f;
    private float BPM;
    [SerializeField] private GameObject elemsContainer;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private GameObject buttonSpawnField;
    // Префабы нот
    [SerializeField] private GameObject basicNotePrefab;
    [SerializeField] private GameObject triolNotePrefab;
    [SerializeField] private GameObject duolNotePrefab;
    MusicController musicController;
    

    void Start()
    {
        musicController = FindFirstObjectByType<MusicController>();
    }
    public void SetLevelBPM(float bpm)
    {
        BPM = bpm;
    }

    public void CreateNewNoteOnCursor(string type, bool meshMouseSetup, Vector2 currMousePos = default(Vector2))
    {
        if (!meshMouseSetup)
        {
            currMousePos = Input.mousePosition;
            RectTransform canvasTransform = elemsContainer.transform.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, currMousePos, worldCamera, out currMousePos);

            if (currMousePos[0] < -840 || currMousePos[0] > 840
                || currMousePos[1] < -420 || currMousePos[1] > 420)
            {
                return;
            }
        }
        SpawnNoteInEditMode(
            true,
            currMousePos,
            type,
            musicController.AddNewNoteInfoInRightPlace(currMousePos, type));
    }

    private void SpawnNoteInEditMode(bool isANewNote, Vector2 spawnPos, string type, int index = 0, float offsetPercent = 0.0f, float fadeTime = 3f)
    {
        GameObject newNote = Instantiate(GetObjectByType(type), spawnPos, Quaternion.identity);
        newNote.GetComponent<BasicNoteScript>().SetBPM(BPM);
        newNote.GetComponent<BasicNoteScript>().SetEditModeTo(true);
        newNote.GetComponent<BasicNoteScript>().SetNoteIndex(index);
        newNote.GetComponent<BasicNoteScript>().SetFadeTimeForPlayMode(NOTEANIMDURATION / fadeTime);
        newNote.GetComponent<BasicNoteScript>().RotateSelf();
        if (offsetPercent == 0.0f && !isANewNote)
        {
            Destroy(newNote);
        }
        if (isANewNote)
        {
            newNote.GetComponent<BasicNoteScript>().NotePreviewMode();
        }
        else if (offsetPercent > 0f)
        {
            newNote.GetComponent<BasicNoteScript>().SetAnimOffsetForEditMode(offsetPercent);
            newNote.GetComponent<BasicNoteScript>().PauseNoteForEditMode();
        }
        newNote.transform.SetParent(buttonSpawnField.transform, false);
    }

    public void SpawnNoteInPlayMode(Vector2 spawnPos, string type, float fadeTime) {
        GameObject newNote = Instantiate(GetObjectByType(type), spawnPos, Quaternion.identity);
        newNote.GetComponent<BasicNoteScript>().SetFadeTimeForPlayMode(NOTEANIMDURATION / fadeTime);
        newNote.GetComponent<BasicNoteScript>().SetBPM(BPM);
        newNote.GetComponent<BasicNoteScript>().RotateSelf();
        newNote.transform.SetParent(elemsContainer.transform, false);   
    }

    public void RefreshNotesOnTimingUpdate(List<float> beatsAhead = null, List<int> indexes = null) {
        GameObject[] prevNotes = GameObject.FindGameObjectsWithTag("Note");
        foreach(GameObject note in prevNotes) {
            Destroy(note);
        }
        if (indexes != null) {
            foreach(int index in indexes) {
                float currNoteFadeTime = musicController.GetNoteFadeTimeByIndex(index);
                Vector2 currNotePos = musicController.GetNotePositionByIndex(index);
                string currNoteType = musicController.GetNoteTypeByIndex(index);

                float percent = (currNoteFadeTime - beatsAhead[0]) / currNoteFadeTime;
                if (percent > 0) {
                    SpawnNoteInEditMode(false, currNotePos, currNoteType, index, percent, currNoteFadeTime);
                }
                beatsAhead.RemoveAt(0);
            }
        }
    }

    private GameObject GetObjectByType(string type) {
        switch (type)
        {
            case "BASIC":
                return basicNotePrefab;
            case "TRIOL":
                return triolNotePrefab;
            case "DUOL":
                return duolNotePrefab;
        }
        return null;
    }
}
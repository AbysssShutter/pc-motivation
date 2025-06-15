using UnityEngine;

public class PlayModeController : MonoBehaviour
{
    private const float NOTEANIMDURATION = 1.5f;
    private float BPM;
    [SerializeField] private GameObject elemsContainer;
    [SerializeField] private GameObject lineDraw;
    [SerializeField] private GameObject basicNotePrefab;
    [SerializeField] private GameObject triolNotePrefab;
    [SerializeField] private GameObject duolNotePrefab;

    public void SetLevelBPM(float bpm)
    {
        BPM = bpm;
    }

    public void SpawnNoteInPlayMode(Vector2 spawnPos, string type, float fadeTime)
    {
        GameObject newNote = Instantiate(GetObjectByType(type), spawnPos, Quaternion.identity);
        newNote.GetComponent<BasicNoteScript>().SetFadeTimeForPlayMode(NOTEANIMDURATION / fadeTime);
        newNote.GetComponent<BasicNoteScript>().SetBPM(BPM);
        newNote.GetComponent<BasicNoteScript>().RotateSelf();
        newNote.transform.SetParent(elemsContainer.transform, false);
    }

    public void DrawLineFromOneNoteToOther(SirVector start, SirVector end, float duration)
    {
        GameObject line = Instantiate(lineDraw, new Vector2(0, 0), Quaternion.identity);
        line.transform.SetParent(elemsContainer.transform, false);
        StartCoroutine(line.GetComponent<LineDrawer>().DrawFromTo(start, end, duration));
    }

    private GameObject GetObjectByType(string type)
    {
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
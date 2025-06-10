using UnityEngine;

public class PlayModeController : MonoBehaviour
{
    private const float NOTEANIMDURATION = 1.5f;
    [SerializeField] private Canvas levelCanvas;
    [SerializeField] private GameObject basicNotePrefab;
    PlayMusicController playMusicController;
    

    void Start()
    {
        playMusicController = FindFirstObjectByType<PlayMusicController>();
    }

    public void SpawnNoteInPlayMode(Vector2 spawnPos, string type, float fadeTime) {
        GameObject newNote = Instantiate(GetObjectByType(type), spawnPos, Quaternion.identity);
        newNote.GetComponent<BasicNoteScript>().SetFadeTimeForPlayMode(NOTEANIMDURATION / fadeTime);
        newNote.GetComponent<BasicNoteScript>().RotateSelf();
        newNote.transform.SetParent(levelCanvas.transform, false);   
    }

    private GameObject GetObjectByType(string type) {
        switch(type) {
            case "BASIC":
                return basicNotePrefab;
        }
        return null;
    }
}
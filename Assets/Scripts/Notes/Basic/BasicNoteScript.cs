using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicNoteScript : MonoBehaviour
{
    [SerializeField] Animator noteAnimator;
    [SerializeField] AudioSource clickSound;
    private float fadeTime = 0f;
    private bool editMode = false;
    private int noteIndex;
    private bool interacted;
    private string sceneName;

    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }

    public void NoteHit()
    {
        if (!editMode)
        {
            if (!interacted)
            {
                interacted = true;
                // Реакция ноты на нажатие в игровом режиме
                noteAnimator.Play("NotePressed");
                clickSound.Play();
                if (sceneName == "PlayScene")
                {
                    foreach (var clipInfo in noteAnimator.GetCurrentAnimatorClipInfo(0))
                    {
                        FindAnyObjectByType<PlayMusicController>().RecordNotePress(clipInfo.clip.name);
                    }
                }
            }
        }
        else
        {
            if (sceneName == "EditScene")
            {
                FindAnyObjectByType<EditorUIController>().UserNoteDataControl(noteIndex, fadeTime, gameObject);
            }
        }
    }

    public void SetFadeTimeForPlayMode(float newFadeMulitpier)
    {
        fadeTime = newFadeMulitpier;
        noteAnimator.SetFloat("animSpeed", newFadeMulitpier);
    }

    public void SetAnimOffsetForEditMode(float timePercent)
    {
        noteAnimator.Play("NoteAppears", 0, timePercent);
    }

    public void PauseNoteForEditMode()
    {
        noteAnimator.speed = 0.0f;
    }

    public void ResumeNoteForEditMode()
    {
        noteAnimator.SetBool("preview", false);
        noteAnimator.enabled = true;
        noteAnimator.speed = 1f;
    }

    public void NotePreviewMode()
    {
        noteAnimator.SetBool("preview", true);
    }

    public void SetNoteIndex(int index)
    {
        noteIndex = index;
    }

    public void SetEditModeTo(bool state)
    {
        editMode = state;
    }

    public void DestroyNote()
    {
        if (sceneName == "PlayScene")
        {
            FindAnyObjectByType<PlayMusicController>().RecordNotePress("NoteLost");
        }
        Destroy(gameObject);
    }

    public void NoteNotInteractable()
    {
        interacted = true;
    }

    public void RotateSelf()
    {
        transform.Rotate(0, 0, 45);
    }
}

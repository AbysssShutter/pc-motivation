using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicNoteScript : MonoBehaviour
{
    [SerializeField] private Animator noteAnimator;
    [SerializeField] private AudioSource[] clickSound;
    [SerializeField] private TMP_Text scoreText;
    private float fadeTime = 0f;
    private bool editMode = false;
    private int noteIndex;
    private float BPM;
    private bool interacted;
    public PlayMusicController playMusicController = null;
    public EditorUIController editorUIController = null;
    public string currentAnimation;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "PlayScene")
        {
            playMusicController = FindAnyObjectByType<PlayMusicController>();
        }
        else
        {
            editorUIController = FindAnyObjectByType<EditorUIController>();
        }
    }

    public virtual void NoteHit()
    {
        if (!editMode)
        {
            if (!interacted)
            {
                interacted = true;
                // Реакция ноты на нажатие в игровом режиме
                StartCoroutine(AnimateText());
                clickSound[0].Play();
                if (playMusicController)
                {
                    playMusicController.RecordNotePress(currentAnimation);
                }
                noteAnimator.Play("NotePressed");
            }
        }
        else
        {
            if (editorUIController)
            {
                editorUIController.UserNoteDataControl(noteIndex, fadeTime, gameObject);
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

    public void SetBPM(float bpm)
    {
        BPM = bpm;
    }

    public void SetEditModeTo(bool state)
    {
        editMode = state;
    }

    public void DestroyNote()
    {
        Destroy(gameObject);
    }

    public void DestroyLostNote()
    {
        if (playMusicController)
        {
            playMusicController.RecordNotePress("Missed");
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

    public void SetAnimationName(string name)
    {
        currentAnimation = name;
    }
    public IEnumerator AnimateText()
    {
        Color currentColor = new Color();
        switch (currentAnimation)
        {
            case "Got it!":
                currentColor = new Color(1, 1, 0, 1);
                break;
            case "Perfect!":
                currentColor = new Color(0, 1, 0, 1);
                break;
            case "Meh":
                currentColor = new Color(1, 0.5f, 0.25f, 1);
                break;
        }
        scoreText.text = currentAnimation;
        scoreText.transform.localPosition = RandomAroundNote();
        scoreText.DOColor(currentColor, 0.25f).OnComplete(() =>
        {
            currentColor.a = 0;
            scoreText.DOColor(currentColor, 0.25f);
        });
        yield return null;
    }

    private Vector2 RandomAroundNote()
    {
        float x = Random.Range(0f, 20f);
        float y = 0;
        // 18-20 => -10 - 10
        // 10-18 => 13 - 15
        // 0 - 10 => 19-20
        if (x > 18 && x < 18)
        {
            y = Random.Range(-10f, 10f);
        }
        else if (x > 10 && x < 18)
        {
            y = Random.Range(13f, 15f);
        }
        else if (x > 0 && x < 10)
        {
            y = Random.Range(19f, 20f);
        }

        x *= Random.value < 0.5f ? 1 : -1;
        y *= Random.value < 0.5f ? 1 : -1;
        return new Vector2(x, y);
    }

    public AudioSource[] GetAudioSources()
    {
        return clickSound;
    }

    public float GetBPM()
    {
        return BPM;
    }

    public Animator GetAnimator()
    {
        return noteAnimator;
    }

    public bool GetEditMode()
    {
        return editMode;
    }

    public bool GetInteracted()
    {
        return interacted;
    }

    public int GetNoteIndex()
    {
        return noteIndex;
    }

    public float GetFadeTime()
    {
        return fadeTime;
    }
}
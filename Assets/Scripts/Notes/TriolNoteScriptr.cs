using System.Collections;
using UnityEngine;

public class TriolNoteScript : BasicNoteScript
{
    public override void NoteHit()
    {
        if (!GetEditMode())
        {
            if (!GetInteracted())
            {
                NoteNotInteractable();
                // Реакция ноты на нажатие в игровом режиме
                StartCoroutine(AnimateText());
                StartCoroutine(PlayTriol());
                if (playMusicController)
                {
                    playMusicController.RecordNotePress(currentAnimation);
                }
                GetAnimator().Play("NotePressed");
            }
        }
        else
        {
            if (editorUIController)
            {
                editorUIController.UserNoteDataControl(GetNoteIndex(), GetFadeTime(), gameObject);
            }
        }
    }

    private IEnumerator PlayTriol()
    {
        float delay = (240 / GetBPM()) * 0.125f;
        foreach (AudioSource src in GetAudioSources())
        {
            src.Play();
            yield return new WaitForSeconds(delay);
        }
        DestroyNote();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private Slider generalSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;

    [SerializeField] private TMP_Text generalCount;
    [SerializeField] private TMP_Text musicCount;
    [SerializeField] private TMP_Text effectsCount;

    [SerializeField] private AudioMixer masterMixer;
void Start()
    {
        PlayerPrefs.DeleteKey("CurrentLevelName");
        
        generalSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        effectsSlider.onValueChanged.RemoveAllListeners();

        if (PlayerPrefs.HasKey("GeneralVolume"))
        {
            generalSlider.value = PlayerPrefs.GetFloat("GeneralVolume");
            generalCount.SetText(PlayerPrefs.GetFloat("GeneralVolume").ToString("0.00"));
            masterMixer.SetFloat("GeneralVolume", Mathf.Log10(PlayerPrefs.GetFloat("GeneralVolume")) * 20);
        }
        else
        {
            generalCount.SetText("1.00");
            generalSlider.value = 1;
        }
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
            musicCount.SetText(PlayerPrefs.GetFloat("MusicVolume").ToString("0.00"));
            masterMixer.SetFloat("MusicVolume", Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume")) * 20);
        }
        else
        {
            musicCount.SetText("1.00");
            musicSlider.value = 1;
        }
        if (PlayerPrefs.HasKey("EffectsVolume"))
        {
            effectsSlider.value = PlayerPrefs.GetFloat("EffectsVolume");
            effectsCount.SetText(PlayerPrefs.GetFloat("EffectsVolume").ToString("0.00"));
            masterMixer.SetFloat("EffectsVolume", Mathf.Log10(PlayerPrefs.GetFloat("EffectsVolume")) * 20);
        }
        else
        {
            effectsCount.SetText("1.00");
            effectsSlider.value = 1;
        }

        generalSlider.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<float>((value) =>
        {
            generalCount.SetText(value.ToString("0.00"));
            masterMixer.SetFloat("GeneralVolume", Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat("GeneralVolume", value);
        }));

        musicSlider.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<float>((value) =>
        {
            musicCount.SetText(value.ToString("0.00"));
            masterMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat("MusicVolume", value);
        }));

        effectsSlider.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<float>((value) =>
        {
            effectsCount.SetText(value.ToString("0.00"));
            masterMixer.SetFloat("EffectsVolume", Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat("EffectsVolume", value);
        }));
    }
}

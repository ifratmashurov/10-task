using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer mixer;
    public string param = "MusicVol";   // MusicVol или SFXVol
    Slider s;

    void Start()
    {
        s = GetComponent<Slider>();
        Refresh();
        s.onValueChanged.AddListener(OnChanged);
    }

    void OnChanged(float v)
    {
        Apply(v);
        if (param == "SFXVol") SaveSystem.SetSfxVolume(v);
        else SaveSystem.SetMusicVolume(v);
    }

    void Apply(float v)
    {
        mixer.SetFloat(param, Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f);
    }

    public void Refresh()   // вызывается после Reset
    {
        float v = (param == "SFXVol") ? SaveSystem.Data.sfxVol : SaveSystem.Data.musicVol;
        s.SetValueWithoutNotify(v);
        Apply(v);
    }
}
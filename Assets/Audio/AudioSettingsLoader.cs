using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsLoader : MonoBehaviour
{
    public AudioMixer mixer;

    void Start()
    {
        Apply("MasterVol");
        Apply("MusicVol");
        Apply("SFXVol");
    }

    void Apply(string param)
    {
        float v = PlayerPrefs.GetFloat(param, 0.8f);
        mixer.SetFloat(param, Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f);
    }
}
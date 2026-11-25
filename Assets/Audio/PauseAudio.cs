using UnityEngine;
using UnityEngine.Audio;

public class PauseAudio : MonoBehaviour
{
    public AudioMixer mixer;
    public AudioMixerSnapshot normalSnapshot, pausedSnapshot;

    void Start()
    {
        mixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
    }

    public void SetPaused(bool isPaused)
    {
        if (isPaused) pausedSnapshot.TransitionTo(0.3f);
        else normalSnapshot.TransitionTo(0.3f);
    }
}
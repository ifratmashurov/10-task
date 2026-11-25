using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    [Header("Audio")]
    public AudioMixer mixer;                       // GameMix
    public AudioMixerSnapshot normalSnapshot;      // Normal
    public AudioMixerSnapshot pausedSnapshot;      // Paused
    public float transitionTime = 0.3f;

    void Start()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);

        // чтобы переход между снапшотами шёл и при Time.timeScale = 0
        if (mixer != null) mixer.updateMode = AudioMixerUpdateMode.UnscaledTime;

        // на старте уровня всегда обычный звук
        if (normalSnapshot != null) normalSnapshot.TransitionTo(0f);
    }

    public void Pause()   { SetPaused(true); }
    public void Resume()  { SetPaused(false); }

    public void Restart()
    {
        ResetAudio();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToMenu()
    {
        ResetAudio();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    void SetPaused(bool paused)
    {
        pausePanel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;

        if (paused)
        {
            if (pausedSnapshot != null) pausedSnapshot.TransitionTo(transitionTime);   // ЗВУК: приглушение
        }
        else
        {
            if (normalSnapshot != null) normalSnapshot.TransitionTo(transitionTime);
        }
    }

    // снапшоты действуют на весь миксер, поэтому перед выходом из паузы возвращаем обычный звук
    void ResetAudio()
    {
        if (normalSnapshot != null) normalSnapshot.TransitionTo(0f);
    }
}
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;
    AudioSource source;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        source = GetComponent<AudioSource>();
        source.volume = SaveSystem.Data.musicVol;
    }
    public void ApplySavedVolume()
{
    source.volume = SaveSystem.Data.musicVol;
}

    public void SetVolume(float v)
    {
        source.volume = v;
        SaveSystem.SetMusicVolume(v);
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveSystem.Save();
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    public AudioMixer mixer;   // перетащи GameMix
    AudioSource a, b, current;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // первый источник: тот, что уже висит на объекте
        a = GetComponent<AudioSource>();
        a.loop = true;
        a.playOnAwake = false;
        a.volume = 0f;

        // второй источник создаём сами, с теми же настройками и выходом
        b = gameObject.AddComponent<AudioSource>();
        b.loop = true;
        b.playOnAwake = false;
        b.spatialBlend = 0f;
        b.volume = 0f;
        b.outputAudioMixerGroup = a.outputAudioMixerGroup;

        current = a;
    }

    void Start()
    {
        ApplySavedVolume();
    }

    // применяет сохранённые громкости к миксеру (вызывается и после Reset)
    public void ApplySavedVolume()
    {
        SetDb("MusicVol", SaveSystem.Data.musicVol);
        SetDb("SFXVol", SaveSystem.Data.sfxVol);
    }

    void SetDb(string param, float v)
    {
        if (mixer != null)
            mixer.SetFloat(param, Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f);
    }

    public void SetVolume(float v)
    {
        SetDb("MusicVol", v);
        SaveSystem.SetMusicVolume(v);
    }

    public void Play(AudioClip clip, float fade = 1f)
    {
        if (clip == null) return;
        if (current.clip == clip && current.isPlaying) return;   // тот же трек: не перезапускаем

        AudioSource next = (current == a) ? b : a;
        next.clip = clip;
        next.volume = 0f;
        next.Play();

        StopAllCoroutines();
        StartCoroutine(Crossfade(current, next, fade));
        current = next;
    }

    IEnumerator Crossfade(AudioSource from, AudioSource to, float time)
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / time)
        {
            from.volume = 1f - t;
            to.volume = t;
            yield return null;
        }
        from.Stop();
        to.volume = 1f;
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveSystem.Save();
    }
}
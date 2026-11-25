using UnityEngine;

public class UIClick : MonoBehaviour
{
    public AudioSource source;   // Output = UI
    public AudioClip click;

    static AudioSource persistent;

    public void Click()
    {
        if (click == null || source == null) return;

        // источник, который не уничтожается при смене сцены
        if (persistent == null)
        {
            var go = new GameObject("UIClickPersistent");
            DontDestroyOnLoad(go);
            persistent = go.AddComponent<AudioSource>();
            persistent.playOnAwake = false;
            persistent.spatialBlend = 0f;
            persistent.outputAudioMixerGroup = source.outputAudioMixerGroup;
        }
        persistent.PlayOneShot(click);
    }
}
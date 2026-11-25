using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public AudioClip track;

    void Start()
    {
        if (MusicPlayer.Instance != null) MusicPlayer.Instance.Play(track);
    }
}
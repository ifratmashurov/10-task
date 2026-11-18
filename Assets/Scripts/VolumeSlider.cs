using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    Slider s;

    void Start()
    {
        s = GetComponent<Slider>();
        Refresh();
        s.onValueChanged.AddListener(v =>
        {
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.SetVolume(v);
            else SaveSystem.SetMusicVolume(v);
        });
    }

    public void Refresh()   // вызывается после Reset
    {
        s.SetValueWithoutNotify(SaveSystem.Data.musicVol);
    }
}
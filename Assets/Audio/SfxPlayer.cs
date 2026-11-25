using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    public AudioSource source;   // Output = SFX
    public AudioClip jump, pickup, death, windup, win, lose;
    public AudioClip[] hits;     // 2–3 версии hit
    int last = -1;

    void Play(AudioClip clip)
    {
        source.pitch = Random.Range(0.92f, 1.08f);
        source.PlayOneShot(clip);
    }

    public void PlayJump()   => Play(jump);
    public void PlayPickup() => Play(pickup);
    public void PlayDeath()  => Play(death);
    public void PlayWindup() => Play(windup);
    public void PlayWin()    => Play(win);
    public void PlayLose()   => Play(lose);

    public void PlayHit()
    {
        int i = Random.Range(0, hits.Length);
        if (i == last && hits.Length > 1) i = (i + 1) % hits.Length;
        last = i;
        Play(hits[i]);
    }
}
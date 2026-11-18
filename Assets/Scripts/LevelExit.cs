using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        SaveSystem.LevelCompleted(LevelController.Current.levelNumber, 0);
        SceneManager.LoadScene(LevelController.Current.nextScene);
    }
}
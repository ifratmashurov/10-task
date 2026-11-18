using UnityEngine;

public class SaveManager : MonoBehaviour
{
    void Awake()
    {
        if (FindFirstObjectByType<SaveManager>() != this)
        {
            Destroy(gameObject);   // уже есть другой, этот лишний
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveSystem.Save();   // игрок свернул приложение
    }
}
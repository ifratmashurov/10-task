using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelController : MonoBehaviour
{
    public static LevelData Current;   // отсюда другие скрипты берут данные уровня

    public LevelData data;
    public TextMeshProUGUI timerText;  // можно оставить пустым

    float timeLeft;

    void Awake()  { Current = data; }
    void Start()  { timeLeft = data.timeLimit; }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timerText != null) timerText.text = "Time: " + Mathf.CeilToInt(timeLeft);
        if (timeLeft <= 0f)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);   // время вышло, уровень заново
    }
}
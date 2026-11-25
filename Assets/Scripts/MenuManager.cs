using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject mainPanel;           // кнопки Play, Quit, Reset
    public GameObject levelSelectPanel;    // панель выбора уровня
    public Button[] levelButtons;          // 3 кнопки по порядку: уровень 1, 2, 3
    public string[] sceneNames = { "Level1", "Level2", "Level3" };
    public GameObject confirmPanel;        // окно "Сбросить прогресс?"
    public VolumeSlider volumeSlider;      // ползунок громкости (обновляется после Reset)

    void Start()
    {
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    public void PlayGame()   // кнопка Play открывает выбор уровня
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        RefreshLevelButtons();
    }

    public void CloseLevelSelect()   // кнопка Back
    {
        levelSelectPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    void RefreshLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            string scene = sceneNames[i];
            Button b = levelButtons[i];

            b.interactable = SaveSystem.IsUnlocked(levelNumber);   // закрытый уровень серый
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => SceneManager.LoadScene(scene));
        }
    }

    public void OpenResetConfirm()   // кнопка Reset progress
    {
        confirmPanel.SetActive(true);
    }

    public void ConfirmReset()
{
    SaveSystem.ResetAll();
    if (MusicPlayer.Instance != null) MusicPlayer.Instance.ApplySavedVolume();

    var vs = volumeSlider != null ? volumeSlider : FindFirstObjectByType<VolumeSlider>();
    if (vs != null) vs.Refresh();

    confirmPanel.SetActive(false);
}

    public void CancelReset()        // кнопка No
    {
        confirmPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
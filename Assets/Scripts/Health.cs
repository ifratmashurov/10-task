using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Обязательно для работы с текстом

public class Health : MonoBehaviour
{
    public int hp = 3;
    public int maxHp = 5;

    [Header("UI")]
    public TextMeshProUGUI hpText; // Сюда мы перетащим наш новый текст

    [Header("Audio")]
    public SfxPlayer sfx;

    [Header("Invincibility")]
    public int blinkCount = 10;
    public float blinkInterval = 0.2f;

    private bool isInvincible;
    private bool isDead;
    private SpriteRenderer[] allRenderers;

    void Awake()
    {
        allRenderers = GetComponentsInChildren<SpriteRenderer>();
        if (sfx == null) sfx = GetComponent<SfxPlayer>();
    }

    void Start()
    {
        UpdateHealthUI(); // Обновляем текст при старте игры
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;

        hp -= damage;
        UpdateHealthUI(); // Обновляем текст при уроне

        if (hp <= 0)
        {
            StartCoroutine(DeathRoutine());
        }
        else
        {
            if (sfx != null) sfx.PlayHit();          // ЗВУК: получение урона
            StartCoroutine(InvincibilityRoutine());
        }
    }

    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp);
        UpdateHealthUI(); // Обновляем текст при лечении
    }

    private void UpdateHealthUI()
    {
        // Обновляем только наш новый текст здоровья
        if (hpText != null)
        {
            hpText.text = $"ЖИЗНИ: {hp}";
        }
    }

    // Смерть: даём звукам доиграть, и только потом перезапускаем уровень
    private IEnumerator DeathRoutine()
    {
        isDead = true;
        if (hpText != null) hpText.text = "ИГРА ОКОНЧЕНА!";

        if (sfx != null) sfx.PlayDeath();            // ЗВУК: смерть
        yield return new WaitForSeconds(0.5f);
        if (sfx != null) sfx.PlayLose();             // ЗВУК: проигрыш
        yield return new WaitForSeconds(1.2f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        for (int i = 0; i < blinkCount; i++)
        {
            foreach (var sr in allRenderers)
                if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
        foreach (var sr in allRenderers)
            if (sr != null) sr.enabled = true;

        isInvincible = false;
    }
}
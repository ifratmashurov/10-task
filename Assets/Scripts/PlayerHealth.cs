using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] PlayerStats stats;        // сюда перетащим PlayerStats
    public TextMeshProUGUI hpText;
    public GameObject[] hearts;                // иконки сердец в HUD

    int currentHealth;                         // ТЕКУЩЕЕ здоровье хранится здесь, не в ассете
    SpriteRenderer spriteRenderer;
    bool isInvincible = false;

    void Start()
    {
        currentHealth = stats.maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
            StartCoroutine(InvincibilityRoutine());
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        for (int i = 0; i < stats.blinkCount; i++)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, stats.blinkAlpha);
            yield return new WaitForSeconds(stats.blinkInterval);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(stats.blinkInterval);
        }
        isInvincible = false;
    }

    void UpdateUI()
    {
        if (hpText != null)
            hpText.text = "HP: " + currentHealth;

        if (hearts != null)
            for (int i = 0; i < hearts.Length; i++)
                hearts[i].SetActive(i < currentHealth);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
            TakeDamage(stats.contactDamage);
    }
}
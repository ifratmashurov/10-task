using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Boss : MonoBehaviour
{
    [SerializeField] BossStats stats;          // сюда перетащим BossStats
    const string NextScene = "WinScene";        // это не баланс, а переход между сценами

    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public Slider bossHpSlider;

    [Header("Audio")]
    public SfxPlayer sfx;
    public float windupTime = 0.5f;            // сколько секунд игрок слышит предупреждение до выстрела
    public float deathDelay = 1f;              // пауза, чтобы звук смерти доиграл

    int currentHealth;
    bool isDead = false;

    void Awake()
    {
        if (sfx == null) sfx = GetComponent<SfxPlayer>();
    }

    void Start()
    {
        currentHealth = stats.maxHealth;

        if (bossHpSlider != null)
        {
            bossHpSlider.maxValue = stats.maxHealth;
            bossHpSlider.value = currentHealth;
        }

        StartCoroutine(ShootRoutine());
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (bossHpSlider != null) bossHpSlider.value = currentHealth;

        GetComponent<SpriteRenderer>().color = Color.red;
        Invoke(nameof(ResetColor), stats.hitFlashTime);

        if (currentHealth <= 0)
        {
            isDead = true;
            SaveSystem.LevelCompleted(3, 0);
            StartCoroutine(DeathRoutine());
        }
        else
        {
            if (sfx != null) sfx.PlayHit();        // ЗВУК: босс получил урон
        }
    }

    void ResetColor()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;
    }

    // Босс скрывается, звук смерти доигрывает, потом загружается WinScene
    IEnumerator DeathRoutine()
    {
        if (sfx != null) sfx.PlayDeath();          // ЗВУК: смерть босса

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
        foreach (var c in GetComponents<Collider2D>()) c.enabled = false;
        if (bossHpSlider != null) bossHpSlider.gameObject.SetActive(false);

        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene(NextScene);
    }

    IEnumerator ShootRoutine()
    {
        while (!isDead)
        {
            // ждём, оставляя время на предупреждение
            yield return new WaitForSeconds(Mathf.Max(0f, stats.shootInterval - windupTime));
            if (isDead) yield break;

            if (sfx != null) sfx.PlayWindup();     // ЗВУК: wind-up, игрок слышит угрозу заранее
            yield return new WaitForSeconds(windupTime);
            if (isDead) yield break;

            if (enemyBulletPrefab != null && firePoint != null)
                Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}
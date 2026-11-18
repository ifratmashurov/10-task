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

    int currentHealth;
    bool isDead = false;

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
    Destroy(gameObject);
    SceneManager.LoadScene(NextScene);
}
    }

    void ResetColor() { GetComponent<SpriteRenderer>().color = Color.white; }

    IEnumerator ShootRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(stats.shootInterval);

            if (enemyBulletPrefab != null && firePoint != null)
                Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}
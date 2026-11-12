using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    void Start()
    {
        // Проверяем, не забыл ли ты Rigidbody 2D
        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError("🛑 ОШИБКА НА ПУЛЕ: У меня нет Rigidbody 2D! Я не смогу никого убить!");
        }

        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Пуля пишет, к чему она прикоснулась
        Debug.Log("🎯 ПУЛЯ КОСНУЛАСЬ: [" + other.gameObject.name + "]. Его Тег: [" + other.tag + "]");

        // NEW (Assignment 10): the three new enemies have EnemyHealth
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(1);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Boss"))
        {
            Boss boss = other.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(1);
            }
            else
            {
                Debug.LogError("🛑 ОШИБКА: Я попала в объект с тегом Boss, но на нём нет скрипта Boss!");
            }

            Destroy(gameObject);
        }
    }
}

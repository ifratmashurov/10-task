using UnityEngine;

// Hit points for the new enemies. PlayerBullet calls TakeDamage.
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int maxHp = 3;   // number of bullets to kill it

    int hp;

    void Awake()
    {
        hp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0) Destroy(gameObject);
    }
}

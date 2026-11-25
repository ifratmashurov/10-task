using UnityEngine;

[CreateAssetMenu(fileName = "BossStats", menuName = "Game/Boss Stats")]
public class BossStats : ScriptableObject
{
    public int maxHealth = 8;            // жизни босса
    public float shootInterval = 2f;     // пауза между выстрелами (сек)
    public float hitFlashTime = 0.1f;    // как долго босс красный после попадания
}
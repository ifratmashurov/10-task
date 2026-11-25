using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Game/Player Stats")]
public class PlayerStats : ScriptableObject
{
    public int maxHealth = 3;            // жизни игрока
    public int contactDamage = 1;        // урон, когда касается врага или босса
    public int blinkCount = 5;           // сколько раз мигает после удара
    public float blinkInterval = 0.2f;   // пауза между миганиями (сек)
    [Range(0f, 1f)]
    public float blinkAlpha = 0.3f;      // прозрачность в момент мигания

    public float speed = 7f;             // скорость бега
    public float jumpForce = 12f;        // сила прыжка
}
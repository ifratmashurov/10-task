using UnityEngine;

// The Shooter's slow shot. Flies in a straight line, hits the player or dies on a wall.
// Prefab needs: Rigidbody2D, a Collider2D with "Is Trigger" ticked, and this script.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] float lifetime = 4f;   // seconds before it disappears

    Rigidbody2D rb;
    int damage = 1;
    LayerMask walls;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;   // needed so trigger events fire
        rb.gravityScale = 0f;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Called by the Shooter right after Instantiate.
    public void Init(Vector2 direction, float speed, int dmg, LayerMask wallMask)
    {
        damage = dmg;
        walls = wallMask;
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player2D>() != null)
        {
            EnemyUtil.HitPlayer(other.gameObject, damage);
            Destroy(gameObject);
            return;
        }

        // Hit something on the Walls layer -> disappear
        if (((1 << other.gameObject.layer) & walls.value) != 0)
            Destroy(gameObject);
    }
}

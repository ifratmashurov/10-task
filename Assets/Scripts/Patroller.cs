
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Patroller : MonoBehaviour
{
    private enum State
    {
        Patrol,
        Chase,
        Attack,
        Search
    }

    private State state = State.Patrol;

    private float timer;
    private float nextAttackTime;
    private float sightTimer;
    private float lastSeenX;

    private bool struck;
    private bool sees;

    private int facing = 1;
    private int pointIndex;

    private Color baseColor;

    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] points;
    [Tooltip("Выбери только слой Walls")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Движение")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float searchSpeed = 2f;

    [Header("Зрение")]
    [SerializeField] private float viewRange = 6f;
    [SerializeField, Range(10, 360)] private float fov = 90f;
    [SerializeField] private float sightInterval = 0.15f;
    [Tooltip("Время поиска игрока в последней известной позиции")]
    [SerializeField] private float searchTime = 2f;

    [Header("Атака")]
    [SerializeField] private float attackRange = 1.2f;
    [Tooltip("Если игрок выше этого значения, удар не попадёт")]
    [SerializeField] private float attackHeight = 1.5f;
    [Tooltip("Предупреждение перед ударом, от 0.3 до 0.8 секунды")]
    [SerializeField, Range(0.3f, 0.8f)] private float windUp = 0.5f;
    [SerializeField] private float recovery = 0.6f;
    [SerializeField] private float cooldown = 1.5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private Color warningColor = Color.red;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Collider2D col;
    private Collider2D playerCol;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        if (rb == null || col == null)
        {
            Debug.LogError("Patroller requires Rigidbody2D and Collider2D!", this);
            enabled = false;
            return;
        }

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (sprite != null)
            baseColor = sprite.color;

        facing = transform.localScale.x < 0 ? -1 : 1;

        if (player == null)
        {
            PlayerHealth found = FindAnyObjectByType<PlayerHealth>();

            if (found == null)
            {
                Debug.LogError("Patroller: PlayerHealth was not found!", this);
                enabled = false;
                return;
            }

            player = found.transform;
        }

        playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            playerHealth = player.GetComponentInChildren<PlayerHealth>();

        playerCol = player.GetComponent<Collider2D>();

        if (playerCol == null)
            playerCol = player.GetComponentInChildren<Collider2D>();

        if (playerCol == null)
        {
            Debug.LogError("Patroller: Player Collider2D is missing!", this);
            enabled = false;
            return;
        }

        if (playerHealth == null)
            Debug.LogWarning("Patroller: PlayerHealth is missing. Attacks will not deal damage.", this);

        if (obstacleMask.value == 0)
            Debug.LogWarning("Patroller: Obstacle Mask is empty. Vision may pass through walls.", this);

        if (points == null || points.Length == 0)
            Debug.LogWarning("Patroller: No patrol points assigned.", this);

        if (sightInterval <= 0f)
            sightInterval = 0.15f;

        if (sprite != null)
            baseColor = sprite.color;
    }

    private void Update()
    {
        if (!enabled || player == null || col == null || playerCol == null)
            return;

        timer += Time.deltaTime;
        sightTimer -= Time.deltaTime;

        if (sightTimer <= 0f)
        {
            sightTimer = sightInterval;
            sees = CanSeePlayer();
        }

        if (sees)
            lastSeenX = player.position.x;

        switch (state)
        {
            case State.Patrol:
                Patrol();

                if (sees)
                    SetState(State.Chase);
                break;

            case State.Chase:
                MoveX(player.position.x, chaseSpeed);

                if (!sees)
                {
                    SetState(State.Search);
                }
                else if (DistanceX() < attackRange &&
                         Time.time >= nextAttackTime &&
                         EnemySense.IsOnScreen(transform.position))
                {
                    SetState(State.Attack);
                }
                break;

            case State.Attack:
                Stop();

                if (!struck && timer >= windUp)
                {
                    struck = true;

                    if (sprite != null)
                        sprite.color = baseColor;

                    if (DistanceX() < attackRange * 1.2f &&
                        Mathf.Abs(player.position.y - transform.position.y) < attackHeight &&
                        playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                    }
                }
                else if (struck && timer >= windUp + recovery)
                {
                    nextAttackTime = Time.time + cooldown;
                    SetState(sees ? State.Chase : State.Search);
                }
                break;

            case State.Search:
                if (Mathf.Abs(lastSeenX - transform.position.x) > 0.3f)
                    MoveX(lastSeenX, searchSpeed);
                else
                    Stop();

                if (sees)
                    SetState(State.Chase);
                else if (timer > searchTime &&
                         Mathf.Abs(lastSeenX - transform.position.x) <= 0.3f)
                    SetState(State.Patrol);
                else if (timer > searchTime + 4f)
                    SetState(State.Patrol);
                break;
        }
    }

    private void SetState(State next)
    {
        state = next;
        timer = 0f;
        struck = false;

        if (sprite != null)
            sprite.color = next == State.Attack ? warningColor : baseColor;
    }

    private void Patrol()
    {
        if (points == null || points.Length == 0)
        {
            Stop();
            return;
        }

        if (points[pointIndex] == null)
        {
            Stop();
            return;
        }

        MoveX(points[pointIndex].position.x, patrolSpeed);

        if (Mathf.Abs(points[pointIndex].position.x - transform.position.x) < 0.1f)
            pointIndex = (pointIndex + 1) % points.Length;
    }

    private void MoveX(float targetX, float speed)
    {
        float dir = Mathf.Sign(targetX - transform.position.x);

        if (dir == 0f)
        {
            Stop();
            return;
        }

        facing = (int)dir;

        rb.linearVelocity = new Vector2(
            dir * speed,
            rb.linearVelocity.y
        );

        EnemySense.Flip(transform, facing);
    }

    private void Stop()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private float DistanceX()
    {
        if (player == null)
            return float.MaxValue;

        return Mathf.Abs(player.position.x - transform.position.x);
    }

    private bool CanSeePlayer()
    {
        if (col == null || playerCol == null ||
            player == null || !player.gameObject.activeInHierarchy)
            return false;

        Vector2 eye = col.bounds.center;
        Vector2 target = playerCol.bounds.center;

        if (!EnemySense.InCone(eye, facing, target, viewRange, fov))
            return false;

        return EnemySense.HasLineOfSight(eye, target, obstacleMask);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRange);

        Vector3 forward = new Vector3(
            transform.localScale.x < 0 ? -1 : 1,
            0,
            0
        );

        Gizmos.color = Color.cyan;

        Gizmos.DrawRay(
            transform.position,
            Quaternion.Euler(0, 0, fov / 2f) * forward * viewRange
        );

        Gizmos.DrawRay(
            transform.position,
            Quaternion.Euler(0, 0, -fov / 2f) * forward * viewRange
        );
    }
}

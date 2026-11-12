using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Shooter : MonoBehaviour
{
    enum State { Idle, Detect, Aim, Shoot, Reposition }
    State state;
    float timer, nextShotTime, sightTimer;
    bool sees, fired;
    int facing = 1;
    Vector2 aimDir;
    Color baseColor;

    [Header("Ссылки")]
    [SerializeField] Transform player;
    [SerializeField] EnemyProjectile projectilePrefab;
    [Tooltip("Дочерний пустой объект у 'дула'. Если пусто, берётся центр врага")]
    [SerializeField] Transform firePoint;
    [SerializeField] LayerMask obstacleMask;

    [Header("Зрение")]
    [SerializeField] float viewRange = 12f;
    [SerializeField, Range(10, 360)] float fov = 270f;
    [SerializeField] float sightInterval = 0.15f;

    [Header("Дистанция")]
    [Tooltip("Ближе этого он отступает")]
    [SerializeField] float minDistance = 4f;
    [Tooltip("Дальше этого он подходит")]
    [SerializeField] float maxDistance = 8f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float repositionTime = 1.2f;

    [Header("Атака")]
    [SerializeField] float detectTime = 0.3f;
    [Tooltip("Время лазера до выстрела, сек (0.3-0.8)")]
    [SerializeField] float aimTime = 0.6f;
    [SerializeField] float shootPause = 0.2f;
    [SerializeField] float cooldown = 2f;
    [SerializeField] float projectileSpeed = 8f;
    [SerializeField] int projectileDamage = 1;

    [Header("Визуал")]
    [SerializeField] Color laserColor = new Color(1f, 0.1f, 0.1f, 0.8f);
    [SerializeField] float laserWidth = 0.06f;
    [SerializeField] Color aimTint = new Color(1f, 0.6f, 0.6f);

    Rigidbody2D rb;
    SpriteRenderer sprite;
    Collider2D col, playerCol;
    LineRenderer laser;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        col = GetComponent<Collider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        baseColor = sprite.color;
        facing = transform.localScale.x < 0 ? -1 : 1;
        laser = EnemySense.CreateLine(gameObject, sprite.sharedMaterial, laserColor, laserWidth);

        if (player == null) player = FindFirstObjectByType<PlayerHealth>().transform;
        playerCol = player.GetComponent<Collider2D>();
        if (playerCol == null) playerCol = player.GetComponentInChildren<Collider2D>();
        if (obstacleMask.value == 0) Debug.LogWarning("Shooter: Obstacle Mask пустая!", this);
        if (projectilePrefab == null) Debug.LogError("Shooter: не назначен Projectile Prefab!", this);
    }

    void Update()
    {
        timer += Time.deltaTime;
        sightTimer -= Time.deltaTime;
        if (sightTimer <= 0f) { sightTimer = sightInterval; sees = CanSeePlayer(); }

        switch (state)
        {
            case State.Idle:
                Stop();
                if (sees) SetState(State.Detect);
                break;

            case State.Detect:
                Stop();
                FacePlayer();
                if (timer < detectTime) break;
                if (!sees) SetState(State.Idle);
                else if (Time.time >= nextShotTime) SetState(State.Aim);
                else if (Dist() < minDistance || Dist() > maxDistance) SetState(State.Reposition);
                break; // иначе стоит и ждёт конца cooldown

            case State.Aim:
                Stop();
                if (timer >= aimTime) SetState(State.Shoot);
                break;

            case State.Shoot:
                Stop();
                if (!fired) { fired = true; Fire(); }
                if (timer >= shootPause) SetState(State.Reposition);
                break;

            case State.Reposition:
                FacePlayer();
                float toPlayer = Mathf.Sign(player.position.x - transform.position.x);
                float dir = 0f;
                if (Dist() < minDistance) dir = -toPlayer;          // отступаем
                else if (Dist() > maxDistance) dir = toPlayer;      // подходим
                if (dir != 0f && CanWalk(dir))
                    rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
                else Stop();
                if (timer >= repositionTime) SetState(sees ? State.Detect : State.Idle);
                break;
        }
    }

    void SetState(State next)
    {
        state = next;
        timer = 0f;
        fired = false;
        laser.enabled = false;
        sprite.color = baseColor;

        if (next == State.Aim)
        {
            // направление фиксируется в начале прицеливания: игрок успевает отойти
            Vector2 origin = Origin();
            aimDir = ((Vector2)playerCol.bounds.center - origin).normalized;
            sprite.color = aimTint;
            RaycastHit2D hit = Physics2D.Raycast(origin, aimDir, viewRange, obstacleMask);
            Vector2 end = hit.collider != null ? hit.point : origin + aimDir * viewRange;
            laser.SetPosition(0, origin);
            laser.SetPosition(1, end);
            laser.enabled = true;
        }
    }

    void Fire()
    {
        // не стреляем, если враг или игрок за экраном
        if (projectilePrefab == null) return;
        if (!EnemySense.IsOnScreen(transform.position) || !EnemySense.IsOnScreen(playerCol.bounds.center)) return;
        EnemyProjectile p = Instantiate(projectilePrefab, Origin(), Quaternion.identity);
        p.Init(aimDir, projectileSpeed, projectileDamage, obstacleMask);
        nextShotTime = Time.time + cooldown;
    }

    Vector2 Origin() { return firePoint != null ? (Vector2)firePoint.position : (Vector2)col.bounds.center; }
    float Dist() { return Mathf.Abs(player.position.x - transform.position.x); }
    void Stop() { rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); }

    void FacePlayer()
    {
        facing = player.position.x >= transform.position.x ? 1 : -1;
        EnemySense.Flip(transform, facing);
    }

    // Нет стены впереди и есть пол под ногами (не сойдёт с платформы)
    bool CanWalk(float dir)
    {
        Bounds b = col.bounds;
        Vector2 side = new Vector2(dir, 0f);
        if (Physics2D.Raycast(b.center, side, b.extents.x + 0.2f, obstacleMask).collider != null) return false;
        Vector2 footProbe = new Vector2(b.center.x + dir * (b.extents.x + 0.2f), b.center.y);
        return Physics2D.Raycast(footProbe, Vector2.down, b.extents.y + 0.6f, obstacleMask).collider != null;
    }

    bool CanSeePlayer()
    {
        Vector2 eye = col.bounds.center;
        Vector2 target = playerCol.bounds.center;
        if (!EnemySense.IsOnScreen(target)) return false;
        if (!EnemySense.InCone(eye, facing, target, viewRange, fov)) return false;
        return EnemySense.HasLineOfSight(eye, target, obstacleMask);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, viewRange);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.color = Color.green;  Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}
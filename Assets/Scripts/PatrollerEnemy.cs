using TMPro;
using UnityEngine;

// PATROLLER: walks between waypoints, chases what it sees, short melee hit.
// States: Patrol -> Chase -> WindUp -> Recover -> Chase ...
// Tests the player's POSITIONING: stay out of its view, use walls as cover.
[RequireComponent(typeof(Rigidbody2D))]
public class PatrollerEnemy : MonoBehaviour
{
    enum State { Patrol, Chase, WindUp, Recover }

    [Header("References")]
    [SerializeField] Transform player;            // leave empty: found once in Awake
    [SerializeField] Transform[] points;          // 2+ waypoints placed in the level (NOT children of the enemy)
    [SerializeField] LayerMask obstacleMask;      // tick ONLY the Walls layer
    [SerializeField] TMP_Text stateLabel;         // optional 3D text above the head (debug)
    [SerializeField] bool spriteFacesRight = true;

    [Header("Sight")]
    [SerializeField] float viewRange = 6f;        // meters
    [SerializeField] float fov = 120f;            // degrees, whole cone
    [SerializeField] float eyeHeight = 0.5f;      // meters above the pivot
    [SerializeField] float sightInterval = 0.15f; // seconds between sight checks
    [SerializeField] float loseTime = 2f;         // seconds without seeing -> back to Patrol

    [Header("Movement")]
    [SerializeField] float patrolSpeed = 1.7f;    // meters/second
    [SerializeField] float chaseSpeed = 3.5f;     // meters/second (player runs at 7)

    [Header("Attack")]
    [SerializeField] float attackRange = 1.2f;    // meters, starts the wind-up
    [SerializeField] float strikeTolerance = 0.3f;// meters, extra reach when the hit lands
    [SerializeField] float windUp = 0.5f;         // seconds of warning (0.3-0.8)
    [SerializeField] float recoverTime = 0.4f;    // seconds standing still after the hit
    [SerializeField] float attackCooldown = 1f;   // seconds before the next attack
    [SerializeField] int damage = 1;
    [SerializeField] Color windUpColor = new Color(1f, 0.25f, 0.25f);

    Rigidbody2D rb;
    SpriteRenderer sr;
    Color baseColor = Color.white;

    State state;
    float timer;          // seconds in the current state
    float sightTimer;     // seconds until the next sight check
    float sinceSeen;      // Chase: seconds since the player was last seen
    float cooldownTimer;  // seconds until an attack is allowed again
    bool sees;
    int facing = 1;       // +1 right, -1 left
    int pointIndex;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        if (player == null) player = EnemyUtil.FindPlayer();
        sightTimer = Random.Range(0f, sightInterval);   // spread raycasts over several frames
        SetState(State.Patrol);
    }

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;
        cooldownTimer -= Time.deltaTime;

        // SENSE: look for the player a few times per second, not every frame
        sightTimer -= Time.deltaTime;
        if (sightTimer <= 0f)
        {
            sightTimer = sightInterval;
            sees = EnemyUtil.CanSee(Eye(), new Vector2(facing, 0f), player, viewRange, fov, obstacleMask);
        }

        // DECIDE + ACT
        switch (state)
        {
            case State.Patrol:
                Patrol();
                if (sees) SetState(State.Chase);
                break;

            case State.Chase:
                if (sees) sinceSeen = 0f; else sinceSeen += Time.deltaTime;
                MoveToward(player.position.x, chaseSpeed);

                if (DistanceToPlayer() < attackRange && cooldownTimer <= 0f
                    && EnemyUtil.IsOnScreen(transform.position))
                    SetState(State.WindUp);
                else if (sinceSeen > loseTime)
                    SetState(State.Patrol);
                break;

            case State.WindUp:                       // stands still, red: this is the warning
                if (timer >= windUp)
                {
                    Strike();
                    SetState(State.Recover);
                }
                break;

            case State.Recover:                      // stands still: the player's turn
                if (timer >= recoverTime) SetState(State.Chase);
                break;
        }
    }

    // All state changes go through here: it resets the timer and runs the enter logic.
    void SetState(State next)
    {
        state = next;
        timer = 0f;

        if (next == State.Chase) sinceSeen = 0f;
        if (next == State.WindUp || next == State.Recover) Stop();
        if (sr != null) sr.color = next == State.WindUp ? windUpColor : baseColor;
        if (stateLabel != null) stateLabel.text = next.ToString();
    }

    void Patrol()
    {
        if (points == null || points.Length == 0)
        {
            Stop();
            return;
        }

        float targetX = points[pointIndex].position.x;
        MoveToward(targetX, patrolSpeed);

        if (Mathf.Abs(targetX - transform.position.x) < 0.1f)
            pointIndex = (pointIndex + 1) % points.Length;    // next point, loop to 0
    }

    void Strike()
    {
        // The hit lands only if the player is still in reach: running away during the wind-up works.
        if (DistanceToPlayer() <= attackRange + strikeTolerance)
            EnemyUtil.HitPlayer(player.gameObject, damage);

        cooldownTimer = attackCooldown;
    }

    void MoveToward(float targetX, float speed)
    {
        float dx = targetX - transform.position.x;
        if (Mathf.Abs(dx) < 0.05f)
        {
            Stop();
            return;
        }

        facing = dx > 0f ? 1 : -1;
        EnemyUtil.ApplyFacing(sr, facing, spriteFacesRight);
        rb.linearVelocity = new Vector2(facing * speed, rb.linearVelocity.y);
    }

    void Stop()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    Vector2 Eye()
    {
        return (Vector2)transform.position + Vector2.up * eyeHeight;
    }

    float DistanceToPlayer()
    {
        return Vector2.Distance(transform.position, player.position);
    }

    void OnDrawGizmosSelected()
    {
        EnemyUtil.DrawVisionGizmo(transform.position + Vector3.up * eyeHeight, facing, viewRange, fov);
    }
}

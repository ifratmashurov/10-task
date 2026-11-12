using TMPro;
using UnityEngine;

// SHOOTER: keeps its distance, aims with a red laser, fires a slow shot.
// States: Idle -> KeepDistance -> Aim -> Cooldown -> KeepDistance ...
// Tests the player's REACTION: step out of the laser line before the shot.
[RequireComponent(typeof(Rigidbody2D))]
public class ShooterEnemy : MonoBehaviour
{
    enum State { Idle, KeepDistance, Aim, Cooldown }

    [Header("References")]
    [SerializeField] Transform player;              // leave empty: found once in Awake
    [SerializeField] LayerMask obstacleMask;        // tick ONLY the Walls layer
    [SerializeField] GameObject projectilePrefab;   // prefab with EnemyProjectile
    [SerializeField] Transform muzzle;              // optional: where the shot starts (default: eye)
    [SerializeField] LineRenderer laser;            // optional: if empty it is created at runtime
    [SerializeField] TMP_Text stateLabel;           // optional 3D text above the head (debug)
    [SerializeField] bool spriteFacesRight = true;

    [Header("Sight")]
    [SerializeField] float viewRange = 8f;          // meters
    [SerializeField] float fov = 360f;              // degrees, 360 = sees all around
    [SerializeField] float eyeHeight = 0.5f;        // meters above the pivot
    [SerializeField] float sightInterval = 0.15f;   // seconds between sight checks
    [SerializeField] float loseTime = 2f;           // seconds without seeing -> back to Idle

    [Header("Movement")]
    [SerializeField] float keepDistance = 5f;       // meters it wants to stay away
    [SerializeField] float distanceTolerance = 1f;  // meters, no movement inside keepDistance +- this
    [SerializeField] float moveSpeed = 2f;          // meters/second

    [Header("Attack")]
    [SerializeField] float aimTime = 0.6f;          // seconds the laser is visible before the shot
    [SerializeField] float cooldownTime = 1.5f;     // seconds after the shot
    [SerializeField] float projectileSpeed = 5f;    // meters/second
    [SerializeField] int damage = 1;
    [SerializeField] Color laserColor = new Color(1f, 0.1f, 0.1f);
    [SerializeField] float laserWidth = 0.06f;      // meters

    Rigidbody2D rb;
    SpriteRenderer sr;

    State state;
    float timer;          // seconds in the current state
    float sightTimer;     // seconds until the next sight check
    float sinceSeen;      // seconds since the player was last seen
    bool sees;
    int facing = -1;      // +1 right, -1 left
    Vector2 aimDir;       // direction locked when the Aim state starts

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (player == null) player = EnemyUtil.FindPlayer();
        if (projectilePrefab == null) Debug.LogError("ShooterEnemy: no projectilePrefab assigned!", this);

        SetupLaser();
        sightTimer = Random.Range(0f, sightInterval);
        SetState(State.Idle);
    }

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;

        // SENSE
        sightTimer -= Time.deltaTime;
        if (sightTimer <= 0f)
        {
            sightTimer = sightInterval;
            sees = EnemyUtil.CanSee(Eye(), new Vector2(facing, 0f), player, viewRange, fov, obstacleMask);
        }

        // DECIDE + ACT
        switch (state)
        {
            case State.Idle:
                if (sees) SetState(State.KeepDistance);
                break;

            case State.KeepDistance:
                if (sees) sinceSeen = 0f; else sinceSeen += Time.deltaTime;
                FacePlayer();
                KeepDistance();

                if (sinceSeen > loseTime)
                    SetState(State.Idle);
                else if (sees && InShootingBand() && EnemyUtil.IsOnScreen(transform.position))
                    SetState(State.Aim);
                break;

            case State.Aim:                          // stands still, laser shows the line
                DrawLaser();
                if (timer >= aimTime)
                {
                    Fire();
                    SetState(State.Cooldown);
                }
                break;

            case State.Cooldown:                     // stands still: the player's turn
                if (timer >= cooldownTime) SetState(State.KeepDistance);
                break;
        }
    }

    // All state changes go through here: it resets the timer and runs the enter logic.
    void SetState(State next)
    {
        state = next;
        timer = 0f;

        if (next == State.KeepDistance) sinceSeen = 0f;
        if (next == State.Idle || next == State.Aim || next == State.Cooldown) Stop();

        if (next == State.Aim)
        {
            // Lock the direction NOW. The player can step aside while the laser is on.
            Vector2 target = (Vector2)player.position + Vector2.up * 0.5f;
            aimDir = (target - MuzzlePos()).normalized;
            FacePlayer();
        }

        if (laser != null) laser.enabled = next == State.Aim;
        if (stateLabel != null) stateLabel.text = next.ToString();
    }

    void KeepDistance()
    {
        float dx = player.position.x - transform.position.x;
        float dist = Vector2.Distance(transform.position, player.position);
        float dirToPlayer = dx > 0f ? 1f : -1f;

        if (dist < keepDistance - distanceTolerance)
            rb.linearVelocity = new Vector2(-dirToPlayer * moveSpeed, rb.linearVelocity.y);   // too close: back off
        else if (dist > keepDistance + distanceTolerance)
            rb.linearVelocity = new Vector2(dirToPlayer * moveSpeed, rb.linearVelocity.y);    // too far: come closer
        else
            Stop();
    }

    bool InShootingBand()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        return Mathf.Abs(dist - keepDistance) <= distanceTolerance;
    }

    void Fire()
    {
        if (projectilePrefab == null) return;

        GameObject shot = Instantiate(projectilePrefab, MuzzlePos(), Quaternion.identity);
        EnemyProjectile p = shot.GetComponent<EnemyProjectile>();
        if (p != null) p.Init(aimDir, projectileSpeed, damage, obstacleMask);
    }

    void DrawLaser()
    {
        if (laser == null) return;

        Vector2 start = MuzzlePos();
        float length = viewRange;
        RaycastHit2D hit = Physics2D.Raycast(start, aimDir, viewRange, obstacleMask);
        if (hit.collider != null) length = hit.distance;      // the laser stops at a wall

        laser.SetPosition(0, start);
        laser.SetPosition(1, start + aimDir * length);
    }

    void SetupLaser()
    {
        if (laser == null)
        {
            GameObject go = new GameObject("Laser");
            go.transform.SetParent(transform, false);
            laser = go.AddComponent<LineRenderer>();
            laser.material = new Material(Shader.Find("Sprites/Default"));
            laser.sortingOrder = 10;
        }

        laser.useWorldSpace = true;
        laser.positionCount = 2;
        laser.startWidth = laserWidth;
        laser.endWidth = laserWidth;
        laser.startColor = laserColor;
        laser.endColor = laserColor;
        laser.enabled = false;
    }

    void FacePlayer()
    {
        facing = player.position.x > transform.position.x ? 1 : -1;
        EnemyUtil.ApplyFacing(sr, facing, spriteFacesRight);
    }

    void Stop()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    Vector2 Eye()
    {
        return (Vector2)transform.position + Vector2.up * eyeHeight;
    }

    Vector2 MuzzlePos()
    {
        return muzzle != null ? (Vector2)muzzle.position : Eye();
    }

    void OnDrawGizmosSelected()
    {
        EnemyUtil.DrawVisionGizmo(transform.position + Vector3.up * eyeHeight, facing, viewRange, fov);
    }
}

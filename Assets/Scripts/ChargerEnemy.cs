using TMPro;
using UnityEngine;

// CHARGER: waits, winds up, dashes in a straight line, gets stunned.
// States: Idle -> WindUp -> Dash -> Stunned -> Idle ...
// Tests the player's TIMING: jump over or sidestep at the end of the wind-up, hit it while stunned.
[RequireComponent(typeof(Rigidbody2D))]
public class ChargerEnemy : MonoBehaviour
{
    enum State { Idle, WindUp, Dash, Stunned }

    [Header("References")]
    [SerializeField] Transform player;            // leave empty: found once in Awake
    [SerializeField] LayerMask obstacleMask;      // tick ONLY the Walls layer
    [SerializeField] TMP_Text stateLabel;         // optional 3D text above the head (debug)
    [SerializeField] bool spriteFacesRight = true;

    [Header("Sight")]
    [SerializeField] float triggerRange = 6f;     // meters, it notices the player inside this
    [SerializeField] float maxHeightDiff = 1.5f;  // meters, ignores the player on another platform
    [SerializeField] float eyeHeight = 0.5f;      // meters above the pivot
    [SerializeField] float sightInterval = 0.15f; // seconds between sight checks

    [Header("Attack")]
    [SerializeField] float windUp = 0.7f;         // seconds of warning (0.3-0.8)
    [SerializeField] float dashSpeed = 9f;        // meters/second
    [SerializeField] float dashTime = 0.6f;       // seconds (dash length = speed x time)
    [SerializeField] float stunTime = 1f;         // seconds helpless after the dash
    [SerializeField] float cooldown = 1f;         // seconds after the stun before the next wind-up
    [SerializeField] float wallCheckDistance = 0.6f; // meters, a wall this close stops the dash
    [SerializeField] int damage = 1;
    [SerializeField] Color windUpColor = new Color(1f, 0.25f, 0.25f);
    [SerializeField] Color stunColor = new Color(0.6f, 0.6f, 1f);

    Rigidbody2D rb;
    SpriteRenderer sr;
    Color baseColor = Color.white;

    State state;
    float timer;          // seconds in the current state
    float sightTimer;     // seconds until the next sight check
    float cooldownTimer;  // seconds until a wind-up is allowed again
    bool sees;
    int facing = 1;       // +1 right, -1 left
    int dashDir = 1;      // direction locked when the wind-up starts
    bool hitThisDash;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        if (player == null) player = EnemyUtil.FindPlayer();
        sightTimer = Random.Range(0f, sightInterval);
        SetState(State.Idle);
    }

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;
        cooldownTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Idle:
                // SENSE: a few times per second
                sightTimer -= Time.deltaTime;
                if (sightTimer <= 0f)
                {
                    sightTimer = sightInterval;
                    sees = CanNotice();
                }

                if (sees && cooldownTimer <= 0f && EnemyUtil.IsOnScreen(transform.position))
                    SetState(State.WindUp);
                break;

            case State.WindUp:                       // stands still, red: this is the warning
                if (timer >= windUp) SetState(State.Dash);
                break;

            case State.Dash:
                rb.linearVelocity = new Vector2(dashDir * dashSpeed, rb.linearVelocity.y);
                if (timer >= dashTime || WallAhead()) SetState(State.Stunned);
                break;

            case State.Stunned:                      // helpless: the player's turn
                if (timer >= stunTime) SetState(State.Idle);
                break;
        }
    }

    // All state changes go through here: it resets the timer and runs the enter logic.
    void SetState(State next)
    {
        state = next;
        timer = 0f;

        switch (next)
        {
            case State.Idle:
                Stop();
                cooldownTimer = cooldown;
                sees = false;
                break;

            case State.WindUp:
                Stop();
                // Lock the direction NOW. The player can jump over or step away.
                dashDir = player.position.x > transform.position.x ? 1 : -1;
                Face(dashDir);
                break;

            case State.Dash:
                hitThisDash = false;
                break;

            case State.Stunned:
                Stop();
                break;
        }

        if (sr != null)
        {
            if (next == State.WindUp) sr.color = windUpColor;
            else if (next == State.Stunned) sr.color = stunColor;
            else sr.color = baseColor;
        }

        if (stateLabel != null) stateLabel.text = next.ToString();
    }

    // Distance + same height + line of sight (walls block it).
    bool CanNotice()
    {
        if (Mathf.Abs(player.position.y - transform.position.y) > maxHeightDiff) return false;

        // Look toward the player first, then run the three checks (fov 360: angle always passes).
        Face(player.position.x > transform.position.x ? 1 : -1);
        return EnemyUtil.CanSee(Eye(), new Vector2(facing, 0f), player, triggerRange, 360f, obstacleMask);
    }

    bool WallAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(Eye(), new Vector2(dashDir, 0f), wallCheckDistance, obstacleMask);
        return hit.collider != null;
    }

    // Damage the player once per dash, only while dashing.
    void OnCollisionEnter2D(Collision2D collision)
    {
        TryHit(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TryHit(collision);
    }

    void TryHit(Collision2D collision)
    {
        if (state != State.Dash || hitThisDash) return;
        if (collision.collider.GetComponentInParent<Player2D>() == null) return;

        hitThisDash = true;
        EnemyUtil.HitPlayer(collision.collider.gameObject, damage);
    }

    void Face(int dir)
    {
        facing = dir;
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

    void OnDrawGizmosSelected()
    {
        EnemyUtil.DrawVisionGizmo(transform.position + Vector3.up * eyeHeight, facing, triggerRange, 360f);
    }
}

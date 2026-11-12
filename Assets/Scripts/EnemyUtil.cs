using UnityEngine;

// Small helpers shared by the three enemies.
public static class EnemyUtil
{
    // Find the player ONCE (call in Awake). Never call this in Update.
    public static Transform FindPlayer()
    {
        Player2D p = Object.FindFirstObjectByType<Player2D>();
        return p != null ? p.transform : null;
    }

    // The project has two health scripts:
    // Health (Level1) and PlayerHealth (Level2, Level3). Hit whichever one is on the player.
    public static void HitPlayer(GameObject player, int damage)
    {
        PlayerHealth ph = player.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            return;
        }

        Health h = player.GetComponentInParent<Health>();
        if (h != null) h.TakeDamage(damage);
    }

    // True if the world point is inside the camera view. Enemies never start an attack off-screen.
    public static bool IsOnScreen(Vector3 worldPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return true;
        Vector3 v = cam.WorldToViewportPoint(worldPos);
        return v.z > 0f && v.x > 0f && v.x < 1f && v.y > 0f && v.y < 1f;
    }

    // Three checks, cheap ones first: 1) distance  2) angle  3) walls (Raycast).
    // facing = direction the enemy looks (Vector2.right or Vector2.left).
    public static bool CanSee(Vector2 eye, Vector2 facing, Transform player,
                              float viewRange, float fov, LayerMask walls)
    {
        Vector2 toPlayer = (Vector2)player.position - eye;
        float dist = toPlayer.magnitude;

        if (dist > viewRange) return false;                                   // 1. distance
        if (dist > 0.01f && Vector2.Angle(facing, toPlayer) > fov * 0.5f)     // 2. angle
            return false;

        // 3. walls: any hit on the Walls layer before the player = blocked
        RaycastHit2D hit = Physics2D.Raycast(eye, toPlayer.normalized, dist, walls);
        return hit.collider == null;
    }

    // Flip the sprite to look left (-1) or right (+1).
    public static void ApplyFacing(SpriteRenderer sr, int facing, bool spriteFacesRight)
    {
        if (sr == null) return;
        sr.flipX = spriteFacesRight ? facing < 0 : facing > 0;
    }

    // Debug: draw range and view cone in the Scene view (used in OnDrawGizmosSelected).
    public static void DrawVisionGizmo(Vector3 eye, int facing, float range, float fov)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye, range);

        Vector3 dir = new Vector3(facing, 0f, 0f);
        Vector3 a = Quaternion.Euler(0f, 0f, -fov * 0.5f) * dir;
        Vector3 b = Quaternion.Euler(0f, 0f, fov * 0.5f) * dir;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eye, a * range);
        Gizmos.DrawRay(eye, b * range);
    }
}

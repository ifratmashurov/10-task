using UnityEngine;

public static class EnemySense
{
    static Camera cam;

    // Точка в кадре? Враг не атакует из-за экрана.
    public static bool IsOnScreen(Vector3 worldPos, float margin = 0.02f)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return true;
        Vector3 v = cam.WorldToViewportPoint(worldPos);
        return v.z > 0f && v.x > margin && v.x < 1f - margin && v.y > margin && v.y < 1f - margin;
    }

    // Прямая видимость: true, если на луче нет стен (по маске Walls).
    public static bool HasLineOfSight(Vector2 from, Vector2 to, LayerMask wallMask)
    {
        Vector2 d = to - from;
        return Physics2D.Raycast(from, d.normalized, d.magnitude, wallMask).collider == null;
    }

    // Дистанция + угол обзора относительно направления взгляда.
    public static bool InCone(Vector2 from, int facing, Vector2 to, float range, float fov)
    {
        Vector2 d = to - from;
        if (d.sqrMagnitude > range * range) return false;
        return Vector2.Angle(new Vector2(facing, 0f), d) <= fov * 0.5f;
    }

    public static void Flip(Transform t, int facing)
    {
        Vector3 s = t.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        t.localScale = s;
    }

    // Линия-предупреждение (лазер / путь рывка). Материал берём у спрайта врага.
    public static LineRenderer CreateLine(GameObject owner, Material mat, Color color, float width)
    {
        var go = new GameObject("Telegraph");
        go.transform.SetParent(owner.transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.startWidth = lr.endWidth = width;
        lr.sharedMaterial = mat;
        lr.startColor = lr.endColor = color;
        lr.sortingOrder = 10;
        lr.enabled = false;
        return lr;
    }
}
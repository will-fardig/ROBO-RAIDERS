using UnityEngine;
using UnityEngine.Tilemaps;

public class CamFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.08f;
    [SerializeField] private Vector2 offset = Vector2.zero;

    [Header("Map Bounds")]
    [Tooltip("The ground tilemap. The camera never shows past its painted edges.")]
    [SerializeField] private Tilemap boundsTilemap;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    private Vector3 smoothPos;
    private Bounds mapBounds;
    private bool hasBounds;

    // --- Screen shake ---
    private static CamFollow instance;
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    private void Awake()
    {
        instance = this;
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        smoothPos = transform.position;

        if (boundsTilemap != null)
        {
            mapBounds = GetTilemapWorldBounds(boundsTilemap);
            hasBounds = true;
        }
    }

    // World-space rectangle covering only the painted tiles
    public static Bounds GetTilemapWorldBounds(Tilemap tilemap)
    {
        tilemap.CompressBounds(); // ignore empty cells around the edges
        Bounds local = tilemap.localBounds;
        Vector3 a = tilemap.transform.TransformPoint(local.min);
        Vector3 b = tilemap.transform.TransformPoint(local.max);

        Bounds world = new Bounds();
        world.SetMinMax(Vector3.Min(a, b), Vector3.Max(a, b));
        return world;
    }

    public static void Shake(float duration, float magnitude)
    {
        if (instance == null) return;
        if (magnitude >= instance.CurrentMagnitude())
        {
            instance.shakeDuration = duration;
            instance.shakeTimer = duration;
            instance.shakeMagnitude = magnitude;
        }
    }

    private float CurrentMagnitude()
    {
        if (shakeTimer <= 0f || shakeDuration <= 0f) return 0f;
        return shakeMagnitude * (shakeTimer / shakeDuration);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        smoothPos = Vector3.SmoothDamp(smoothPos, desired, ref velocity, smoothTime);

        // Clamp the follow position first, so shake is applied on top of it
        Vector3 basePos = ClampToMap(smoothPos);

        Vector3 shakeOffset = Vector3.zero;
        if (shakeTimer > 0f)
        {
            shakeOffset = (Vector3)(Random.insideUnitCircle * CurrentMagnitude());
            shakeTimer -= Time.deltaTime;

            // At an edge, flip any shake that would push past it back inward,
            // so the shake keeps its full strength
            Vector3 test = ClampToMap(basePos + shakeOffset);
            if (!Mathf.Approximately(test.x, basePos.x + shakeOffset.x)) shakeOffset.x = -shakeOffset.x;
            if (!Mathf.Approximately(test.y, basePos.y + shakeOffset.y)) shakeOffset.y = -shakeOffset.y;
        }

        Vector3 finalPos = basePos + shakeOffset;

        // Snap to real screen pixels (fine enough to look smooth, stops tile seams)
        float unitsPerScreenPixel = (cam.orthographicSize * 2f) / Screen.height;
        Vector3 snapped = new Vector3(
            Mathf.Round(finalPos.x / unitsPerScreenPixel) * unitsPerScreenPixel,
            Mathf.Round(finalPos.y / unitsPerScreenPixel) * unitsPerScreenPixel,
            smoothPos.z
        );

        // Final safety clamp so rounding never shows a sliver past the edge
        transform.position = ClampToMap(snapped);
    }

    private Vector3 ClampToMap(Vector3 pos)
    {
        if (!hasBounds) return pos;

        // Read every frame: Pixel Perfect Camera changes the size with the window
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        pos.x = ClampAxis(pos.x, mapBounds.min.x + halfW, mapBounds.max.x - halfW, mapBounds.center.x);
        pos.y = ClampAxis(pos.y, mapBounds.min.y + halfH, mapBounds.max.y - halfH, mapBounds.center.y);
        return pos;
    }

    private static float ClampAxis(float value, float min, float max, float center)
    {
        // Map narrower than the view on this axis: just center on it
        return min > max ? center : Mathf.Clamp(value, min, max);
    }
}
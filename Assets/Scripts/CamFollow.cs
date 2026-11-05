using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.08f;
    [SerializeField] private Vector2 offset = Vector2.zero;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    private Vector3 smoothPos;

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

        Vector3 shakeOffset = Vector3.zero;
        if (shakeTimer > 0f)
        {
            shakeOffset = (Vector3)(Random.insideUnitCircle * CurrentMagnitude());
            shakeTimer -= Time.deltaTime;
        }

        Vector3 finalPos = smoothPos + shakeOffset;

        // Snap to real screen pixels (fine enough to look smooth, stops tile seams)
        float unitsPerScreenPixel = (cam.orthographicSize * 2f) / Screen.height;
        transform.position = new Vector3(
            Mathf.Round(finalPos.x / unitsPerScreenPixel) * unitsPerScreenPixel,
            Mathf.Round(finalPos.y / unitsPerScreenPixel) * unitsPerScreenPixel,
            smoothPos.z
        );
    }
}
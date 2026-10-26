using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.08f;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private float pixelsPerUnit = 32f; // matches your Mars tiles

    private Vector3 velocity = Vector3.zero;
    private Vector3 smoothPos;

    private void Start()
    {
        smoothPos = transform.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        // Glide smoothly behind the scenes...
        smoothPos = Vector3.SmoothDamp(smoothPos, desired, ref velocity, smoothTime);

        // ...but always put the real camera on an exact pixel
        transform.position = new Vector3(
            Mathf.Round(smoothPos.x * pixelsPerUnit) / pixelsPerUnit,
            Mathf.Round(smoothPos.y * pixelsPerUnit) / pixelsPerUnit,
            smoothPos.z
        );
    }
}
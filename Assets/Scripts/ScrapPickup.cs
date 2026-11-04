using UnityEngine;

public class ScrapPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private float bobHeight = 0.1f; // little floating motion
    [SerializeField] private float bobSpeed = 3f;

    private Vector3 startPos;
    private bool collected = false;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        Spaceman player = other.GetComponentInParent<Spaceman>();
        if (player == null) return; // ignore bullets, enemies, etc.

        collected = true;
        player.AddScrap(value);
        Destroy(gameObject);
    }
}
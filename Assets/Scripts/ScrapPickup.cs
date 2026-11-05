using UnityEngine;

public class ScrapPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;

    [Header("Bob")]
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float bobSpeed = 3f;

    [Header("Magnet")]
    [SerializeField] private float magnetRange = 2.5f;     // starts pulling inside this distance
    [SerializeField] private float startSpeed = 2f;        // speed when the pull begins
    [SerializeField] private float pullAcceleration = 25f; // how fast it speeds up
    [SerializeField] private float spawnDelay = 0.3f;      // let it land before it can be pulled

    private Vector3 startPos;
    private Transform player;
    private Collider2D playerCol;
    private Spaceman spaceman;
    private bool magnetized = false;
    private bool collected = false;
    private float currentSpeed;
    private float age = 0f;

    private void Start()
    {
        startPos = transform.position;
        spaceman = FindFirstObjectByType<Spaceman>();
        if (spaceman != null)
        {
            player = spaceman.transform;
            playerCol = spaceman.GetComponent<Collider2D>();
        }
    }

    private void Update()
    {
        age += Time.deltaTime;

        if (player == null)
        {
            Bob();
            return;
        }

        // Aim at the middle of the player's hitbox, not his feet
        Vector3 target = playerCol != null ? playerCol.bounds.center : player.position;

        if (!magnetized && age >= spawnDelay &&
            Vector2.Distance(transform.position, target) <= magnetRange)
        {
            magnetized = true;
            currentSpeed = startSpeed;
        }

        if (magnetized)
        {
            currentSpeed += pullAcceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, currentSpeed * Time.deltaTime);

            // Safety net in case it reaches the center without a trigger firing
            if (Vector2.Distance(transform.position, target) < 0.05f) Collect();
        }
        else
        {
            Bob();
        }
    }

    private void Bob()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Spaceman>() != null) Collect();
    }

    private void Collect()
    {
        if (collected) return;
        collected = true;
        if (spaceman != null) spaceman.AddScrap(value);
        Destroy(gameObject);
    }

    // Shows the magnet range in the Scene view when selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}
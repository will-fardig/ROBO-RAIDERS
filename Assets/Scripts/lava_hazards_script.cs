using UnityEngine;
using UnityEngine.Tilemaps;

public class lava_hazards_script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    void Update()
    {
        
    }
    private Tilemap hazardTilemap;
    [SerializeField] private Spaceman _spaceman;

    [Header("Damage Settings")]
    public int damageAmount = 1;
    public float damageCooldown = 1.0f;
    private float nextDamageTime = 0f;

    void Start()
    {
        // Automatically grab the Tilemap component attached to this GameObject
        hazardTilemap = GetComponent<Tilemap>();
        _spaceman = FindAnyObjectByType<Spaceman>();

        if (hazardTilemap == null)
        {
            Debug.LogError("TilemapHazard script requires a Tilemap component on the same GameObject!");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Check if the colliding object is your player
        if (collision.CompareTag("Player") || collision.gameObject.name.Contains("Spaceman_1"))
        {
            // Optional: If you want to check if they are standing on a specific tile coordinate
            Vector3 contactPoint = collision.transform.position; // Approximate contact point
            Vector3Int cellPosition = hazardTilemap.WorldToCell(contactPoint);

            // Check if there is actually a tile at this grid position
            if (hazardTilemap.HasTile(cellPosition))
            {
                if (Time.time >= nextDamageTime)
                {
                    ApplyHazardDamage(collision.gameObject, contactPoint);
                    nextDamageTime = Time.time + damageCooldown;
                }
            }
        }
    }

    void ApplyHazardDamage(GameObject player, Vector3 contactPoint)
    {
        // Option A: If your player script has a public method to take damage
        // PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        // if (playerHealth != null) { playerHealth.TakeDamage(damageAmount); }

        // Option B: Debug log to verify it's working
        _spaceman.TakeDamage(damageAmount, contactPoint);
        Debug.Log($"Player stepped on a hazard tile at coordinate! Took {damageAmount} damage.");
    }
}

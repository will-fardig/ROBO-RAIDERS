using System.Collections;
using UnityEngine;

// Health, getting hit, and dying. EVERY enemy type gets this component.
// Bullets and the WaveManager only talk to this, never to movement scripts.
public class EnemyHealth : MonoBehaviour
{
    // fires when any enemy dies (WaveManager counts these)
    public static event System.Action<GameObject> Died;

    [Header("Health")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float currentHealth;   // visible for debugging

    [Header("Hit Feedback")]
    [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float knockbackSpeed = 6f;   // 0 = can't be pushed (big enemies)
    [SerializeField] private float stunDuration = 0.15f;  // movement scripts pause while stunned
    [SerializeField] private float knockbackDrag = 40f;

    [Header("Drops")]
    [SerializeField] private GameObject scrapPrefab;
    [SerializeField] private int scrapDropCount = 5;
    [SerializeField] private float scatterRadius = 0.5f;

    public float Health => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => dead;
    public bool IsStunned => stunTimer > 0f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;
    private Coroutine flashRoutine;
    private float stunTimer;
    private bool dead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        // Knocked back: slide to a stop. Movement scripts skip their turn while IsStunned.
        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (rb != null)
                rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, knockbackDrag * Time.fixedDeltaTime);
        }
    }

    // Called by bullets (or anything else that hurts enemies). Returns true if this hit killed it.
    public bool TakeHit(float damage, Vector2 hitDirection)
    {
        if (dead) return false;

        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            Die();
            return true;
        }

        if (rb != null && knockbackSpeed > 0f)
        {
            rb.linearVelocity = hitDirection.normalized * knockbackSpeed;
            stunTimer = stunDuration;
        }

        if (sr != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(Flash());
        }
        return false;
    }

    private IEnumerator Flash()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
    }

    public void Die()
    {
        if (dead) return;   // two bullets on the same frame can't kill it twice
        dead = true;
        Died?.Invoke(gameObject);

        CamFollow.Shake(0.12f, 0.1f);
        if (scrapPrefab != null)
        {
            Vector2 center = sr != null ? (Vector2)sr.bounds.center : (Vector2)transform.position;
            for (int i = 0; i < scrapDropCount; i++)
            {
                Vector2 offset = (i == 0) ? Vector2.zero : Random.insideUnitCircle * scatterRadius;
                Instantiate(scrapPrefab, center + offset, Quaternion.identity);
            }
        }
        Destroy(gameObject);
    }
}
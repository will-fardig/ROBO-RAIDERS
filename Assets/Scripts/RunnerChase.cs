using System.Collections;
using UnityEngine;

public class RunnerChase : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float pauseAfterHit = 0.6f;  // backs off after hitting the player

    [Header("Sprite")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Health")]
    [SerializeField] public float RunnerHealth = 20;

    [Header("Hit Feedback")]
    [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f); // red tint
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float knockbackSpeed = 6f;    // how hard bullets push it
    [SerializeField] private float stunDuration = 0.15f;   // can't steer while stunned
    [SerializeField] private float knockbackDrag = 40f;    // how fast the push slows down

    [Header("Drops")]
    [SerializeField] private GameObject scrapPrefab;
    [SerializeField] private int scrapDropCount = 1;
    [SerializeField] private float scatterRadius = 0.5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private float pauseTimer = 0f;
    private float stunTimer = 0f;
    private Color originalColor;
    private Coroutine flashRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        originalColor = sr.color;
    }

    private void Start()
    {
        Spaceman spaceman = FindFirstObjectByType<Spaceman>();
        if (spaceman != null) player = spaceman.transform;
    }

    private void FixedUpdate()
    {
        // Knocked back: slide to a stop, no chasing, no turning around
        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, knockbackDrag * Time.fixedDeltaTime);
            return;
        }

        if (player == null) return;

        Vector2 targetVelocity = Vector2.zero;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;
        }
        else
        {
            Vector2 toPlayer = player.position - transform.position;
            if (toPlayer.magnitude < detectionRange)
                targetVelocity = toPlayer.normalized * moveSpeed;
        }

        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        if (rb.linearVelocity.x > 0.1f)
            sr.flipX = !spriteFacesRight;
        else if (rb.linearVelocity.x < -0.1f)
            sr.flipX = spriteFacesRight;
    }

    // Called by bullets. Returns true if this hit killed it.
    public bool TakeHit(float damage, Vector2 hitDirection)
    {
        RunnerHealth -= damage;

        if (RunnerHealth <= 0)
        {
            Die();
            return true;
        }

        // Knockback + stun
        rb.linearVelocity = hitDirection.normalized * knockbackSpeed;
        stunTimer = stunDuration;

        // Flash
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());

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
        CamFollow.Shake(0.12f, 0.1f);
        if (scrapPrefab != null)
        {
            for (int i = 0; i < scrapDropCount; i++)
            {
                Vector2 offset = (i == 0) ? Vector2.zero : Random.insideUnitCircle * scatterRadius;
                Instantiate(scrapPrefab, (Vector2)sr.bounds.center + offset, Quaternion.identity);
            }
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Spaceman>() != null)
            pauseTimer = pauseAfterHit;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
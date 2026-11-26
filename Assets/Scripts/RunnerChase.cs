using UnityEngine;

// Runner movement: charge at the player along the flow field.
// Health/hits/death live in EnemyHealth.
public class RunnerChase : MonoBehaviour, IWaveScalable
{
    [Header("Waves")]
    [SerializeField] private bool huntPlayer = false;
    [SerializeField] private float directChaseRange = 1.5f;
    [SerializeField] private string flowFieldId = "default";   // "big" for large enemies

    [Header("Chase Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float pauseAfterHit = 0.6f;

    [Header("Sprite")]
    [SerializeField] private bool spriteFacesRight = true;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private EnemyHealth health;
    private Transform player;
    private float pauseTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        health = GetComponent<EnemyHealth>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        Spaceman spaceman = FindFirstObjectByType<Spaceman>();
        if (spaceman != null) player = spaceman.transform;
    }

    public void ApplyWaveStats(EnemyWaveStats stats)
    {
        huntPlayer = stats.huntPlayer;
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsStunned) return;   // EnemyHealth is sliding us back
        if (player == null) return;

        Vector2 targetVelocity = Vector2.zero;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;
        }
        else
        {
            Vector2 toPlayer = (Vector2)player.position - Center;
            if (huntPlayer || toPlayer.magnitude < detectionRange)
                targetVelocity = GetChaseDirection(toPlayer) * moveSpeed;
        }

        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        if (rb.linearVelocity.x > 0.1f)
            sr.flipX = !spriteFacesRight;
        else if (rb.linearVelocity.x < -0.1f)
            sr.flipX = spriteFacesRight;
    }

    private Vector2 Center => col != null ? (Vector2)col.bounds.center : rb.position;

    private Vector2 GetChaseDirection(Vector2 toPlayer)
    {
        FlowField field = FlowField.Get(flowFieldId);
        if (toPlayer.magnitude < directChaseRange || field == null)
            return toPlayer.normalized;

        Vector2 dir = field.GetDirection(Center);
        return dir != Vector2.zero ? dir : toPlayer.normalized;
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
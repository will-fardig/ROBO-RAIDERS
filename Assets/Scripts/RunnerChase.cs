using UnityEngine;

public class RunnerChase : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float moveSpeed = 3f;        // a bit slower than the player (7)
    [SerializeField] private float acceleration = 20f;    // lower = more lumbering
    [SerializeField] private float detectionRange = 8f;   // starts chasing inside this distance
    [SerializeField] private float pauseAfterHit = 0.6f;  // backs off briefly after hitting you

    [Header("Sprite")]
    [SerializeField] private bool spriteFacesRight = true; // untick if the art faces left

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private float pauseTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        Spaceman spaceman = FindFirstObjectByType<Spaceman>();
        if (spaceman != null) player = spaceman.transform;
    }

    private void FixedUpdate()
    {
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
            {
                targetVelocity = toPlayer.normalized * moveSpeed;
            }
        }

        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        // Face the direction we're moving
        if (rb.linearVelocity.x > 0.1f)
            sr.flipX = !spriteFacesRight;
        else if (rb.linearVelocity.x < -0.1f)
            sr.flipX = spriteFacesRight;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Spaceman>() != null)
        {
            pauseTimer = pauseAfterHit; // don't sit on top of the player draining HP
        }
    }

    // Shows the detection range in the Scene view when selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
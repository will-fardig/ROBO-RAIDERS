using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Spaceman: MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 300f; // Velocity multiplier for top-down movement
    private Vector2 moveInput;

    [Header("Health & Survival")]
    [SerializeField] private int maxLives = 25;
    [SerializeField] private int lives = 20;
    public int Lives { get => lives; private set => lives = value; }
    public int MaxLives => maxLives;

    public int ItemsCount { get; set; } = 0;

    [Header("Hurt & Knockback Visuals")]
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private float knockbackForce = 6.0f;
    private Coroutine hitCoroutine;

    //[Header("References")]
    //[SerializeField] private RandomEvents _randomevents;
    //public RespawnSystem _respawnSystem;

    // Tags
    private const string EnemyTag = "Enemy";
    private const string AppleTag = "Apple";

    private void Awake()
    {
        Application.targetFrameRate = -1;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Ensure 2D Top-Down physics settings
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        // Enforce input action map assignment if configured
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.defaultActionMap != null)
        {
            playerInput.SwitchCurrentActionMap(playerInput.defaultActionMap);
        }
    }

    private void FixedUpdate()
    {
        // 1. Move character in all directions (X & Y)
        rb.linearVelocity = moveInput * moveSpeed * Time.fixedDeltaTime;

        // 2. Sprite flipping (Visual direction based on horizontal movement)
        if (moveInput.x > 0.01f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput.x < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // 3. Drive Walking/Moving animation state
        if (anim != null)
        {
            anim.SetBool("walk", moveInput.sqrMagnitude > 0.01f);
        }
    }

    // --- INPUT SYSTEM CALLBACK ---
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // --- DAMAGE & TRIGGER INTERACTIONS ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Pickup: Heal item (Apple)
        /*
        if (collision.CompareTag(AppleTag))
        {
            ItemsCount++;
            if (_randomevents != null) _randomevents.PowerUps_Count--;
            Heal(5);
            Destroy(collision.gameObject);
            Debug.Log($"Apple collected! Lives: {Lives} | Apples: {ItemsCount}");
        }
        */
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Enemy collision damage
        if (collision.gameObject.CompareTag(EnemyTag))
        {
            TakeDamage(1, collision.transform.position);
        }
    }

    public void TakeDamage(int damageAmount, Vector3 attackerPosition)
    {
        lives -= damageAmount;
        Debug.Log($"Player took damage! Remaining Lives: {lives}");

        if (hitCoroutine != null) StopCoroutine(hitCoroutine);
        hitCoroutine = StartCoroutine(HurtSequenceRoutine(attackerPosition));

        if (lives <= 0)
        {
            GameOver();
        }
    }

    private IEnumerator HurtSequenceRoutine(Vector3 attackerPosition)
    {
        // Calculate knockback vector away from attacker
        Vector2 knockbackDir = (transform.position - attackerPosition).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        if (anim != null)
        {
            anim.SetTrigger("hurt");
        }

        yield return new WaitForSeconds(flashDuration);

        if (anim != null)
        {
            anim.ResetTrigger("hurt");
        }
    }

    // --- UTILITIES & GAME STATE ---
    public void Heal(int amount)
    {
        lives = Mathf.Min(lives + amount, maxLives);
        Debug.Log($"Healed! Lives: {lives}/{maxLives}");
    }

    public void Respawn(Vector3 targetPosition)
    {
        transform.position = targetPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Restarting run...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

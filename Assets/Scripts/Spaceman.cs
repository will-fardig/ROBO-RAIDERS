using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Spaceman: MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 7f;        // units per second
    [SerializeField] private float acceleration = 80f;   // how fast you reach top speed
    [SerializeField] private float deceleration = 100f;  // how fast you stop when you let go
    [SerializeField] private float turnBoost = 1.5f;     // extra grip when reversing direction
    private Vector2 moveInput;
    private float knockbackTimer = 0f;
    [SerializeField] private float knockbackDuration = 0.15f;

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

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject specialBulletPrefab;
    [SerializeField] private Transform firePoint; // Assign an empty child GameObject positioned at weapon barrel

    [Header("Points")]
    public int points = 500;
    //weapon stats
    [Header("Primary Weapon Stats")]
    [SerializeField] public int prime_wpn_id = 0;
    [SerializeField] public int prime_wpn_mag_max = 8;
    [SerializeField] public int prime_wpn_mag = 8;
    [SerializeField] public int prime_wpn_reserve_max = 80;
    [SerializeField] public int prime_wpn_reserve = 32;
    [SerializeField] public int prime_wpn_damage = 4;
    [Header("Weapon Stats")]
    //weapon 1
    [SerializeField] public int wpn1_id = 0;
    [SerializeField] public int wpn1_mag_max = 8;
    [SerializeField] public int wpn1_mag = 8;
    [SerializeField] public int wpn1_reserve_max = 80;
    [SerializeField] public int wpn1_reserve = 32;
    [SerializeField] public int wpn1_damage = 4;
    //weapon 2 - no weapon 2 yet
    [SerializeField] public int wpn2_id = 0;
    [SerializeField] public int wpn2_mag_max = 0;
    [SerializeField] public int wpn2_mag = 0;
    [SerializeField] public int wpn2_reserve_max = 0;
    [SerializeField] public int wpn2_reserve = 0;
    [SerializeField] public int wpn2_damage = 0;
    //weapon 3 - no weapon 3 yet
    [SerializeField] public int wpn3_id = 0;
    [SerializeField] public int wpn3_mag_max = 0;
    [SerializeField] public int wpn3_mag = 0;
    [SerializeField] public int wpn3_reserve_max = 0;
    [SerializeField] public int wpn3_reserve = 0;
    [SerializeField] public int wpn3_damage = 0;
    //PACKED BLAST
    private bool _thunderblast = false;
    // Direction Tracking
    private bool faceRight = true;
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
        // 1. Snappy movement with acceleration/deceleration
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime; // let knockback play out
        }
        else
        {
            Vector2 targetVelocity = moveInput.normalized * maxSpeed;
            Vector2 current = rb.linearVelocity;

            float rate;
            if (moveInput.sqrMagnitude < 0.01f)
                rate = deceleration;
            else if (Vector2.Dot(current, targetVelocity) < 0f)
                rate = acceleration * turnBoost; // reversing direction
            else
                rate = acceleration;

            rb.linearVelocity = Vector2.MoveTowards(current, targetVelocity, rate * Time.fixedDeltaTime);
        }

        // 2. Sprite flipping (keeps whatever size you set in the Inspector)
        Vector3 scale = transform.localScale;
        if (moveInput.x > 0.01f)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else if (moveInput.x < -0.01f)
        {
            scale.x = -Mathf.Abs(scale.x);
        }
        transform.localScale = scale;

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
        //UPDATE FACERIGHT
        if (moveInput.x > 0)
        {
            //FACING RIGHT
            faceRight = true;
        }
        else if (moveInput.x < 0)
        {
            //FACING LEFT
            faceRight = false;
        }
    }
    //SHOOTING MECHANICS
    //SHOOT BULLET
    public void OnShoot(InputValue value)
    {
        if (value.isPressed)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        if (_thunderblast == false)
        {

            // Point right (0 deg) if facing right, point left (180 deg) if facing left
            Quaternion bulletRotation = faceRight ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

            Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        }
        else
        {
            _thunderblast = false;
            Quaternion bulletRotation = faceRight ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

            Instantiate(specialBulletPrefab, firePoint.position, bulletRotation);

        }
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
        knockbackTimer = knockbackDuration;
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

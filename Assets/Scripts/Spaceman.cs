using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

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
    [Header("Aiming")]
    [SerializeField] private float muzzleDistance = 0.6f; // how far from the body bullets appear
    [SerializeField] private float muzzleHeight = 0f;     // raise this if the gun is above his center

    [Header("Map Bounds")]
    [Tooltip("The ground tilemap. The player can't walk past its painted edges.")]
    [SerializeField] private Tilemap boundsTilemap;
    private Bounds mapBounds;
    private bool hasBounds;
    private Collider2D bodyCollider;

    [Header("Points")]
    public int points = 500;
    [Header("Scrap")]
    [SerializeField] private int scrap = 0;
    [SerializeField] private ScrapCounterUI scrapUI;
    public int Scrap => scrap;
    //weapon stats
    [Header("Primary Weapon Stats")]
    [SerializeField] public int prime_wpn_id = 0;
    [SerializeField] public int prime_wpn_mag_max = 8;
    [SerializeField]public int prime_wpn_mag = 8;
    [SerializeField]public int prime_wpn_reserve_max = 80;
    [SerializeField]public int prime_wpn_reserve = 32;
    [SerializeField] public int prime_wpn_damage = 4;
    [SerializeField] private float prime_wpn_reloadTime = 2.0f;
    [Header("Weapon Stats")]
    //weapon 1
    [SerializeField] public int wpn1_id = 0;
    [SerializeField] public int wpn1_mag_max = 8;
    [SerializeField] public int wpn1_mag = 8;
    [SerializeField] public int wpn1_reserve_max = 80;
    [SerializeField] public int wpn1_reserve = 32;
    [SerializeField] public int wpn1_damage = 4;
    [SerializeField] private float wp1_reloadTime = 0f;
    //weapon 2 - no weapon 2 yet
    [SerializeField] public int wpn2_id = 0;
    [SerializeField] public int wpn2_mag_max = 0;
    [SerializeField] public int wpn2_mag = 0;
    [SerializeField] public int wpn2_reserve_max = 0;
    [SerializeField] public int wpn2_reserve = 0;
    [SerializeField] public int wpn2_damage = 0;
    [SerializeField] private float wp2_reloadTime = 0f;
    //weapon 3 - no weapon 3 yet
    [SerializeField] public int wpn3_id = 0;
    [SerializeField] public int wpn3_mag_max = 0;
    [SerializeField] public int wpn3_mag = 0;
    [SerializeField] public int wpn3_reserve_max = 0;
    [SerializeField] public int wpn3_reserve = 0;
    [SerializeField] public int wpn3_damage = 0;
    [SerializeField] private float wp3_reloadTime = 0f;
    //PACKED BLAST
    private bool _thunderblast = false;
    // Direction Tracking
    private bool faceRight = true;
    //[Header("References")]
    //[SerializeField] private RandomEvents _randomevents;
    //public RespawnSystem _respawnSystem;
    [Header("Reload Settings")]
    public bool IsReloading = false;

    [Header("Auto Mode")]
    [Tooltip("On: the player runs from enemies and shoots by itself. Off: normal mouse/keyboard control.")]
    public bool autoMode = false;
    [Tooltip("Runs away from enemies closer than this.")]
    [SerializeField] private float fleeRange = 4f;
    [Tooltip("Shoots at the closest enemy within this distance.")]
    [SerializeField] private float shootRange = 8f;
    [SerializeField] private float shotsPerSecond = 4f;
    [Tooltip("Steers away from the map edges so it doesn't get stuck in corners.")]
    [SerializeField] private float edgeMargin = 1.5f;
    [SerializeField] private float scanInterval = 0.2f;

    private GameObject[] autoEnemies = new GameObject[0];
    private Transform autoTarget;
    private Vector2 autoAimDirection = Vector2.right;
    private float nextScanTime = 0f;
    private float nextShotTime = 0f;
    private bool wasAutoMode = false;

    // Tags
    private const string EnemyTag = "Enemy";
    private const string AppleTag = "Apple";

    private void Awake()
    {
        Application.targetFrameRate = -1;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider2D>();

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
        if (boundsTilemap != null)
        {
            mapBounds = CamFollow.GetTilemapWorldBounds(boundsTilemap);
            hasBounds = true;
        }
    }

    // --- MAP BOUNDS ---
    private void KeepInsideMap()
    {
        if (!hasBounds) return;

        // How far the hitbox reaches from the pivot on each side
        Vector2 pivot = transform.position;
        Vector2 reachMin = Vector2.zero, reachMax = Vector2.zero;
        if (bodyCollider != null)
        {
            reachMin = (Vector2)bodyCollider.bounds.min - pivot;
            reachMax = (Vector2)bodyCollider.bounds.max - pivot;
        }

        float minX = mapBounds.min.x - reachMin.x;
        float maxX = mapBounds.max.x - reachMax.x;
        float minY = mapBounds.min.y - reachMin.y;
        float maxY = mapBounds.max.y - reachMax.y;

        Vector2 pos = rb.position;

        // Already outside (spawned there, shoved by an enemy): snap back in
        Vector2 clamped = new Vector2(Mathf.Clamp(pos.x, minX, maxX), Mathf.Clamp(pos.y, minY, maxY));
        if (clamped != pos)
        {
            rb.position = clamped;
            pos = clamped;
        }

        // Cap velocity so this physics step stops exactly at the edge (covers knockback too)
        float dt = Time.fixedDeltaTime;
        Vector2 vel = rb.linearVelocity;
        vel.x = Mathf.Clamp(vel.x, (minX - pos.x) / dt, (maxX - pos.x) / dt);
        vel.y = Mathf.Clamp(vel.y, (minY - pos.y) / dt, (maxY - pos.y) / dt);
        rb.linearVelocity = vel;
    }

    private void Update()
    {
        if (!autoMode)
        {
            // Just switched back to manual: stop the leftover auto movement
            if (wasAutoMode) moveInput = Vector2.zero;
            wasAutoMode = false;
            return;
        }
        wasAutoMode = true;

        if (Time.time >= nextScanTime)
        {
            autoEnemies = GameObject.FindGameObjectsWithTag(EnemyTag);
            nextScanTime = Time.time + scanInterval;
        }

        autoTarget = FindNearestEnemy(shootRange);
        moveInput = GetFleeDirection();

        // Aim at the target, or where we're walking if there's nothing to shoot
        if (autoTarget != null)
        {
            Vector2 toTarget = EnemyCenter(autoTarget) - GetAimOrigin();
            if (toTarget.sqrMagnitude > 0.0001f) autoAimDirection = toTarget.normalized;
        }
        else if (moveInput.sqrMagnitude > 0.01f)
        {
            autoAimDirection = moveInput.normalized;
        }

        AutoShoot();
    }

    // --- AUTO MODE HELPERS ---
    private Transform FindNearestEnemy(float range)
    {
        Transform nearest = null;
        float nearestDist = range;
        Vector2 me = transform.position;

        foreach (GameObject enemy in autoEnemies)
        {
            if (enemy == null) continue; // destroyed since the last scan
            float d = Vector2.Distance(me, EnemyCenter(enemy.transform));
            if (d < nearestDist)
            {
                nearestDist = d;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    private Vector2 GetFleeDirection()
    {
        Vector2 me = transform.position;
        Vector2 flee = Vector2.zero;

        // Push away from every enemy inside flee range, harder the closer it is
        foreach (GameObject enemy in autoEnemies)
        {
            if (enemy == null) continue;
            Vector2 away = me - EnemyCenter(enemy.transform);
            float d = away.magnitude;
            if (d < 0.01f || d > fleeRange) continue;
            flee += (away / d) * (1f - d / fleeRange);
        }

        if (flee.sqrMagnitude < 0.0001f) return Vector2.zero; // nothing close: stand and shoot

        // Push away from map edges so it slides along walls instead of getting pinned
        if (hasBounds)
        {
            if (me.x - mapBounds.min.x < edgeMargin) flee.x += 1f;
            if (mapBounds.max.x - me.x < edgeMargin) flee.x -= 1f;
            if (me.y - mapBounds.min.y < edgeMargin) flee.y += 1f;
            if (mapBounds.max.y - me.y < edgeMargin) flee.y -= 1f;
        }

        return flee.sqrMagnitude < 0.01f ? Vector2.zero : flee.normalized;
    }

    private void AutoShoot()
    {
        if (IsReloading) return;

        if (autoTarget == null)
        {
            // Nothing to shoot: top off the magazine
            if (prime_wpn_mag < prime_wpn_mag_max && prime_wpn_reserve > 0) StartReload();
            return;
        }

        if (Time.time < nextShotTime) return;

        if (prime_wpn_mag > 0)
        {
            Shoot();
            nextShotTime = Time.time + 1f / Mathf.Max(0.1f, shotsPerSecond);
        }
        else if (prime_wpn_reserve > 0)
        {
            StartReload();
        }
    }

    private Vector2 EnemyCenter(Transform enemy)
    {
        Collider2D col = enemy.GetComponent<Collider2D>();
        return col != null ? (Vector2)col.bounds.center : (Vector2)enemy.position;
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
        KeepInsideMap();

        // 2. Face the mouse (top-down shooter style)
        faceRight = GetAimDirection().x >= 0f;
        Vector3 scale = transform.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
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
        if (autoMode) return;
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
    // --- AIMING ---
    private Vector2 GetAimOrigin()
    {
        return (Vector2)transform.position + Vector2.up * muzzleHeight;
    }

    private Vector2 GetAimDirection()
    {
        if (autoMode) return autoAimDirection;

        if (Mouse.current == null || Camera.main == null)
            return faceRight ? Vector2.right : Vector2.left;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        Vector2 dir = mouseWorld - GetAimOrigin();

        // Mouse right on top of the player: keep shooting the way we face
        if (dir.sqrMagnitude < 0.0001f)
            return faceRight ? Vector2.right : Vector2.left;

        return dir.normalized;
    }
    //SHOOTING MECHANICS
    //SHOOT BULLET
    public void OnShoot(InputValue value)
    {
        if (autoMode) return;
        if (value.isPressed)
        {
            // Don't shoot while reloading
            if (IsReloading==true) return;
            if (prime_wpn_mag > 0)
            {
                Shoot();
            }
            else if(prime_wpn_mag<=0 && prime_wpn_reserve>0){
                StartReload(); //reload time

            }
            else
            {
                Debug.Log("No Ammo");
            }
        }
    }
    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector2 dir = GetAimDirection();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion bulletRotation = Quaternion.Euler(0f, 0f, angle);
        Vector2 spawnPos = GetAimOrigin() + dir * muzzleDistance;

        if (_thunderblast == false)
        {
            prime_wpn_mag -= 1;
            Instantiate(bulletPrefab, spawnPos, bulletRotation);
        }
        else
        {
            _thunderblast = false;
            Instantiate(specialBulletPrefab, spawnPos, bulletRotation);
        }
    }
    //WEAPON RELOADS
    private void OnReload(InputValue value)
    {
        if (autoMode) return;
        if (value.isPressed)
        {
            if (prime_wpn_mag < prime_wpn_mag_max && prime_wpn_reserve > 0)
            {
                StartReload(); //reload time

            }
            else
            {
                Debug.Log("No Ammo");
            }
        }
    }
    private void StartReload()
    {
        if (IsReloading==true) return;

        // Check if magazine is already full
        if (prime_wpn_mag >= prime_wpn_mag_max) return;

        // Check if reserve ammo is empty
        if (prime_wpn_reserve <= 0) return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        IsReloading = true;

        Debug.Log("Reloading...");

        // Wait for reload duration
        yield return new WaitForSeconds(prime_wpn_reloadTime);

        // Calculate how much ammo is needed
        int ammoNeeded = prime_wpn_mag_max - prime_wpn_mag;

        // Calculate how much ammo can be transferred
        int ammoToLoad = Mathf.Min(ammoNeeded, prime_wpn_reserve);

        // Transfer ammunition
        prime_wpn_mag += ammoToLoad;
        prime_wpn_reserve -= ammoToLoad;

        IsReloading = false;

        Debug.Log($"Reload complete! Magazine: {prime_wpn_mag} | Reserve: {prime_wpn_reserve}");
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
        CamFollow.Shake(0.2f, 0.2f);
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
    public void AddScrap(int amount)
    {
        scrap += amount;
        if (scrapUI != null) scrapUI.UpdateScrap(scrap);
        Debug.Log($"Scrap collected! Total: {scrap}");
    }

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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Spaceman : MonoBehaviour
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
    [SerializeField] public int maxLives = 25;
    [SerializeField] public int lives = 20;
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

    [Header("Scrap-Points")]
    [SerializeField] public int scrap = 500;
    [SerializeField] private ScrapCounterUI scrapUI;
    [SerializeField] public int point_multiplier = 1;

    //PERK-A-COLAS
    /*Double Trouble Tonic = Double tap 
    Power pop = Juggernog
    Comeback Cola = Quick Revive
    Fizzy Fastness = Speed Cola*/

    [Header("Perks")]
    [SerializeField] public int perk_count = 0;
    [SerializeField] public int max_perk_count = 4;
    [SerializeField] public Dictionary<int, string> PERKS_ORDER = new Dictionary<int, string>();
    public int Scrap => scrap;
    //weapon stats
    [Header("Primary Weapon Stats")]
    //weapons order 
    [SerializeField] public ArrayList weapon_ids = new ArrayList();
    [SerializeField] public Dictionary<int, bool> WPNS_ORDER = new Dictionary<int, bool>();
    //wpn_id
    // Adding elements
    [SerializeField] public int prime_wpn_id = 0;
    [SerializeField] public int prime_wpn_mag_max = 8;
    [SerializeField] public int prime_wpn_mag = 8;
    [SerializeField] public int prime_wpn_reserve_max = 80;
    [SerializeField] public int prime_wpn_reserve = 32;
    [SerializeField] public int prime_wpn_damage = 4;
    [SerializeField] public float prime_wpn_reloadTime = 2.0f;
    [SerializeField] public int wpn_damage_multiplier = 1;
    [SerializeField] public float prime_fireRate = 0.1f;       // Time in seconds between shots (lower = faster)
    [SerializeField] public float nextFireTime = 0f;
    [SerializeField] public bool wpn_prime_upgraded = false;
    [SerializeField] public bool prime_wpn_automatic = false;   // hold to keep firing
    [SerializeField] public float prime_bulletSpeed = 8f;       // units per second
    [SerializeField] public float prime_bulletSize = 1f;        // 1 = bullet prefab's normal size
    [SerializeField] public float prime_spread = 0f;            // total cone in degrees (0 = laser accurate)
    [SerializeField] public int prime_pellets = 1;              // bullets per shot (shotguns > 1)
    [SerializeField] public int prime_pierce = 0;// extra enemies each bullet passes through
    [Header("Weapon Stats")]
    //weapon 1
    [SerializeField] public int wpn1_id = 0;
    [SerializeField] public int wpn1_mag_max = 8;
    [SerializeField] public int wpn1_mag = 8;
    [SerializeField] public int wpn1_reserve_max = 80;
    [SerializeField] public int wpn1_reserve = 32;
    [SerializeField] public int wpn1_damage = 4;
    [SerializeField] public float wp1_reloadTime = 2.0f;
    [SerializeField] public float wp1_fireRate = 0.1f;       // Time in seconds between shots (lower = faster)
    [SerializeField] public bool wp1_upgraded = false;
    [SerializeField] public bool wp1_automatic = false;
    [SerializeField] public float wp1_bulletSpeed = 8f;
    [SerializeField] public float wp1_bulletSize = 1f;
    [SerializeField] public float wp1_spread = 0f;
    [SerializeField] public int wp1_pellets = 1;
    [SerializeField] public int wp1_pierce = 0;
    //weapon 2 - no weapon 2 yet
    [SerializeField] public int wpn2_id = 0;
    [SerializeField] public int wpn2_mag_max = 0;
    [SerializeField] public int wpn2_mag = 0;
    [SerializeField] public int wpn2_reserve_max = 0;
    [SerializeField] public int wpn2_reserve = 0;
    [SerializeField] public int wpn2_damage = 0;
    [SerializeField] public float wp2_reloadTime = 0f;
    [SerializeField] public float wp2_fireRate = 0.0f;       // Time in seconds between shots (lower = faster)
    [SerializeField] public bool wp2_upgraded = false;
    [SerializeField] public bool wp2_automatic = false;
    [SerializeField] public float wp2_bulletSpeed = 8f;
    [SerializeField] public float wp2_bulletSize = 1f;
    [SerializeField] public float wp2_spread = 0f;
    [SerializeField] public int wp2_pellets = 1;
    [SerializeField] public int wp2_pierce = 0;
    //weapon 3 - no weapon 3 yet
    [SerializeField] public int wpn3_id = 0;
    [SerializeField] public int wpn3_mag_max = 0;
    [SerializeField] public int wpn3_mag = 0;
    [SerializeField] public int wpn3_reserve_max = 0;
    [SerializeField] public int wpn3_reserve = 0;
    [SerializeField] public int wpn3_damage = 0;
    [SerializeField] public float wp3_reloadTime = 0f;
    [SerializeField] public float wp3_fireRate = 0.0f;       // Time in seconds between shots (lower = faster)
    [SerializeField] public bool wp3_upgraded = false;
    [SerializeField] public bool wp3_automatic = false;
    [SerializeField] public float wp3_bulletSpeed = 8f;
    [SerializeField] public float wp3_bulletSize = 1f;
    [SerializeField] public float wp3_spread = 0f;
    [SerializeField] public int wp3_pellets = 1;
    [SerializeField] public int wp3_pierce = 0;
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
    [Header("Weapon Inventory Limits")]
    [SerializeField] public int base_max_weapons = 2;
    [SerializeField] public int mule_kick_max_weapons = 3;
    private bool hasMuleKick = false;

    private GameObject[] autoEnemies = new GameObject[0];
    private Transform autoTarget;
    private Vector2 autoAimDirection = Vector2.right;
    private float nextScanTime = 0f;
    private float nextShotTime = 0f;
    private bool wasAutoMode = false;
    private InputAction shootAction;   // checked every frame so automatic weapons fire while held

    // Tags
    private const string EnemyTag = "Enemy";
    private const string AppleTag = "Apple";
    // Store a reference to the PowerPop currently in range
    private soda_script currentSoda;
    //Check if the player is in range of a wall buy LOCATION
    private wallbuy_script currentWallBuy;
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
        if (playerInput != null) shootAction = playerInput.actions["Shoot"];
        if (boundsTilemap != null)
        {
            mapBounds = CamFollow.GetTilemapWorldBounds(boundsTilemap);
            hasBounds = true;
        }
        scrapUI?.UpdateScrap(scrap);
        InitializeWeapons();
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

            // Automatic weapons keep firing while the button is held
            if (prime_wpn_automatic && shootAction != null && shootAction.IsPressed()) TryFire(false);
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
        if (Time.time >= nextFireTime)
        {
            AutoShoot();
            nextFireTime = Time.time + Mathf.Max(0.02f, prime_fireRate);
        }
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
        if (value.isPressed) TryFire(true);
    }

    // All manual shooting goes through here so fire rate is always respected
    private void TryFire(bool fromPress)
    {
        if (IsReloading) return;
        if (Time.time < nextFireTime) return;   // Shoot() sets nextFireTime from prime_fireRate

        if (prime_wpn_mag > 0)
        {
            Shoot();
        }
        else if (prime_wpn_reserve > 0)
        {
            StartReload();
        }
        else if (fromPress)
        {
            Debug.Log("No Ammo");   // only on a click, so holding doesn't spam the console
        }
    }
    private void Shoot()
    {
        if (bulletPrefab == null) return;
        nextFireTime = Time.time + Mathf.Max(0.02f, prime_fireRate);
        Vector2 dir = GetAimDirection();
        float aimAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector2 spawnPos = GetAimOrigin() + dir * muzzleDistance;

        if (_thunderblast)
        {
            _thunderblast = false;
            Instantiate(specialBulletPrefab, spawnPos, Quaternion.Euler(0f, 0f, aimAngle));
            return;
        }

        prime_wpn_mag -= 1;

        // Damage is locked in now, so swapping guns mid-flight doesn't change it
        float damage = prime_wpn_damage * wpn_damage_multiplier;
        int pellets = Mathf.Max(1, prime_pellets);
        float halfSpread = prime_spread * 0.5f;

        for (int i = 0; i < pellets; i++)
        {
            float angle = aimAngle + UnityEngine.Random.Range(-halfSpread, halfSpread);
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
            bullet.transform.localScale = bulletPrefab.transform.localScale * prime_bulletSize;

            Bullets_Script bs = bullet.GetComponent<Bullets_Script>();
            if (bs != null) bs.Init(prime_bulletSpeed, damage, prime_pierce);
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
        if (IsReloading == true) return;

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
    //INITIALIZE WEAPONS
    private void InitializeWeapons()
    {
        // Initialize weapon slots
        weapon_ids = new ArrayList()
        {
            0,  // Primary weapon
            -1, // Secondary weapon empty
            -1  // Third weapon empty
        };

        // Initialize weapon upgrade statuses
        WPNS_ORDER = new Dictionary<int, bool>()
        {
            { 0, false } // Pistol starts unupgraded
        };
    }
    public void OnSwitchWeapon(InputValue value)
    {
        if (value.isPressed)
        {
            CycleWeaponSlot();
        }
    }

    private void CycleWeaponSlot()
    {
        int maxWeapons = GetMaxWeapons();

        if (weapon_ids.Count < maxWeapons)
            return;

        int currentSlot = -1;

        // Find currently equipped weapon.
        for (int i = 0; i < maxWeapons; i++)
        {
            if ((int)weapon_ids[i] == prime_wpn_id)
            {
                currentSlot = i;
                break;
            }
        }

        if (currentSlot == -1)
        {
            Debug.LogWarning("Current weapon not found in inventory!");
            return;
        }

        // Find next owned weapon.
        for (int i = 1; i <= maxWeapons; i++)
        {
            int nextSlot = (currentSlot + i) % maxWeapons;

            int nextWeapon = (int)weapon_ids[nextSlot];

            if (nextWeapon != -1)
            {
                EquipWeapon(nextWeapon);
                return;
            }
        }

        Debug.Log("No other weapons available!");
    }

    //save weapon

    private void SaveCurrentWeapon()
    {
        for (int i = 0; i < GetMaxWeapons(); i++)
        {
            if ((int)weapon_ids[i] != prime_wpn_id)
                continue;

            switch (i)
            {
                case 0:
                    wpn1_mag = prime_wpn_mag;
                    wpn1_reserve = prime_wpn_reserve;
                    break;

                case 1:
                    wpn2_mag = prime_wpn_mag;
                    wpn2_reserve = prime_wpn_reserve;
                    break;

                case 2:
                    wpn3_mag = prime_wpn_mag;
                    wpn3_reserve = prime_wpn_reserve;
                    break;
            }

            return;
        }
    }

    public void EquipWeapon(int weaponId)
    {
        if (IsReloading)
        {
            Debug.Log("Cannot switch weapons while reloading!");
            return;
        }

        int slot = -1;

        for (int i = 0; i < GetMaxWeapons(); i++)
        {
            if ((int)weapon_ids[i] == weaponId)
            {
                slot = i;
                break;
            }
        }

        if (slot == -1)
        {
            Debug.LogWarning("Weapon not found!");
            return;
        }

        // Save old weapon ammunition.
        SaveCurrentWeapon();

        prime_wpn_id = weaponId;

        switch (slot)
        {
            case 0:
                prime_wpn_mag_max = wpn1_mag_max;
                prime_wpn_mag = wpn1_mag;
                prime_wpn_reserve_max = wpn1_reserve_max;
                prime_wpn_reserve = wpn1_reserve;
                prime_wpn_damage = wpn1_damage;
                prime_wpn_reloadTime = wp1_reloadTime;
                prime_fireRate = wp1_fireRate;
                wpn_prime_upgraded = wp1_upgraded;
                prime_wpn_automatic = wp1_automatic;
                prime_bulletSpeed = wp1_bulletSpeed;
                prime_bulletSize = wp1_bulletSize;
                prime_spread = wp1_spread;
                prime_pellets = wp1_pellets;
                prime_pierce = wp1_pierce;
                break;

            case 1:
                prime_wpn_mag_max = wpn2_mag_max;
                prime_wpn_mag = wpn2_mag;
                prime_wpn_reserve_max = wpn2_reserve_max;
                prime_wpn_reserve = wpn2_reserve;
                prime_wpn_damage = wpn2_damage;
                prime_wpn_reloadTime = wp2_reloadTime;
                prime_fireRate = wp2_fireRate;
                wpn_prime_upgraded = wp2_upgraded;
                prime_wpn_automatic = wp2_automatic;
                prime_bulletSpeed = wp2_bulletSpeed;
                prime_bulletSize = wp2_bulletSize;
                prime_spread = wp2_spread;
                prime_pellets = wp2_pellets;
                prime_pierce = wp2_pierce;
                break;

            case 2:
                prime_wpn_mag_max = wpn3_mag_max;
                prime_wpn_mag = wpn3_mag;
                prime_wpn_reserve_max = wpn3_reserve_max;
                prime_wpn_reserve = wpn3_reserve;
                prime_wpn_damage = wpn3_damage;
                prime_wpn_reloadTime = wp3_reloadTime;
                prime_fireRate = wp3_fireRate;
                wpn_prime_upgraded = wp3_upgraded;
                prime_wpn_automatic = wp3_automatic;
                prime_bulletSpeed = wp3_bulletSpeed;
                prime_bulletSize = wp3_bulletSize;
                prime_spread = wp3_spread;
                prime_pellets = wp3_pellets;
                prime_pierce = wp3_pierce;
                break;
        }

        Debug.Log($"Switched to weapon ID: {prime_wpn_id}, Slot: {slot + 1}");
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
            // Enemy contact damage: only enemies with an EnemyContactDamage component hurt on touch
            EnemyContactDamage hurt = collision.gameObject.GetComponentInParent<EnemyContactDamage>();
            if (hurt != null && hurt.enabled)
            {
                TakeDamage(hurt.Damage, collision.transform.position);
            }
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
    //SODAS
    // Called by the power-up when you walk into its radius
    public void RegisterPerks(soda_script soda)
    {
        currentSoda = soda;
    }

    // Called by the power-up when you walk away
    public void UnregisterPerks(soda_script soda)
    {
        if (currentSoda == soda)
        {
            currentSoda = null;
        }
    }

    public void RegisterWallBuy(wallbuy_script wallBuy)
    {
        currentWallBuy = wallBuy;
    }

    public void UnregisterWallBuy(wallbuy_script wallBuy)
    {
        if (currentWallBuy == wallBuy)
        {
            currentWallBuy = null;
        }
    }

    // Called automatically when you hit your Interact input
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            if (currentSoda != null)
            {
                TryBuyPerk(currentSoda,currentSoda.perkName,currentSoda.perkID,currentSoda.sodaCost); // Example perk name and cost
            }
            else if (currentWallBuy != null)
            {
                currentWallBuy.Interact();
            }
            else
            {
                Debug.Log("Nothing to interact with.");
            }
        }
    }
    // --- PERK PURCHASE SYSTEM ---
    public void TryBuyPerk(soda_script perkMachine, string perkName, int perkID, int cost)
    {
        // 1. Check if the player already owns this specific perk
        if (PERKS_ORDER.ContainsValue(perkName))
        {
            Debug.Log($"You already own {perkName}!");
            return;
        }

        // 2. Check max perk limit (4-perk limit)
        if (perk_count >= max_perk_count)
        {
            Debug.Log("Maximum perk limit reached (4/4)!");
            return;
        }

        // 3. Check if player has enough scrap
        if (scrap < cost)
        {
            Debug.Log($"Not enough scrap! Need {cost}, but you only have {scrap}.");
            return;
        }
        // 5. Register the perk in tracking structures
        if (perkID != 5) //unless it is pack a punch, it will not be added to the perks order dictionary
        {
            // 4. Successful purchase! Deduct scrap and update UI
            scrap -= cost;
            if (scrapUI != null) scrapUI.UpdateScrap(scrap);
            PERKS_ORDER.Add(perk_count, perkName);
            perk_count++;
            // 6. Tell the machine to apply its unique perk effect to the player
        }

        // 6. Tell the machine to apply its unique perk effect to the player
        perkMachine.ApplyEffect(this);
        if (perkID != 5)
        {
            Debug.Log($"Successfully purchased {perkName}! Total Perks: {perk_count} | Scrap left: {scrap}");
        }
    }
    //try to buy a wall weapon
    // --- WALL-BUY PURCHASE SYSTEM (2-Weapon Max) ---

    public void TryBuyWallWeapon(wallbuy_script wallBuy, int cost, int weaponId)
    {
        int maxWeapons = GetMaxWeapons();

        // Check whether player already owns this weapon.
        bool alreadyOwns = false;

        Debug.Log("========== WALL BUY DEBUG ==========");
        Debug.Log($"Max Weapons: {maxWeapons}");
        Debug.Log($"Inventory Count: {weapon_ids.Count}");

        for (int i = 0; i < maxWeapons; i++)
        {
            if (i < weapon_ids.Count && (int)weapon_ids[i] == weaponId)
            {
                alreadyOwns = true;
                break;
            }
        }

        // Ammo refills cost half price.
        int actualCost = alreadyOwns ? cost / 2 : cost;

        if (scrap < actualCost)
        {
            Debug.Log($"Not enough scrap! Need {actualCost}, but you have {scrap}.");
            return;
        }

        // Purchase.
        scrap -= actualCost;

        if (scrapUI != null)
            scrapUI.UpdateScrap(scrap);
        Debug.Log("STEP 1: About to call ApplyWeapon");

        wallBuy.ApplyWeapon(this);

        Debug.Log("STEP 2: ApplyWeapon finished");

        Debug.Log($"Wall buy successful! Scrap left: {scrap}");
    }

    // Call this from Mule Kick's ApplyEffect when purchased
    public void EnableMuleKick()
    {
        hasMuleKick = true;

        // Expand your weapon list capacity to hold a 3rd weapon slot if needed
        while (weapon_ids.Count < mule_kick_max_weapons)
        {
            weapon_ids.Add(-1);
        }

        Debug.Log("Mule Kick acquired! Weapon limit increased to 3.");
    }

    // Helper property to check current dynamic weapon limit
    public int GetMaxWeapons()
    {
        return hasMuleKick ? mule_kick_max_weapons : base_max_weapons;
    }
    private void GameOver()
    {
        Debug.Log("Game Over! Restarting run...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
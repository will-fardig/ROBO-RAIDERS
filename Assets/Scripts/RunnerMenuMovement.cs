using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class RunnerMenuMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float resetMargin = 100f;

    [Header("Spaceman Interaction")]
    [SerializeField] private RectTransform spaceman;
    [SerializeField] private float stopDistance = 400f;
    [SerializeField] private float respawnDelay = 1.5f;

    [Header("Shooting")]
    [SerializeField] private RectTransform bullet;
    [SerializeField] private float bulletSpeed = 700f;

    [SerializeField]
    private Vector2 muzzleOffset = new Vector2(300f, 800f);

    [Header("Muzzle Flash")]
    [SerializeField] private RectTransform muzzleFlash;
    [SerializeField] private float flashDuration = 0.10f;

    private RectTransform runnerRect;
    private RectTransform canvasRect;
    private Image runnerImage;

    private Vector2 bulletStartPosition;

    private float startY;
    private float respawnTimer;
    private float flashTimer;

    private bool isShooting;
    private bool isDefeated;

    void Start()
    {
        runnerRect = GetComponent<RectTransform>();
        canvasRect = transform.parent as RectTransform;
        runnerImage = GetComponent<Image>();

        startY = runnerRect.anchoredPosition.y;

        // Hide the bullet when the menu starts.
        if (bullet != null)
        {
            bullet.gameObject.SetActive(false);
        }

        // Hide the muzzle flash when the menu starts.
        if (muzzleFlash != null)
        {
            muzzleFlash.gameObject.SetActive(false);
        }

        // Check required references.
        if (canvasRect == null || spaceman == null || bullet == null)
        {
            Debug.LogError(
                "RunnerMenu: Assign SpacemanMenu and BulletMenu!"
            );

            enabled = false;
            return;
        }
    }

    void Update()
    {
        // Control the muzzle flash duration.
        if (muzzleFlash != null &&
            muzzleFlash.gameObject.activeSelf)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0f)
            {
                muzzleFlash.gameObject.SetActive(false);
            }
        }

        // Wait before respawning the Runner.
        if (isDefeated)
        {
            respawnTimer -= Time.deltaTime;

            if (respawnTimer <= 0f)
            {
                RespawnRunner();
            }

            return;
        }

        // Move the bullet while shooting.
        if (isShooting)
        {
            MoveBullet();
            return;
        }

        // Move the Runner toward the Spaceman.
        runnerRect.anchoredPosition +=
            Vector2.left * moveSpeed * Time.deltaTime;

        float runnerX = runnerRect.anchoredPosition.x;

        float spacemanX =
            canvasRect.InverseTransformPoint(spaceman.position).x;

        // Start shooting when the Runner is close enough.
        if (runnerX <= spacemanX + stopDistance)
        {
            StartShooting();
        }
    }

    void StartShooting()
    {
        isShooting = true;

        // Get the Spaceman position relative to the Canvas.
        Vector3 spacemanCanvasPosition =
            canvasRect.InverseTransformPoint(spaceman.position);

        // Calculate where the bullet should appear.
        bulletStartPosition = new Vector2(
            spacemanCanvasPosition.x + muzzleOffset.x,
            spacemanCanvasPosition.y + muzzleOffset.y
        );

        // Activate the bullet.
        bullet.anchoredPosition = bulletStartPosition;
        bullet.gameObject.SetActive(true);

        // Activate the muzzle flash at the same position.
        if (muzzleFlash != null)
        {
            muzzleFlash.anchoredPosition = bulletStartPosition;
            muzzleFlash.gameObject.SetActive(true);

            flashTimer = flashDuration;
        }

        Debug.Log("Spaceman fired!");
    }

    void MoveBullet()
    {
        // Move the bullet horizontally toward the right.
        bullet.anchoredPosition +=
            Vector2.right * bulletSpeed * Time.deltaTime;

        // Check if the bullet has reached the Runner.
        if (bullet.anchoredPosition.x >=
            runnerRect.anchoredPosition.x)
        {
            DefeatRunner();
        }
    }

    void DefeatRunner()
    {
        isShooting = false;
        isDefeated = true;

        respawnTimer = respawnDelay;

        // Hide the Runner.
        runnerImage.enabled = false;

        // Hide the bullet.
        bullet.gameObject.SetActive(false);

        // Ensure the muzzle flash is hidden.
        if (muzzleFlash != null)
        {
            muzzleFlash.gameObject.SetActive(false);
        }

        Debug.Log("Runner defeated!");
    }

    void RespawnRunner()
    {
        float width = canvasRect.rect.width;

        // Move the Runner back to the right side.
        runnerRect.anchoredPosition = new Vector2(
            width / 2f + resetMargin,
            startY
        );

        // Show the Runner again.
        runnerImage.enabled = true;

        // Reset the animation state.
        isDefeated = false;
        isShooting = false;
    }
}
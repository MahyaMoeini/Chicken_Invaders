using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player damage via GameManager lives/power, invincibility, and blink feedback.
/// Damaging contacts use tags: Enemy, Egg, Asteroid.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isInvincible;
    private bool isDead;
    private Coroutine invincibilityRoutine;
    private Color originalColor = Color.white;

    public float InvincibilityDuration
    {
        get => invincibilityDuration;
        set => invincibilityDuration = Mathf.Max(0f, value);
    }

    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("PlayerHealth requires a GameManager in the scene.");
            return;
        }

        // Ensure Phase 2 starts from the required default life count.
        if (GameManager.Instance.State.Lives <= 0)
        {
            GameManager.Instance.InitializeNewGame();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= HandlePlayerDied;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTakeDamageFrom(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTakeDamageFrom(collision.gameObject);
    }

    public void TakeDamage()
    {
        if (isDead || isInvincible || GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.LoseLife();
        GameManager.Instance.DecreaseWeaponPower();

        PlayerVisualController visuals = GetComponent<PlayerVisualController>();
        visuals?.PlayDamageFlash();

        if (GameManager.Instance.State.Lives <= 0)
        {
            isDead = true;
            VfxSpawner.Explosion(transform.position, 1.2f);
            return;
        }

        BeginInvincibility();
    }

    private void TryTakeDamageFrom(GameObject other)
    {
        if (other == null)
        {
            return;
        }

        if (other.CompareTag("Enemy") || other.CompareTag("Egg") || other.CompareTag("Asteroid"))
        {
            TakeDamage();
        }
    }

    private void BeginInvincibility()
    {
        if (invincibilityRoutine != null)
        {
            StopCoroutine(invincibilityRoutine);
        }

        invincibilityRoutine = StartCoroutine(InvincibilityCoroutine());
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invincibilityDuration)
        {
            visible = !visible;
            SetSpriteVisible(visible);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        SetSpriteVisible(true);
        isInvincible = false;
        invincibilityRoutine = null;
    }

    private void SetSpriteVisible(bool visible)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color color = originalColor;
        color.a = visible ? originalColor.a : 0.25f;
        spriteRenderer.color = color;
    }

    private void HandlePlayerDied()
    {
        isDead = true;
        isInvincible = false;

        if (invincibilityRoutine != null)
        {
            StopCoroutine(invincibilityRoutine);
            invincibilityRoutine = null;
        }

        SetSpriteVisible(true);
    }
}

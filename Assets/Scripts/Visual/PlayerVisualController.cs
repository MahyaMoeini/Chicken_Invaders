using UnityEngine;

/// <summary>
/// Applies ship art, damaged look, exhaust animation, and damage flash.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] private Sprite healthySprite;
    [SerializeField] private Sprite damagedSprite;
    [SerializeField] private Transform exhaustAnchor;
    [SerializeField] private float exhaustOffsetY = -0.22f;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer exhaustRenderer;
    private SimpleSpriteAnimator exhaustAnimator;
    private Color baseColor = Color.white;
    private float flashTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (healthySprite == null)
        {
            healthySprite = VisualAssets.ShipHealthy;
        }

        if (damagedSprite == null)
        {
            damagedSprite = VisualAssets.ShipDamaged;
        }

        if (healthySprite != null)
        {
            spriteRenderer.sprite = healthySprite;
        }

        baseColor = spriteRenderer.color;
        EnsureExhaust();

        ForcedVisualScale scaler = GetComponent<ForcedVisualScale>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<ForcedVisualScale>();
        }

        scaler.SetKind(ForcedVisualScale.Kind.Player);
    }

    private void OnEnable()
    {
        GameEvents.OnLivesChanged += HandleLivesChanged;
        RefreshShipSprite();
    }

    private void OnDisable()
    {
        GameEvents.OnLivesChanged -= HandleLivesChanged;
    }

    private void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            float t = Mathf.PingPong(Time.time * 20f, 1f);
            spriteRenderer.color = Color.Lerp(baseColor, Color.red, t);
            if (flashTimer <= 0f)
            {
                spriteRenderer.color = baseColor;
            }
        }
    }

    public void PlayDamageFlash(float duration = 0.25f)
    {
        flashTimer = duration;
        ScreenShake.Shake(0.12f, 0.18f);
    }

    private void HandleLivesChanged(int lives)
    {
        RefreshShipSprite();
        if (lives > 0 && lives < GameState.DefaultLives)
        {
            PlayDamageFlash();
        }
    }

    private void RefreshShipSprite()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        bool damaged = GameManager.Instance != null && GameManager.Instance.State.Lives <= 1;
        Sprite next = damaged && damagedSprite != null ? damagedSprite : healthySprite;
        if (next != null)
        {
            spriteRenderer.sprite = next;
        }
    }

    private void EnsureExhaust()
    {
        if (exhaustAnchor == null)
        {
            GameObject go = new GameObject("Exhaust");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, exhaustOffsetY, 0f);
            exhaustAnchor = go.transform;
            exhaustRenderer = go.AddComponent<SpriteRenderer>();
            exhaustRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
            exhaustAnchor.localScale = Vector3.one * 0.7f;
            exhaustAnimator = go.AddComponent<SimpleSpriteAnimator>();
        }

        Sprite[] frames = VisualAssets.ExhaustFrames;
        if (exhaustAnimator != null)
        {
            exhaustAnimator.SetFrames(frames, 12f, true);
        }
    }
}

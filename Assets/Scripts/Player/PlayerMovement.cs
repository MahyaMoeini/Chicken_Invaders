using UnityEngine;

/// <summary>
/// Rigidbody2D spaceship movement with normalized diagonals and camera clamping.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Camera boundsCamera;
    [SerializeField] private Vector2 colliderPadding = new Vector2(0.4f, 0.4f);

    private Rigidbody2D rb;
    private PlayerInputReader inputReader;
    private SpriteRenderer spriteRenderer;
    private bool movementEnabled = true;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputReader = GetComponent<PlayerInputReader>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (boundsCamera == null)
        {
            boundsCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDied += DisableMovement;
        GameEvents.OnLivesChanged += HandleLivesChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= DisableMovement;
        GameEvents.OnLivesChanged -= HandleLivesChanged;
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.State.Lives <= 0)
        {
            DisableMovement();
        }
    }

    private void FixedUpdate()
    {
        if (!movementEnabled || inputReader == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Normalize so diagonals are not faster than cardinal movement.
        Vector2 input = inputReader.MoveInput.normalized;

        rb.linearVelocity = input * moveSpeed;
        ClampToCameraBounds();
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!enabled && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void DisableMovement()
    {
        SetMovementEnabled(false);
    }

    private void HandleLivesChanged(int lives)
    {
        if (lives <= 0)
        {
            SetMovementEnabled(false);
        }
    }

    private void ClampToCameraBounds()
    {
        if (boundsCamera == null || !boundsCamera.orthographic)
        {
            return;
        }

        float halfHeight = boundsCamera.orthographicSize;
        float halfWidth = halfHeight * boundsCamera.aspect;

        Vector3 camPos = boundsCamera.transform.position;
        Vector2 halfSize = GetShipHalfExtents();

        float minX = camPos.x - halfWidth + halfSize.x;
        float maxX = camPos.x + halfWidth - halfSize.x;
        float minY = camPos.y - halfHeight + halfSize.y;
        float maxY = camPos.y + halfHeight - halfSize.y;

        Vector2 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        rb.position = pos;
    }

    private Vector2 GetShipHalfExtents()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 extents = spriteRenderer.bounds.extents;
            return new Vector2(
                Mathf.Max(0.01f, extents.x),
                Mathf.Max(0.01f, extents.y));
        }

        return colliderPadding;
    }
}

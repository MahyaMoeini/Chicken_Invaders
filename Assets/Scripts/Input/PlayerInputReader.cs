using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads Move / Fire / Bomb. Falls back to runtime actions if the asset is missing.
/// </summary>
public class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap playerMap;
    private InputAction moveAction;
    private InputAction fireAction;
    private InputAction bombAction;
    private bool ownsRuntimeActions;

    public Vector2 MoveInput { get; private set; }
    public bool IsFireHeld { get; private set; }
    public bool CanReadInput { get; set; } = true;

    public event Action OnFirePressed;
    public event Action OnBombPressed;

    private void Awake()
    {
        if (inputActions != null)
        {
            playerMap = inputActions.FindActionMap("Player", throwIfNotFound: false);
        }

        if (playerMap == null)
        {
            CreateRuntimeActions();
        }

        moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
        fireAction = playerMap.FindAction("Fire", throwIfNotFound: true);
        bombAction = playerMap.FindAction("Bomb", throwIfNotFound: true);
    }

    private void CreateRuntimeActions()
    {
        ownsRuntimeActions = true;
        playerMap = new InputActionMap("Player");

        moveAction = playerMap.AddAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        fireAction = playerMap.AddAction("Fire", InputActionType.Button, "<Keyboard>/space");
        bombAction = playerMap.AddAction("Bomb", InputActionType.Button, "<Keyboard>/x");
    }

    private void OnEnable()
    {
        if (playerMap == null)
        {
            return;
        }

        playerMap.Enable();
        fireAction.performed += HandleFire;
        bombAction.performed += HandleBomb;
    }

    private void OnDisable()
    {
        if (playerMap == null)
        {
            return;
        }

        fireAction.performed -= HandleFire;
        bombAction.performed -= HandleBomb;
        playerMap.Disable();
        MoveInput = Vector2.zero;
        IsFireHeld = false;
    }

    private void OnDestroy()
    {
        if (ownsRuntimeActions && playerMap != null)
        {
            playerMap.Dispose();
            playerMap = null;
        }
    }

    private void Update()
    {
        if (!CanReadInput || moveAction == null || IsGamePaused())
        {
            MoveInput = Vector2.zero;
            IsFireHeld = false;
            return;
        }

        MoveInput = moveAction.ReadValue<Vector2>();
        IsFireHeld = fireAction.IsPressed();
    }

    private void HandleFire(InputAction.CallbackContext context)
    {
        if (!CanReadInput || IsGamePaused())
        {
            return;
        }

        OnFirePressed?.Invoke();
    }

    private void HandleBomb(InputAction.CallbackContext context)
    {
        if (!CanReadInput || IsGamePaused())
        {
            return;
        }

        OnBombPressed?.Invoke();
    }

    private static bool IsGamePaused()
    {
        if (GameOverMenuController.Instance != null && GameOverMenuController.Instance.IsGameOver)
        {
            return true;
        }

        if (VictoryMenuController.Instance != null && VictoryMenuController.Instance.IsVictory)
        {
            return true;
        }

        return PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused;
    }
}

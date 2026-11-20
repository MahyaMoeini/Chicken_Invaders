using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Dynamic TMP HUD for score, lives, bombs, level, and round.
/// </summary>
public class GameplayHudUI : MonoBehaviour
{
    public static GameplayHudUI Instance { get; private set; }

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI livesText;
    private TextMeshProUGUI bombsText;
    private TextMeshProUGUI levelText;
    private TextMeshProUGUI roundText;
    private TextMeshProUGUI bannerText;
    private Image bannerImage;
    private float bannerTimer;

    private static readonly Color PanelColor = new Color(0.05f, 0.12f, 0.28f, 0.72f);
    private static readonly Color AccentGold = new Color(1f, 0.85f, 0.35f, 1f);
    private static readonly Color AccentCyan = new Color(0.45f, 0.9f, 1f, 1f);
    private static readonly Color AccentPink = new Color(1f, 0.45f, 0.65f, 1f);
    private static readonly Color AccentOrange = new Color(1f, 0.7f, 0.3f, 1f);

    public static void EnsureExists()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject go = new GameObject("GameplayHUD");
        go.AddComponent<GameplayHudUI>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildUi();
        RefreshAll();
    }

    private void OnEnable()
    {
        GameEvents.OnScoreChanged += HandleStatChanged;
        GameEvents.OnLivesChanged += HandleStatChanged;
        GameEvents.OnBombCountChanged += HandleStatChanged;
        GameEvents.OnLevelChanged += HandleStatChanged;
        GameEvents.OnRoundChanged += HandleStatChanged;
        GameEvents.OnRoundCompleted += HandleRoundCompleted;
        GameEvents.OnLevelCompleted += HandleLevelCompleted;
        // Game Over / Victory banners are owned by their menu controllers (avoid duplicate).
    }

    private void OnDisable()
    {
        GameEvents.OnScoreChanged -= HandleStatChanged;
        GameEvents.OnLivesChanged -= HandleStatChanged;
        GameEvents.OnBombCountChanged -= HandleStatChanged;
        GameEvents.OnLevelChanged -= HandleStatChanged;
        GameEvents.OnRoundChanged -= HandleStatChanged;
        GameEvents.OnRoundCompleted -= HandleRoundCompleted;
        GameEvents.OnLevelCompleted -= HandleLevelCompleted;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (bannerTimer > 0f)
        {
            bannerTimer -= Time.unscaledDeltaTime;
            if (bannerTimer <= 0f && bannerImage != null)
            {
                bannerImage.gameObject.SetActive(false);
                if (bannerText != null)
                {
                    bannerText.text = string.Empty;
                }
            }
        }
    }

    private void HandleStatChanged(int _)
    {
        RefreshAll();
    }

    private void BuildUi()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        gameObject.AddComponent<GraphicRaycaster>();

        // Top bar background
        CreatePanel("TopBar", new Vector2(0.01f, 0.905f), new Vector2(0.99f, 0.99f), PanelColor);

        scoreText = CreateHudChip(
            "ScoreChip",
            new Vector2(0.02f, 0.915f),
            new Vector2(0.28f, 0.98f),
            null,
            AccentGold,
            TextAlignmentOptions.Left);

        livesText = CreateHudChip(
            "LivesChip",
            new Vector2(0.295f, 0.915f),
            new Vector2(0.45f, 0.98f),
            VisualAssets.Heart,
            AccentPink,
            TextAlignmentOptions.Center);

        bombsText = CreateHudChip(
            "BombsChip",
            new Vector2(0.465f, 0.915f),
            new Vector2(0.62f, 0.98f),
            VisualAssets.BombIcon,
            AccentOrange,
            TextAlignmentOptions.Center);

        levelText = CreateHudChip(
            "LevelChip",
            new Vector2(0.635f, 0.915f),
            new Vector2(0.80f, 0.98f),
            null,
            AccentCyan,
            TextAlignmentOptions.Center);

        roundText = CreateHudChip(
            "RoundChip",
            new Vector2(0.815f, 0.915f),
            new Vector2(0.98f, 0.98f),
            null,
            AccentCyan,
            TextAlignmentOptions.Center);

        GameObject bannerGo = new GameObject("Banner");
        bannerGo.transform.SetParent(transform, false);
        RectTransform rt = bannerGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.2f, 0.55f);
        rt.anchorMax = new Vector2(0.8f, 0.75f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        bannerImage = bannerGo.AddComponent<Image>();
        bannerImage.preserveAspect = true;
        bannerImage.gameObject.SetActive(false);

        bannerText = CreateLabel("BannerText", new Vector2(0.2f, 0.42f), new Vector2(0.8f, 0.52f), 40f, TextAlignmentOptions.Center);
        bannerText.text = string.Empty;
    }

    private Image CreatePanel(string name, Vector2 min, Vector2 max, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private TextMeshProUGUI CreateHudChip(
        string name,
        Vector2 min,
        Vector2 max,
        Sprite icon,
        Color accent,
        TextAlignmentOptions align)
    {
        GameObject chip = new GameObject(name);
        chip.transform.SetParent(transform, false);
        RectTransform chipRt = chip.AddComponent<RectTransform>();
        chipRt.anchorMin = min;
        chipRt.anchorMax = max;
        chipRt.offsetMin = new Vector2(4f, 2f);
        chipRt.offsetMax = new Vector2(-4f, -2f);

        Image bg = chip.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.06f, 0.16f, 0.85f);

        Outline outline = chip.AddComponent<Outline>();
        outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.65f);
        outline.effectDistance = new Vector2(2f, -2f);

        if (icon != null)
        {
            GameObject iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(chip.transform, false);
            RectTransform iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.04f, 0.15f);
            iconRt.anchorMax = new Vector2(0.22f, 0.85f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            Image iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
        }

        GameObject textGo = new GameObject("Value");
        textGo.transform.SetParent(chip.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = icon != null ? new Vector2(0.24f, 0f) : new Vector2(0.05f, 0f);
        textRt.anchorMax = new Vector2(0.98f, 1f);
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 30f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = align;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 18f;
        tmp.fontSizeMax = 34f;
        tmp.text = name;

        // Soft shadow via second outline on text parent chip already; add text outline.
        Outline textOutline = textGo.AddComponent<Outline>();
        textOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        textOutline.effectDistance = new Vector2(1.5f, -1.5f);

        return tmp;
    }

    private TextMeshProUGUI CreateLabel(string name, Vector2 min, Vector2 max, float size, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = align;
        tmp.text = name;
        return tmp;
    }

    private void RefreshAll()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameState s = GameManager.Instance.State;
        if (scoreText != null)
        {
            scoreText.text = $"SCORE  {s.Score:000000}";
        }

        if (livesText != null)
        {
            livesText.text = $"LIVES  ×{s.Lives}";
        }

        if (bombsText != null)
        {
            bombsText.text = $"BOMBS  ×{s.BombCount}";
        }

        if (levelText != null)
        {
            levelText.text = $"LEVEL  {s.CurrentLevel}";
        }

        if (roundText != null)
        {
            roundText.text = $"ROUND  {s.CurrentRound}";
        }
    }

    private void ShowBanner(Sprite sprite, string fallback, float seconds)
    {
        if (bannerImage != null)
        {
            bannerImage.sprite = sprite;
            bannerImage.color = Color.white;
            bannerImage.gameObject.SetActive(sprite != null);
        }

        if (bannerText != null)
        {
            bannerText.text = sprite == null ? fallback : string.Empty;
        }

        bannerTimer = seconds;
    }

    private void HandleRoundCompleted(int round)
    {
        ShowBanner(VisualAssets.BannerRoundComplete, $"Round {round} Complete", 1.5f);
    }

    private void HandleLevelCompleted(int level)
    {
        ShowBanner(VisualAssets.BannerStageComplete, $"Level {level} Complete", 2f);
    }
}

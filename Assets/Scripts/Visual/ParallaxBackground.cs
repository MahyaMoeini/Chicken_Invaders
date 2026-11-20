using UnityEngine;

/// <summary>
/// Keeps the level background locked to the camera and fully covering the view.
/// Uses a subtle far-layer drift that stays on-screen (no gaps / camera clear color).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private bool createFarLayer = true;
    [SerializeField] private float farLayerScale = 1.08f;
    [SerializeField] private float farLayerAlpha = 0.28f;
    [SerializeField] private Vector2 farDrift = new Vector2(0.08f, 0.05f);
    [SerializeField] private float farDriftAmount = 0.25f;

    private SpriteRenderer mainRenderer;
    private Transform farLayer;
    private Camera cam;
    private float zPos;

    private void Awake()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        zPos = transform.position.z;
        FitToCamera();
        EnsureFarLayer();
    }

    private void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                return;
            }
        }

        // Always cover the camera — never drift the main background off-screen.
        Vector3 camPos = cam.transform.position;
        transform.position = new Vector3(camPos.x, camPos.y, zPos);
        FitToCamera();

        if (farLayer != null)
        {
            float ox = Mathf.Sin(Time.time * farDrift.x) * farDriftAmount;
            float oy = Mathf.Cos(Time.time * farDrift.y) * farDriftAmount;
            farLayer.position = new Vector3(camPos.x + ox, camPos.y + oy, zPos + 1f);
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (mainRenderer == null)
        {
            mainRenderer = GetComponent<SpriteRenderer>();
        }

        mainRenderer.sprite = sprite;
        FitToCamera();

        if (farLayer != null)
        {
            SpriteRenderer sr = farLayer.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = sprite;
            }
        }
    }

    private void FitToCamera()
    {
        if (cam == null || !cam.orthographic || mainRenderer == null || mainRenderer.sprite == null)
        {
            return;
        }

        Bounds b = mainRenderer.sprite.bounds;
        if (b.size.x < 0.01f || b.size.y < 0.01f)
        {
            return;
        }

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;
        float scaleY = worldHeight / b.size.y;
        float scaleX = worldWidth / b.size.x;
        float s = Mathf.Max(scaleX, scaleY) * 1.08f;
        transform.localScale = new Vector3(s, s, 1f);

        if (farLayer != null)
        {
            farLayer.localScale = Vector3.one * (s * farLayerScale);
        }
    }

    private void EnsureFarLayer()
    {
        if (!createFarLayer || mainRenderer == null || mainRenderer.sprite == null)
        {
            return;
        }

        if (farLayer != null)
        {
            return;
        }

        GameObject far = new GameObject("ParallaxFar");
        far.transform.SetParent(transform.parent, false);
        SpriteRenderer sr = far.AddComponent<SpriteRenderer>();
        sr.sprite = mainRenderer.sprite;
        sr.sortingOrder = mainRenderer.sortingOrder - 1;
        Color c = sr.color;
        c.a = farLayerAlpha;
        sr.color = c;
        farLayer = far.transform;
    }
}

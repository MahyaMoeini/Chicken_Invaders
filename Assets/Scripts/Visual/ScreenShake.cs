using UnityEngine;

/// <summary>
/// Simple camera shake used by damage / bombs / explosions.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    private Transform cam;
    private Vector3 originalLocalPos;
    private float trauma;
    private float timer;

    private void Awake()
    {
        Instance = this;
        Camera main = Camera.main;
        cam = main != null ? main.transform : null;
        if (cam != null)
        {
            originalLocalPos = cam.localPosition;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static void Shake(float amplitude = 0.15f, float duration = 0.2f)
    {
        if (Instance == null)
        {
            Camera main = Camera.main;
            if (main == null)
            {
                return;
            }

            Instance = main.gameObject.GetComponent<ScreenShake>();
            if (Instance == null)
            {
                Instance = main.gameObject.AddComponent<ScreenShake>();
            }
        }

        Instance.trauma = Mathf.Max(Instance.trauma, amplitude);
        Instance.timer = Mathf.Max(Instance.timer, duration);
    }

    private void LateUpdate()
    {
        if (cam == null)
        {
            return;
        }

        if (timer <= 0f)
        {
            cam.localPosition = originalLocalPos;
            return;
        }

        timer -= Time.unscaledDeltaTime;
        float shake = trauma * (timer > 0f ? 1f : 0f);
        cam.localPosition = originalLocalPos + (Vector3)(Random.insideUnitCircle * shake);
        if (timer <= 0f)
        {
            trauma = 0f;
            cam.localPosition = originalLocalPos;
        }
    }
}

using UnityEngine;

/// <summary>
/// Spawns one-shot visual effects: explosions, feathers, egg break, bomb shockwave.
/// </summary>
public static class VfxSpawner
{
    public static void Explosion(Vector3 position, float scale = 1f)
    {
        SpawnAnimated(position, VisualAssets.ExplosionFrames, 14f, scale, 12);
        ScreenShake.Shake(0.1f * scale, 0.15f);
    }

    public static void FeatherBurst(Vector3 position)
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject go = new GameObject("Feather");
            go.transform.position = position;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VisualAssets.FoodDrumstick != null
                ? VisualAssets.ChickenFrames("normal")[Random.Range(0, 4)]
                : null;
            // Prefer tiny colored quads if chicken frame missing.
            if (sr.sprite == null)
            {
                sr.color = new Color(1f, 0.9f, 0.5f, 1f);
            }
            else
            {
                go.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);
            }

            sr.sortingOrder = 30;
            FeatherParticle fp = go.AddComponent<FeatherParticle>();
            fp.Init(Random.insideUnitCircle.normalized * Random.Range(1.5f, 3.5f));
        }
    }

    public static void EggBreak(Vector3 position)
    {
        GameObject go = new GameObject("EggBreak");
        go.transform.position = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VisualAssets.EggCracked;
        sr.sortingOrder = 25;
        go.transform.localScale = Vector3.one * 0.55f;
        Object.Destroy(go, 0.35f);
    }

    public static void BombShockwave(Vector3 position)
    {
        GameObject go = new GameObject("BombShockwave");
        go.transform.position = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VisualAssets.BombIcon != null ? VisualAssets.BombIcon : VisualAssets.ExplosionFrames[0];
        sr.sortingOrder = 40;
        sr.color = new Color(1f, 1f, 1f, 0.75f);
        go.AddComponent<ShockwavePulse>();
        ScreenShake.Shake(0.28f, 0.35f);
    }

    public static void LaserBloom(Vector3 position, Sprite laserSprite)
    {
        if (laserSprite == null)
        {
            return;
        }

        GameObject go = new GameObject("LaserBloom");
        go.transform.position = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = laserSprite;
        sr.sortingOrder = 19;
        sr.color = new Color(0.6f, 0.9f, 1f, 0.45f);
        go.transform.localScale = new Vector3(1.6f, 1.15f, 1f);
        Object.Destroy(go, 0.08f);
    }

    private static void SpawnAnimated(Vector3 position, Sprite[] frames, float fps, float scale, int sort)
    {
        if (frames == null || frames.Length == 0 || frames[0] == null)
        {
            return;
        }

        GameObject go = new GameObject("VfxAnim");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * scale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sort;
        SimpleSpriteAnimator anim = go.AddComponent<SimpleSpriteAnimator>();
        // destroyOnComplete via helper component
        go.AddComponent<DestroyAfterAnim>().Setup(anim, frames, fps);
    }
}

public class FeatherParticle : MonoBehaviour
{
    private Vector2 velocity;
    private float life = 0.55f;

    public void Init(Vector2 vel)
    {
        velocity = vel;
    }

    private void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity *= 0.96f;
        transform.Rotate(0f, 0f, 180f * Time.deltaTime);
        life -= Time.deltaTime;
        if (life <= 0f)
        {
            Destroy(gameObject);
        }
    }
}

public class ShockwavePulse : MonoBehaviour
{
    private float t;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        t += Time.deltaTime;
        float s = Mathf.Lerp(0.4f, 4.5f, t / 0.4f);
        transform.localScale = Vector3.one * s;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(0.7f, 0f, t / 0.4f);
            sr.color = c;
        }

        if (t >= 0.4f)
        {
            Destroy(gameObject);
        }
    }
}

public class DestroyAfterAnim : MonoBehaviour
{
    public void Setup(SimpleSpriteAnimator anim, Sprite[] frames, float fps)
    {
        anim.SetFrames(frames, fps, false);
        Destroy(gameObject, (frames.Length / Mathf.Max(1f, fps)) + 0.05f);
    }
}

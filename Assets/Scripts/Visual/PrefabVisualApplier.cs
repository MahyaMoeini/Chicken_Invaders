using UnityEngine;

/// <summary>
/// Applies visual-pack sprites and attaches ForcedVisualScale by component type
/// (not by GameObject name, which was unreliable).
/// </summary>
public class PrefabVisualApplier : MonoBehaviour
{
    private void Awake()
    {
        Apply(gameObject);
    }

    private void Start()
    {
        Apply(gameObject);
    }

    public static void Apply(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            return;
        }

        // Identify by gameplay components (reliable).
        if (go.GetComponent<PlayerHealth>() != null || go.GetComponent<PlayerMovement>() != null)
        {
            if (go.GetComponent<PlayerVisualController>() == null)
            {
                go.AddComponent<PlayerVisualController>();
            }

            if (VisualAssets.ShipHealthy != null)
            {
                sr.sprite = VisualAssets.ShipHealthy;
            }

            EnsureScaler(go, ForcedVisualScale.Kind.Player);
            return;
        }

        if (go.GetComponent<EggProjectile>() != null)
        {
            if (VisualAssets.Egg != null)
            {
                sr.sprite = VisualAssets.Egg;
            }

            EnsureScaler(go, ForcedVisualScale.Kind.Egg);
            return;
        }

        if (go.GetComponent<WeaponGiftPickup>() != null)
        {
            if (VisualAssets.WeaponGift != null)
            {
                sr.sprite = VisualAssets.WeaponGift;
            }

            EnsureScaler(go, ForcedVisualScale.Kind.Gift);
            return;
        }

        if (go.GetComponent<Projectile>() != null)
        {
            EnsureScaler(go, ForcedVisualScale.Kind.Laser);
            return;
        }

        if (go.GetComponent<AsteroidController>() != null)
        {
            if (VisualAssets.AsteroidMedium != null)
            {
                sr.sprite = VisualAssets.AsteroidMedium;
            }

            EnsureScaler(go, ForcedVisualScale.Kind.Asteroid);
            return;
        }

        if (go.GetComponent<EnemyController>() != null)
        {
            string type = "normal";
            string n = go.name;
            if (n.Contains("Armored"))
            {
                type = "armored";
            }
            else if (n.Contains("Alien"))
            {
                type = "alien";
            }

            EnsureChicken(go, sr, type);
            // Keep chickens at their original prefab scale (no ForcedVisualScale).
            go.transform.localScale = Vector3.one;
        }
    }

    private static void EnsureChicken(GameObject go, SpriteRenderer sr, string type)
    {
        ChickenVisualController visual = go.GetComponent<ChickenVisualController>();
        if (visual == null)
        {
            visual = go.AddComponent<ChickenVisualController>();
        }

        visual.SetType(type);
        Sprite[] frames = VisualAssets.ChickenFrames(type);
        if (frames != null && frames.Length > 0 && frames[0] != null)
        {
            sr.sprite = frames[0];
        }
    }

    private static void EnsureScaler(GameObject go, ForcedVisualScale.Kind kind)
    {
        ForcedVisualScale scaler = go.GetComponent<ForcedVisualScale>();
        if (scaler == null)
        {
            scaler = go.AddComponent<ForcedVisualScale>();
        }

        scaler.SetKind(kind);
    }
}

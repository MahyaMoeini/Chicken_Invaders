using UnityEngine;

/// <summary>
/// Forces a constant localScale every LateUpdate so size changes always stick.
/// Added automatically to Player / Egg / Gift / etc.
/// </summary>
public class ForcedVisualScale : MonoBehaviour
{
    public enum Kind
    {
        Player,
        Egg,
        Gift,
        Laser,
        Chicken,
        Asteroid
    }

    [SerializeField] private Kind kind = Kind.Player;

    public void SetKind(Kind newKind)
    {
        kind = newKind;
        ApplyNow();
    }

    private void OnEnable()
    {
        ApplyNow();
    }

    private void Start()
    {
        ApplyNow();
    }

    private void LateUpdate()
    {
        ApplyNow();
    }

    private void ApplyNow()
    {
        float s = kind switch
        {
            Kind.Player => VisualSizeSettings.Player,
            Kind.Egg => VisualSizeSettings.Egg,
            Kind.Gift => VisualSizeSettings.Gift,
            Kind.Laser => VisualSizeSettings.Laser,
            Kind.Chicken => VisualSizeSettings.Chicken,
            Kind.Asteroid => VisualSizeSettings.Asteroid,
            _ => 1f
        };

        s = Mathf.Max(0.01f, s);
        Transform t = transform;
        if (!Mathf.Approximately(t.localScale.x, s) ||
            !Mathf.Approximately(t.localScale.y, s))
        {
            t.localScale = new Vector3(s, s, 1f);
        }
    }
}

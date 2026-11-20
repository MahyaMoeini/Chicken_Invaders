using UnityEngine;

/// <summary>
/// Wing-flap animation for chicken prefabs. Type: normal / armored / alien.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ChickenVisualController : MonoBehaviour
{
    [SerializeField] private string chickenType = "normal";
    [SerializeField] private float framesPerSecond = 7f;

    private SimpleSpriteAnimator animator;

    private void Awake()
    {
        ApplyFrames();
    }

    public void SetType(string type)
    {
        chickenType = type;
        ApplyFrames();
    }

    private void ApplyFrames()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (animator == null)
        {
            animator = GetComponent<SimpleSpriteAnimator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<SimpleSpriteAnimator>();
            }
        }

        Sprite[] frames = VisualAssets.ChickenFrames(chickenType);
        if (frames != null && frames.Length > 0 && frames[0] != null)
        {
            if (sr != null)
            {
                sr.sprite = frames[0];
            }

            animator.SetFrames(frames, framesPerSecond, true);
        }
    }
}

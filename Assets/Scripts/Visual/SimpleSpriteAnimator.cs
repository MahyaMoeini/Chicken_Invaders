using UnityEngine;

/// <summary>
/// Loops a small sprite frame list (wings, exhaust, explosions).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool destroyOnComplete;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int index;
    private bool playing = true;

    public void SetFrames(Sprite[] newFrames, float fps = 8f, bool shouldLoop = true)
    {
        frames = newFrames;
        framesPerSecond = Mathf.Max(1f, fps);
        loop = shouldLoop;
        index = 0;
        timer = 0f;
        playing = frames != null && frames.Length > 0;
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (playing && frames[0] != null)
        {
            spriteRenderer.sprite = frames[0];
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if ((frames == null || frames.Length == 0) == false && frames[0] != null)
        {
            spriteRenderer.sprite = frames[0];
        }
    }

    private void Update()
    {
        if (!playing || frames == null || frames.Length == 0)
        {
            return;
        }

        timer += Time.deltaTime;
        float frameTime = 1f / framesPerSecond;
        while (timer >= frameTime)
        {
            timer -= frameTime;
            index++;
            if (index >= frames.Length)
            {
                if (loop)
                {
                    index = 0;
                }
                else
                {
                    playing = false;
                    index = frames.Length - 1;
                    if (destroyOnComplete)
                    {
                        Destroy(gameObject);
                    }

                    break;
                }
            }

            if (frames[index] != null)
            {
                spriteRenderer.sprite = frames[index];
            }
        }
    }
}


using UnityEngine;

public class EnemySpikes : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private float timer;
    private bool faceExposed;
    private bool isDead;

    [Header("Sprites")]
    public Sprite spikesUpSprite;
    public Sprite faceExposedSprite;

    [Header("Cores temporárias")]
    public Color spikesColor = Color.red;
    public Color faceColor = Color.green;

    [Header("Hitboxes")]
    public Collider2D spikesHitbox;
    public Collider2D faceHitbox;

    [Header("Tempos")]
    public float spikesDuration = 3f;
    public float faceDuration = 1.5f;

    [Header("Combate")]
    public float StompForce = 5f;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        SetState(false);
    }

    void Update()
    {
        if (isDead)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SetState(!faceExposed);
        }
    }

    private void SetState(bool exposeFace)
    {
        faceExposed = exposeFace;

        if (faceExposed)
        {
            timer = faceDuration;

            if (spriteRenderer != null)
            {
                if (faceExposedSprite != null)
                {
                    spriteRenderer.sprite = faceExposedSprite;
                    spriteRenderer.color = Color.white;
                }
                else
                {
                    spriteRenderer.color = faceColor;
                }
            }

            spikesHitbox.enabled = true;
            faceHitbox.enabled = true;

            Debug.Log("Inimigo: ROSTO EXPOSTO");
        }
        else
        {
            timer = spikesDuration;

            if (spriteRenderer != null)
            {
                if (spikesUpSprite != null)
                {
                    spriteRenderer.sprite = spikesUpSprite;
                    spriteRenderer.color = Color.white;
                }
                else
                {
                    spriteRenderer.color = spikesColor;
                }
            }

            spikesHitbox.enabled = true;
            faceHitbox.enabled = false;

            Debug.Log("Inimigo: ESPINHOS LEVANTADOS");
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        spikesHitbox.enabled = false;
        faceHitbox.enabled = false;

        Destroy(gameObject);
    }
}

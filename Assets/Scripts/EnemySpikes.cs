
using UnityEngine;

public class EnemySpikes : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer faceSpriteRenderer;

    private float timer;
    private bool faceExposed;
    private bool isDead;
    private Vector3 spawnPosition;

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
        spawnPosition = transform.position;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (faceHitbox != null)
        {
            faceSpriteRenderer = faceHitbox.GetComponentInChildren<SpriteRenderer>();

            if (faceSpriteRenderer != null)
            {
                faceSpriteRenderer.color = faceColor;
                if (faceExposedSprite != null)
                    faceSpriteRenderer.sprite = faceExposedSprite;

                if (faceHitbox is BoxCollider2D faceBox)
                    faceSpriteRenderer.transform.localPosition = faceBox.offset;
            }
        }

        if (spriteRenderer != null)
        {
            if (spikesUpSprite != null)
                spriteRenderer.sprite = spikesUpSprite;
            spriteRenderer.color = spikesColor;
        }
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

            spikesHitbox.enabled = true;
            faceHitbox.enabled = true;
            if (faceSpriteRenderer != null)
                faceSpriteRenderer.enabled = true;
        }
        else
        {
            timer = spikesDuration;

            spikesHitbox.enabled = true;
            faceHitbox.enabled = false;
            if (faceSpriteRenderer != null)
                faceSpriteRenderer.enabled = false;
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        spikesHitbox.enabled = false;
        faceHitbox.enabled = false;
        if (faceSpriteRenderer != null)
            faceSpriteRenderer.enabled = false;

        gameObject.SetActive(false);
    }

    public void ResetAfterRespawn()
    {
        transform.position = spawnPosition;
        gameObject.SetActive(true);
        isDead = false;
        SetState(false);
    }
}

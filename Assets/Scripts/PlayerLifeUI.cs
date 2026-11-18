using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerLifeUI : MonoBehaviour
{
    [Header("Corações")]
    [SerializeField] private Sprite heartSprite;
    [SerializeField] private Sprite colorHeartSprite;
    [SerializeField] private Vector2 heartSize = new Vector2(48f, 48f);
    [SerializeField] private float spacing = 8f;
    [SerializeField] private Vector2 screenMargin = new Vector2(24f, 24f);

    [Header("Piscar ao perder vida")]
    [SerializeField, Min(0f)] private float blinkDuration = 0.8f;
    [SerializeField, Min(0.01f)] private float blinkInterval = 0.1f;

    [Header("Escala da interface")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private int canvasSortingOrder;

    private readonly List<Image> hearts = new List<Image>();
    private PlayerHealth playerHealth;
    private int displayedHealth;
    private Coroutine lossAnimation;
    private Canvas lifeCanvas;
    private bool colorState;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerHealth.HealthChanged += OnHealthChanged;
        CreateHearts(playerHealth.maxHealth);
        displayedHealth = playerHealth.health;
        SetVisibleHearts(displayedHealth);
    }

    private void OnHealthChanged(int health)
    {
        if (lossAnimation != null)
        {
            StopCoroutine(lossAnimation);
            lossAnimation = null;
        }

        int previousHealth = displayedHealth;
        displayedHealth = Mathf.Clamp(health, 0, hearts.Count);
        SetVisibleHearts(previousHealth);

        if (displayedHealth < previousHealth && displayedHealth < hearts.Count && blinkDuration > 0f)
            lossAnimation = StartCoroutine(BlinkLostHeart(displayedHealth, displayedHealth));
        else
            SetVisibleHearts(displayedHealth);
    }

    private void CreateHearts(int count)
    {
        GameObject canvasObject = new GameObject("Player Life Canvas", typeof(RectTransform));
        lifeCanvas = canvasObject.AddComponent<Canvas>();
        lifeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        lifeCanvas.sortingOrder = canvasSortingOrder;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = lifeCanvas.GetComponent<RectTransform>();
        for (int i = 0; i < count; i++)
        {
            GameObject heartObject = new GameObject(
                "Heart " + (i + 1),
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            heartObject.transform.SetParent(canvasRect, false);

            RectTransform rect = heartObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = heartSize;
            rect.anchoredPosition = new Vector2(
                screenMargin.x + i * (heartSize.x + spacing),
                -screenMargin.y
            );

            Image image = heartObject.GetComponent<Image>();
            image.sprite = colorState && colorHeartSprite != null ? colorHeartSprite : heartSprite;
            image.preserveAspect = true;
            hearts.Add(image);
        }
    }

    public void SetColorState(bool colored)
    {
        colorState = colored;
        foreach (Image heart in hearts)
            heart.sprite = colored && colorHeartSprite != null ? colorHeartSprite : heartSprite;
    }

    public void SetVisible(bool visible)
    {
        if (lifeCanvas != null)
            lifeCanvas.enabled = visible;
    }

    private IEnumerator BlinkLostHeart(int lostHeartIndex, int currentHealth)
    {
        Image lostHeart = hearts[lostHeartIndex];
        float elapsed = 0f;
        float interval = Mathf.Max(0.01f, blinkInterval);
        float duration = Mathf.Max(0f, blinkDuration);
        bool visible = true;

        while (elapsed < duration)
        {
            lostHeart.enabled = visible;
            visible = !visible;
            yield return new WaitForSecondsRealtime(interval);
            elapsed += interval;
        }

        SetVisibleHearts(currentHealth);
        lossAnimation = null;
    }

    private void SetVisibleHearts(int health)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].enabled = true;
            hearts[i].gameObject.SetActive(i < health);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= OnHealthChanged;
    }
}

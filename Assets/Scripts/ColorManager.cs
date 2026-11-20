using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    public GameObject tilemapCinza;
    public GameObject tilemapColorido;
    public Sprite playerColorSprite;

    [Header("Clarão da coleta")]
    public float flashGrowDuration = 0.35f;
    public float flashHoldDuration = 1f;
    public float flashFadeDuration = 0.5f;
    public int flashCanvasSortingOrder = 10000;

    private bool isActivating;

    public void AtivarCor(Move_CHR player, Vector3 flashOrigin)
    {
        if (isActivating || player == null)
            return;

        isActivating = true;
        player.SetControlLocked(true);
        StartCoroutine(PlayColorSequence(player, flashOrigin));
    }

    private IEnumerator PlayColorSequence(Move_CHR player, Vector3 flashOrigin)
    {
        Image flash = CreateFlash(flashOrigin, player);
        RectTransform flashRect = (RectTransform)flash.transform;
        RectTransform canvasRect = (RectTransform)flashRect.parent;
        float canvasWidth = canvasRect.rect.width;
        float flashStartX = flashRect.anchoredPosition.x;
        float growDuration = Mathf.Max(0f, flashGrowDuration);
        float elapsed = 0f;

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / growDuration);
            flashRect.sizeDelta = new Vector2(canvasWidth * progress, 0f);
            flashRect.anchoredPosition = new Vector2(
                Mathf.Lerp(flashStartX, 0f, progress),
                0f
            );
            yield return null;
        }

        flashRect.sizeDelta = new Vector2(canvasWidth, 0f);
        flashRect.anchoredPosition = Vector2.zero;
        yield return new WaitForSeconds(Mathf.Max(0f, flashHoldDuration));

        if (playerColorSprite != null)
        {
            SpriteRenderer playerRenderer = player.GetComponentInChildren<SpriteRenderer>();
            if (playerRenderer != null)
                playerRenderer.sprite = playerColorSprite;
        }
        else
        {
            Debug.LogWarning("Atribua o sprite colorido do jogador no ColorManager.", this);
        }

        if (tilemapCinza != null)
            tilemapCinza.SetActive(false);
        if (tilemapColorido != null)
            tilemapColorido.SetActive(true);

        player.GetComponent<PlayerHealth>()?.RestoreFullHealth();
        player.GetComponent<PlayerLifeUI>()?.SetColorState(true);

        float fadeDuration = Mathf.Max(0f, flashFadeDuration);
        elapsed = 0f;
        Color flashColor = flash.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            flashColor.a = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            flash.color = flashColor;
            yield return null;
        }

        player.GetComponent<PlayerLifeUI>()?.SetVisible(true);
        Destroy(flash.transform.root.gameObject);
        player.SetControlLocked(false);
        isActivating = false;
    }

    private Image CreateFlash(Vector3 position, Move_CHR player)
    {
        GameObject canvasObject = new GameObject("Fragment Screen Flash", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = flashCanvasSortingOrder;

        GameObject flashObject = new GameObject("Flash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        flashObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = (RectTransform)flashObject.transform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        Vector2 screenPosition = UnityEngine.Camera.main.WorldToScreenPoint(position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            screenPosition,
            null,
            out Vector2 canvasPosition
        );
        rect.anchoredPosition = new Vector2(canvasPosition.x, 0f);
        rect.sizeDelta = Vector2.zero;

        Image image = flashObject.GetComponent<Image>();
        image.color = Color.white;
        player.GetComponent<PlayerLifeUI>()?.SetVisible(false);
        return image;
    }
}

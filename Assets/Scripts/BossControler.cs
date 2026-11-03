
using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    private int currentHealth;
    private bool isFighting;
    private bool isVulnerable;
    private bool isDead;

    private SpriteRenderer spriteRenderer;

    [Header("Vida")]
    public int maxHealth = 3;

    [Header("Posicoes")]
    public Transform airbornePoint;
    public Transform groundPoint;

    [Header("Tempos")]
    public float warningDuration = 1.5f;
    public float descentDuration = 0.5f;
    public float impactDuration = 0.8f;
    public float vulnerableDuration = 3f;
    public float ascentDuration = 0.7f;

    [Header("Hitboxes")]
    public Collider2D floorAttackZone;
    public Collider2D headHitbox;

    [Header("Cores temporarias")]
    public Color idleColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color attackingColor = Color.red;
    public Color vulnerableColor = Color.green;

    [Header("Pulo do jogador")]
    public float stompForce = 5f;

    [Header("Arena")]
    public BossArenaController arenaController;

    [Header("Visual do ataque")]
    public SpriteRenderer attackVisualRenderer;
    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        transform.position = airbornePoint.position;
        currentHealth = maxHealth;

        floorAttackZone.enabled = false;
        headHitbox.enabled = false;

        if (attackVisualRenderer != null)
    attackVisualRenderer.enabled = false;
    }

    public void StartFight()
    {
        if (isFighting || isDead)
            return;

        isFighting = true;

        currentHealth = maxHealth;

        // Comeca no ponto aereo.
        transform.position = airbornePoint.position;

        StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {
        while (!isDead)
        {
            // 1. PREPARACAO NO AR
            isVulnerable = false;

            floorAttackZone.enabled = false;
            headHitbox.enabled = false;

            SetColor(warningColor);

            Debug.Log("Boss preparando ataque!");

            yield return new WaitForSeconds(warningDuration);

            if (isDead)
                yield break;

            // 2. DESCIDA
            SetColor(attackingColor);

            Debug.Log("Boss descendo!");

            yield return MoveBoss(
                groundPoint.position,
                descentDuration
            );

            if (isDead)
                yield break;

            // 3. IMPACTO NO CHAO
            floorAttackZone.enabled = true;

            if (attackVisualRenderer != null)
                attackVisualRenderer.enabled = true;

            yield return new WaitForSeconds(impactDuration);

            floorAttackZone.enabled = false;

            if (attackVisualRenderer != null)
                attackVisualRenderer.enabled = false;

            if (isDead)
                yield break;

            // 4. VULNERABILIDADE
            isVulnerable = true;

            headHitbox.enabled = true;

            SetColor(vulnerableColor);

            Debug.Log("Boss vulneravel!");

            yield return new WaitForSeconds(vulnerableDuration);

            isVulnerable = false;
            headHitbox.enabled = false;

            if (isDead)
                yield break;

            // 5. SOBE NOVAMENTE
            SetColor(idleColor);

            Debug.Log("Boss subindo!");

            yield return MoveBoss(
                airbornePoint.position,
                ascentDuration
            );
        }
    }

    private IEnumerator MoveBoss(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (isDead)
                yield break;

            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            transform.position = Vector3.Lerp(
                start,
                target,
                progress
            );

            yield return null;
        }

        transform.position = target;
    }

    
    public void TryStomp(Move_CHR player, Collider2D playerCollider)
    {
        if (!isVulnerable || isDead)
            return;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return;

        // O jogador precisa estar descendo.
        if (playerRb.linearVelocity.y >= 0)
            return;

        // O centro do jogador precisa estar acima
        // do centro da hitbox da cabeça.
        if (playerCollider.bounds.center.y <= headHitbox.bounds.center.y)
            return;

        isVulnerable = false;
        headHitbox.enabled = false;

        currentHealth--;

        Debug.Log("Boss recebeu dano! Vida: " + currentHealth);

        player.BounceAfterStomp(stompForce);

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    private void SetColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        floorAttackZone.enabled = false;
        headHitbox.enabled = false;

        Debug.Log("Boss derrotado!");

        if (attackVisualRenderer != null)
            attackVisualRenderer.enabled = false;

        if (arenaController != null)
            arenaController.OnBossDefeated();

        Destroy(gameObject);
    }
}

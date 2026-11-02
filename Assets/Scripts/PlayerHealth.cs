
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Move_CHR movement;

    private float invulnerabilityTimer;
    private bool collisionIgnored;

    public int health = 3;

    public float KnockbackHorizontal = 6f;
    public float KnockbackVertical = 3f;

    public float InvulnerabilityTime = 2f;

    public bool IsDead { get; private set; }
    public bool IsInvulnerable => invulnerabilityTimer > 0f;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        movement = GetComponent<Move_CHR>();
    }

    void Start()
    {
        // Garante que a colisão comece habilitada
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (playerLayer == -1 || enemyLayer == -1)
        {
            Debug.LogError("Crie as Layers Player e Enemy!");
        }
        else
        {
            Physics2D.IgnoreLayerCollision(
                playerLayer,
                enemyLayer,
                false
            );
        }

        collisionIgnored = false;
    }

    void Update()
    {
        if (IsDead)
            return;

        if (invulnerabilityTimer > 0)
        {
            invulnerabilityTimer -= Time.deltaTime;

            if (invulnerabilityTimer <= 0)
            {
                invulnerabilityTimer = 0;
                SetEnemyCollision(false);

                if (spriteRenderer != null)
                    spriteRenderer.enabled = true;
            }
            else
            {
                // Piscar durante a invulnerabilidade
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled =
                        Mathf.FloorToInt(Time.time * 10) % 2 == 0;
                }
            }
        }
    }

    public void TakeDamage(Transform enemy)
    {
        if (IsDead || IsInvulnerable)
            return;

        health--;

        Debug.Log("Vida restante: " + health);

        if (health <= 0)
        {
            Die();
            return;
        }

        // Ativa invulnerabilidade
        invulnerabilityTimer = InvulnerabilityTime;
        SetEnemyCollision(true);

        // Aplica knockback através do script de movimento
        if (movement != null)
        {
            movement.ApplyKnockback(
                enemy,
                KnockbackHorizontal,
                KnockbackVertical
            );
        }
    }

    private void SetEnemyCollision(bool ignore)
    {
        if (collisionIgnored == ignore)
            return;

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (playerLayer == -1 || enemyLayer == -1)
            return;

        Physics2D.IgnoreLayerCollision(
            playerLayer,
            enemyLayer,
            ignore
        );

        collisionIgnored = ignore;
    }

    public void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}

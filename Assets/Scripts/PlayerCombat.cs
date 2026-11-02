
using UnityEngine;

[RequireComponent(typeof(Move_CHR))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerCombat : MonoBehaviour
{
    private Rigidbody2D rb;
    private Move_CHR movement;
    private PlayerHealth playerHealth;

    public float StompForce = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<Move_CHR>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (playerHealth.IsDead || playerHealth.IsInvulnerable)
            return;

        if (!collision.gameObject.CompareTag("Enemy"))
            return;

        Inimigo enemy = collision.gameObject.GetComponent<Inimigo>();

        if (enemy == null)
            return;

        // Verifica se o jogador pisou no inimigo
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f && rb.linearVelocity.y <= 0.1f)
            {
                enemy.Die();
                movement.BounceAfterStomp(StompForce);
                return;
            }
        }

        // Caso contrário, recebe dano
        playerHealth.TakeDamage(enemy.transform);
    }
}


using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemySpikesHitbox : MonoBehaviour
{
    private EnemySpikes enemy;
    private Collider2D hitbox;

    public bool isFace;

    void Awake()
    {
        enemy = GetComponentInParent<EnemySpikes>();
        hitbox = GetComponent<Collider2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Move_CHR player = other.GetComponentInParent<Move_CHR>();

        if (player == null)
            return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health == null || health.IsDead)
            return;

        // Se for a hitbox do rosto, verifica se o jogador
        // está caindo sobre ela.
        if (isFace)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

            bool stompedFromAbove =
                playerRb.linearVelocity.y <= 0.1f &&
                other.bounds.min.y >= hitbox.bounds.center.y - 0.05f;

            if (stompedFromAbove)
            {
                enemy.Die();
                player.BounceAfterStomp(enemy.StompForce);
                return;
            }
        }

        // Qualquer contato perigoso causa dano.
        health.TakeDamage(enemy.transform);
    }
}

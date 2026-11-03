
using UnityEngine;

public class BossAttackZone : MonoBehaviour
{
    [Header("Referência ao boss")]
    public BossController boss;

    private void OnTriggerStay2D(Collider2D other)
    {
        Move_CHR player = other.GetComponentInParent<Move_CHR>();

        if (player == null || boss == null)
            return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health == null || health.IsDead)
            return;

        health.TakeDamage(boss.transform);
    }
}


using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Move_CHR))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerCombat : MonoBehaviour
{
    private Rigidbody2D rb;
    private Move_CHR movement;
    private PlayerHealth playerHealth;
    private readonly HashSet<Inimigo> contactingEnemies = new HashSet<Inimigo>();
    private readonly HashSet<Inimigo> stompCandidates = new HashSet<Inimigo>();

    public float StompForce = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<Move_CHR>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void FixedUpdate()
    {
        if (playerHealth.IsDead || playerHealth.IsInvulnerable)
        {
            contactingEnemies.Clear();
            stompCandidates.Clear();
            return;
        }

        if (stompCandidates.Count > 0)
        {
            foreach (Inimigo enemy in stompCandidates)
                enemy.Die();

            movement.BounceAfterStomp(StompForce);
        }
        else if (contactingEnemies.Count > 0)
        {
            foreach (Inimigo enemy in contactingEnemies)
            {
                playerHealth.TakeDamage(enemy.transform);
                break;
            }
        }

        contactingEnemies.Clear();
        stompCandidates.Clear();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy"))
            return;

        Inimigo enemy = collision.gameObject.GetComponent<Inimigo>();

        if (enemy == null)
            return;

        contactingEnemies.Add(enemy);

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f && rb.velocity.y <= 0.1f)
            {
                stompCandidates.Add(enemy);
                return;
            }
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class CheckpointManager : MonoBehaviour
{
    private Vector3 checkpointPosition;
    private PlayerHealth playerHealth;
    private Inimigo[] enemies;
    private EnemySpikes[] spikeEnemies;
    private BossArenaController[] bossArenas;

    private void Awake()
    {
        checkpointPosition = transform.position;
        playerHealth = GetComponent<PlayerHealth>();
        enemies = FindObjectsOfType<Inimigo>();
        spikeEnemies = FindObjectsOfType<EnemySpikes>();
        bossArenas = FindObjectsOfType<BossArenaController>();
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
    }

    public void SetCheckpoint(Transform point)
    {
        if (point != null)
            SetCheckpoint(point.position);
    }

    public void Respawn()
    {
        playerHealth.RespawnAt(checkpointPosition);

        foreach (Inimigo enemy in enemies)
            enemy.ResetAfterRespawn();

        foreach (EnemySpikes enemy in spikeEnemies)
            enemy.ResetAfterRespawn();

        foreach (BossArenaController arena in bossArenas)
            arena.ResetAfterPlayerRespawn();
    }
}

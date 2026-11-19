using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class CheckpointManager : MonoBehaviour
{
    private Vector3 checkpointPosition;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        checkpointPosition = transform.position;
        playerHealth = GetComponent<PlayerHealth>();
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
    }
}

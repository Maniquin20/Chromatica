
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Move_CHR player = other.GetComponentInParent<Move_CHR>();

        if (player != null)
        {
            player.Die();
        }
    }
}

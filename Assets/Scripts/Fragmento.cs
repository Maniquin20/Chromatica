
using UnityEngine;

public class Fragmento : MonoBehaviour
{
    public ColorManager colorManager;
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        Move_CHR player = other.GetComponentInParent<Move_CHR>();
        if (player == null)
            return;

        collected = true;
        player.UnlockClimb();

        if (colorManager != null)
            colorManager.AtivarCor(player, transform.position);

        Destroy(gameObject);
    }
}

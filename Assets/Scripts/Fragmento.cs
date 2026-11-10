
using UnityEngine;

public class Fragmento : MonoBehaviour
{
    public ColorManager colorManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Move_CHR player = other.GetComponent<Move_CHR>();

            if (player != null)
            {
                player.UnlockClimb();

                colorManager.AtivarCor();

                Destroy(gameObject);
            }
        }
    }
}


using UnityEngine;

public class BossHeadHitbox : MonoBehaviour
{
    private BossController boss;

    void Awake()
    {
        boss = GetComponentInParent<BossController>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Move_CHR player = other.GetComponentInParent<Move_CHR>();

        if (player == null)
            return;

        boss.TryStomp(player, other);
    }
}

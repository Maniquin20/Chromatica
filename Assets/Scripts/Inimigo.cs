
using UnityEngine;

public class Inimigo : MonoBehaviour
{
    private Rigidbody2D rb;

    public float speed = 2f;

    private int direction = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocityX = direction * speed;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Ignora o jogador: ele nao deve inverter o inimigo
        if (collision.gameObject.CompareTag("Player"))
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                if (direction * contact.normal.x < -0.5f)
                {
                    direction *= -1;
                    break;
                }
            }
        }
    }

    // Funcao chamada quando o inimigo morre
    public void Die()
    {
        Destroy(gameObject);
    }
}

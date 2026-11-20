
using UnityEngine;

public class Inimigo : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 spawnPosition;
    private int initialDirection;

    public float speed = 2f;

    private int direction = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = transform.position;
        initialDirection = direction;
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(direction * speed, rb.velocity.y);
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
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    public void ResetAfterRespawn()
    {
        transform.position = spawnPosition;
        direction = initialDirection;
        rb.velocity = Vector2.zero;
        gameObject.SetActive(true);
    }
}

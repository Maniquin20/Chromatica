
using UnityEngine;

public class Move_CHR : MonoBehaviour
{
    private Rigidbody2D rb;

    private float moveX;
    private float moveY;

    private bool CanClimb = false;
    private bool IsGrounded;
    private bool TouchingWall;
    private bool jumpPressed;

    private float wallNormalX;
    private float originalGravity;
    private float wallJumpTimer;

    public float JumpForce = 3f;
    public float speed = 4f;
    public float ClimbSpeed = 3f;

    public float WallJumpHorizontal = 15f;
    public float WallJumpVertical = 10f;
    public float WallJumpLockTime = 0.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }
    }

    void FixedUpdate()
    {
        bool grounded = IsGrounded;
        bool wall = TouchingWall;
        float wallDirection = wallNormalX;

        IsGrounded = false;
        TouchingWall = false;

        // Temporizador do wall jump
        if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.fixedDeltaTime;
        }

        // WALL JUMP
        if (jumpPressed && CanClimb && wall)
        {
            rb.gravityScale = originalGravity;

            rb.linearVelocity = new Vector2(
                wallDirection * WallJumpHorizontal,
                WallJumpVertical
            );

            wallJumpTimer = WallJumpLockTime;
        }

        // ESCALADA
        else if (CanClimb && wall && wallJumpTimer <= 0)
        {
            rb.gravityScale = 0f;

            rb.linearVelocity = new Vector2(
                0f,
                moveY * ClimbSpeed
            );
        }

        // MOVIMENTAÇÃO NORMAL
        else
        {
            rb.gravityScale = originalGravity;

            rb.linearVelocityX = moveX * speed;
        }

        // PULO NORMAL
        if (jumpPressed && grounded && !wall)
        {
            rb.linearVelocityY = JumpForce;
        }

        jumpPressed = false;
    }

    public void UnlockClimb()
    {
        CanClimb = true;
        Debug.Log("Escalada desbloqueada!");
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Chao"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    IsGrounded = true;
                }
            }
        }

        if (collision.gameObject.CompareTag("ParedeEscalavel"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    TouchingWall = true;
                    wallNormalX = contact.normal.x;
                }
            }
        }
    }
}


using UnityEngine;

public class Move_CHR : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerHealth playerHealth;

    private float moveX;
    private float moveY;
    
    private bool controlsLocked;
    private bool CanClimb = false;
    private bool IsGrounded;
    private bool TouchingWall;
    private bool jumpPressed;
    private bool isKnockedBack;

    private float wallNormalX;
    private float originalGravity;
    private float wallJumpTimer;

    public float JumpForce = 3f;
    public float speed = 4f;
    public float ClimbSpeed = 3f;

    public float WallJumpHorizontal = 6f;
    public float WallJumpVertical = 4f;
    public float WallJumpLockTime = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();

        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        if (controlsLocked)
            {
                jumpPressed = false;
                return;
            }

        if (playerHealth != null && playerHealth.IsDead)
            return;

        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;
    }

    void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (controlsLocked)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        bool grounded = IsGrounded;
        bool wall = TouchingWall;
        float wallDirection = wallNormalX;

        IsGrounded = false;
        TouchingWall = false;

        if (wallJumpTimer > 0)
            wallJumpTimer -= Time.fixedDeltaTime;

        // Bloqueio durante o knockback
        if (isKnockedBack)
        {
            if (grounded && rb.linearVelocity.y <= 0.1f)
            {
                isKnockedBack = false;
            }
            else
            {
                jumpPressed = false;
                return;
            }
        }

        // Wall jump
        if (jumpPressed && CanClimb && wall)
        {
            rb.gravityScale = originalGravity;

            rb.linearVelocity = new Vector2(
                wallDirection * WallJumpHorizontal,
                WallJumpVertical
            );

            wallJumpTimer = WallJumpLockTime;
        }

        // Escalada
        else if (CanClimb && wall && wallJumpTimer <= 0)
        {
            rb.gravityScale = 0f;

            rb.linearVelocity = new Vector2(
                0f,
                moveY * ClimbSpeed
            );
        }

        // Movimentação normal
        else
        {
            rb.gravityScale = originalGravity;
            rb.linearVelocityX = moveX * speed;
        }

        // Pulo normal
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

    // Recebe o impulso do knockback
    public void ApplyKnockback(
        Transform enemy,
        float horizontalForce,
        float verticalForce
    )
    {
        rb.gravityScale = originalGravity;

        float direction = Mathf.Sign(
            transform.position.x - enemy.position.x
        );

        rb.linearVelocity = new Vector2(
            direction * horizontalForce,
            verticalForce
        );

        isKnockedBack = true;
        wallJumpTimer = WallJumpLockTime;
    }

    // Impulso ao derrotar um inimigo pisando nele
    public void BounceAfterStomp(float force)
    {
        rb.gravityScale = originalGravity;
        rb.linearVelocityY = force;
    }

    // Mantém compatibilidade com o DeathZone antigo
    public void Die()
    {
        if (playerHealth != null)
            playerHealth.Die();
    }

    // Bloqueia ou desbloqueia o controle do jogador
    public void SetControlLocked(bool locked)
    {
        controlsLocked = locked;

        if (locked)
        {
            rb.linearVelocity = Vector2.zero;
            jumpPressed = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (collision.gameObject.CompareTag("Chao"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                    IsGrounded = true;
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

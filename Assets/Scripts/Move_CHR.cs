
using UnityEngine;

public class Move_CHR : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private PlayerHealth playerHealth;
    private int groundLayerMask;

    private float moveX;
    private float moveY;

    private bool controlsLocked;
    private bool CanClimb = false;
    private bool IsGrounded;
    private bool TouchingWall;
    private float jumpBufferTimer;
    private bool jumpHeld;
    private bool jumpReleased;
    private bool isKnockedBack;
    private bool stompBounceActive;
    private bool canFastFall;
    private float fastFallTimer;

    private float wallNormalX;
    private float originalGravity;
    private float wallJumpTimer;

    public float JumpForce = 3f;
    public float speed = 4f;
    public float ClimbSpeed = 3f;

    public float WallJumpHorizontal = 6f;
    public float WallJumpVertical = 4f;
    public float WallJumpLockTime = 0.2f;

    // NOVAS CONFIGURAÇÕES
    public float jumpCutMultiplier = 0.5f;
    public float fastFallGravityMultiplier = 2.5f;
    public float maxFallSpeed = 12f;
    [Min(0f)] public float FastFallDelay = 0.1f;
    public float JumpBufferTime = 0.12f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerHealth = GetComponent<PlayerHealth>();
        groundLayerMask = LayerMask.GetMask("Ground");

        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        if (controlsLocked)
        {
            jumpHeld = false;
            jumpReleased = false;
            return;
        }

        if (playerHealth != null && playerHealth.IsDead)
            return;

        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = JumpBufferTime;
        }
        else if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        // NOVO: detecta se o jogador está segurando ou soltando espaço
        jumpHeld = Input.GetButton("Jump");

        if (Input.GetButtonUp("Jump"))
            jumpReleased = true;
    }

    void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (controlsLocked)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        IsGrounded = CheckGrounded();
        bool grounded = IsGrounded;
        bool wall = TouchingWall;
        float wallDirection = wallNormalX;

        TouchingWall = false;

        if (wallJumpTimer > 0)
            wallJumpTimer -= Time.fixedDeltaTime;

        if (grounded)
        {
            canFastFall = false;
            fastFallTimer = 0f;
        }
        else if (canFastFall)
        {
            fastFallTimer += Time.fixedDeltaTime;
        }

        // Bloqueio durante o knockback
        if (isKnockedBack)
        {
            if (grounded && rb.velocity.y <= 0.1f)
            {
                isKnockedBack = false;
            }
            else
            {
                jumpBufferTimer = 0f;
                jumpReleased = false;
                return;
            }
        }

        // Wall jump
        if (jumpBufferTimer > 0f && CanClimb && wall)
        {
            rb.gravityScale = originalGravity;

            rb.velocity = new Vector2(
                wallDirection * WallJumpHorizontal,
                WallJumpVertical
            );

            wallJumpTimer = WallJumpLockTime;
            jumpBufferTimer = 0f;
            canFastFall = true;
            fastFallTimer = 0f;
            playerHealth?.PlayJumpSound();
        }

        // Escalada
        else if (CanClimb && wall && wallJumpTimer <= 0)
        {
            rb.gravityScale = 0f;

            rb.velocity = new Vector2(
                0f,
                moveY * ClimbSpeed
            );
        }

        // Movimentação normal
        else
        {
            rb.gravityScale = originalGravity;
            rb.velocity = new Vector2(moveX * speed, rb.velocity.y);
        }

        // Pulo normal
        if (jumpBufferTimer > 0f && grounded && (!wall || !CanClimb))
        {
            rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            jumpBufferTimer = 0f;
            canFastFall = true;
            fastFallTimer = 0f;
            playerHealth?.PlayJumpSound();
        }

        // NOVO: soltar espaço reduz a altura do pulo
        if (jumpReleased && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }

        // Queda rápida ao segurar S

       // Protege o impulso do stomp até o personagem atingir o ponto mais alto
        if (stompBounceActive)
        {
            if (rb.velocity.y <= 0f)
                stompBounceActive = false;
        }

        // Queda rápida
        if (!grounded && !wall && canFastFall && fastFallTimer >= FastFallDelay &&
            moveY < -0.5f && !stompBounceActive)
        {
            rb.gravityScale = originalGravity * fastFallGravityMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(
                rb.velocity.y,
                -maxFallSpeed
            ));
        }

        jumpReleased = false;
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

        rb.velocity = new Vector2(
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
        rb.velocity = new Vector2(rb.velocity.x, force);

        stompBounceActive = true;
        canFastFall = true;
        fastFallTimer = 0f;
    }

    public void ResetAfterRespawn(Vector3 position)
    {
        rb.position = position;
        rb.velocity = Vector2.zero;
        rb.gravityScale = originalGravity;
        controlsLocked = false;
        moveX = 0f;
        moveY = 0f;
        IsGrounded = false;
        TouchingWall = false;
        wallNormalX = 0f;
        wallJumpTimer = 0f;
        jumpBufferTimer = 0f;
        jumpHeld = false;
        jumpReleased = false;
        isKnockedBack = false;
        stompBounceActive = false;
        canFastFall = false;
        fastFallTimer = 0f;
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
            rb.velocity = Vector2.zero;
            jumpHeld = false;
            jumpReleased = false;
            jumpBufferTimer = 0f;
            canFastFall = false;
            fastFallTimer = 0f;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateContacts(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateContacts(collision);
    }

    private void UpdateContacts(Collision2D collision)
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

    private bool CheckGrounded()
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 checkSize = new Vector2(Mathf.Max(bounds.size.x - 0.05f, 0.01f), 0.06f);
        Vector2 checkPosition = new Vector2(bounds.center.x, bounds.min.y - 0.02f);
        return Physics2D.OverlapBox(checkPosition, checkSize, 0f, groundLayerMask) != null;
    }
}

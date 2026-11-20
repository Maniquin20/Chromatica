
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Move_CHR : MonoBehaviour
{
    private const float WallContactGraceDuration = 0.04f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private PlayerHealth playerHealth;
    private int groundLayerMask;

    private float moveX;
    private float moveY;

    private bool controlsLocked;
    private bool CanClimb = false;
    private bool TouchingWall;
    private float jumpBufferTimer;
    private bool jumpHeld;
    private bool jumpReleased;
    private bool isKnockedBack;
    private bool stompBounceActive;
    private bool jumpCutAvailable;
    private bool wallJumpNeedsWallSeparation;
    private bool canFastFall;
    private float fastFallTimer;
    private float coyoteTimer;

    private float wallNormalX;
    private float originalGravity;
    private float wallJumpTimer;
    private float wallJumpDirectionLockTimer;
    private float wallJumpDirection;
    private float wallIdleTimer;
    private float wallContactGraceTimer;

    [Header("Movimento horizontal")]
    [FormerlySerializedAs("speed"), Min(0f)] public float MaxHorizontalSpeed = 5.75f;
    [Min(0f)] public float GroundAcceleration = 100f;
    [Min(0f)] public float GroundDeceleration = 150f;
    [Min(0f)] public float AirAcceleration = 70f;
    [Min(0f)] public float AirDeceleration = 60f;

    [Header("Pulo")]
    [Min(0f)] public float JumpForce = 7f;
    [FormerlySerializedAs("jumpCutMultiplier"), Range(0.1f, 1f)] public float VariableJumpMultiplier = 0.5f;
    [Min(0f)] public float CoyoteTime = 0.1f;
    [Min(0f)] public float JumpBufferTime = 0.12f;

    [Header("Parede")]
    [Min(0f)] public float ClimbSpeed = 3f;
    [Min(0f)] public float WallSlideSpeed = 2.5f;
    [Min(0f)] public float WallSlideDelay = 0.5f;

    [Min(0f)] public float WallJumpHorizontal = 7f;
    [Min(0f)] public float WallJumpVertical = 5.5f;
    [Min(0f)] public float WallJumpLockTime = 0.5f;
    [Min(0f)] public float WallJumpDirectionLockTime = 0.12f;

    [Header("Gravidade")]
    [Min(0.01f)] public float RisingGravityMultiplier = 1f;
    [Min(0.01f)] public float FallingGravityMultiplier = 1.8f;
    [FormerlySerializedAs("fastFallGravityMultiplier"), Min(0.01f)] public float FastFallGravityMultiplier = 2.5f;
    [FormerlySerializedAs("maxFallSpeed"), Min(0.01f)] public float MaxFallSpeed = 12f;
    [Min(0f)] public float FastFallDelay = 0.05f;

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

        // O probe pode ainda encostar no chão no primeiro passo da subida.
        bool grounded = rb.velocity.y <= 0.1f && CheckGrounded();
        if (TouchingWall)
            wallContactGraceTimer = WallContactGraceDuration;
        else
            wallContactGraceTimer = Mathf.Max(
                0f,
                wallContactGraceTimer - Time.fixedDeltaTime
            );

        bool wall = TouchingWall || wallContactGraceTimer > 0f;
        float wallDirection = wallNormalX;
        bool movingAwayFromWall = wall && moveX * wallDirection > 0.01f;

        TouchingWall = false;
        if (!wall)
            wallJumpNeedsWallSeparation = false;
        if (!wall || movingAwayFromWall)
        {
            wallIdleTimer = 0f;
            if (movingAwayFromWall)
                wallContactGraceTimer = 0f;
        }

        if (wallJumpTimer > 0)
            wallJumpTimer -= Time.fixedDeltaTime;
        if (wallJumpDirectionLockTimer > 0f)
            wallJumpDirectionLockTimer = Mathf.Max(
                0f,
                wallJumpDirectionLockTimer - Time.fixedDeltaTime
            );

        if (grounded)
        {
            coyoteTimer = CoyoteTime;
            canFastFall = false;
            fastFallTimer = 0f;
        }
        else
        {
            coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.fixedDeltaTime);
            if (canFastFall)
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

        bool climbing = false;
        bool wallSliding = false;
        bool jumpedThisStep = false;

        // Wall jump
        if (jumpBufferTimer > 0f && CanClimb && wall &&
            !wallJumpNeedsWallSeparation)
        {
            rb.velocity = new Vector2(
                wallDirection * WallJumpHorizontal,
                WallJumpVertical
            );

            wallJumpTimer = WallJumpLockTime;
            wallJumpDirectionLockTimer = WallJumpDirectionLockTime;
            wallJumpDirection = wallDirection;
            wallJumpNeedsWallSeparation = true;
            jumpBufferTimer = 0f;
            jumpedThisStep = true;
        }

        // Escalada
        else if (CanClimb && wall && wallJumpTimer <= 0 && !movingAwayFromWall)
        {
            if (moveY > 0.1f || moveY < -0.1f)
            {
                wallIdleTimer = 0f;
                climbing = true;
                rb.gravityScale = 0f;
                rb.velocity = new Vector2(0f, Mathf.Sign(moveY) * ClimbSpeed);
            }
            else if (wallIdleTimer < WallSlideDelay)
            {
                wallIdleTimer += Time.fixedDeltaTime;
                climbing = true;
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
            }
            else
            {
                wallSliding = true;
                rb.gravityScale = 0f;
                float slideSpeed = Mathf.Min(WallSlideSpeed, MaxFallSpeed);
                rb.velocity = new Vector2(0f, -slideSpeed);
            }
        }

        // Movimentação normal
        else
        {
            ApplyHorizontalMovement(grounded);
        }

        // Pulo normal ou coyote jump.
        if (!jumpedThisStep && jumpBufferTimer > 0f &&
            (grounded || coyoteTimer > 0f) &&
            (!wall || !CanClimb || (grounded && wallJumpNeedsWallSeparation)))
        {
            rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            jumpBufferTimer = 0f;
            jumpedThisStep = true;
        }

        if (jumpedThisStep)
        {
            wallIdleTimer = 0f;
            coyoteTimer = 0f;
            canFastFall = true;
            fastFallTimer = 0f;
            jumpCutAvailable = jumpHeld;
            if (!jumpHeld)
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * VariableJumpMultiplier);
            playerHealth?.PlayJumpSound();
        }

        if (jumpReleased)
        {
            if (jumpCutAvailable && !climbing && rb.velocity.y > 0f)
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * VariableJumpMultiplier);
            jumpCutAvailable = false;
        }

        // Protege o impulso do stomp até o personagem atingir o ponto mais alto.
        if (stompBounceActive)
        {
            if (rb.velocity.y <= 0f)
                stompBounceActive = false;
        }

        ApplyGravity(grounded, climbing, wall, wallSliding);

        jumpReleased = false;
    }

    private void ApplyHorizontalMovement(bool grounded)
    {
        float horizontalInput = moveX;
        if (wallJumpDirectionLockTimer > 0f && horizontalInput * wallJumpDirection < 0f)
            horizontalInput = 0f;

        bool hasHorizontalInput = Mathf.Abs(horizontalInput) > 0.01f;
        float acceleration = grounded
            ? hasHorizontalInput ? GroundAcceleration : GroundDeceleration
            : hasHorizontalInput ? AirAcceleration : AirDeceleration;
        float targetSpeed = horizontalInput * MaxHorizontalSpeed;
        float horizontalSpeed = Mathf.MoveTowards(
            rb.velocity.x,
            targetSpeed,
            acceleration * Time.fixedDeltaTime
        );

        rb.velocity = new Vector2(horizontalSpeed, rb.velocity.y);
    }

    private void ApplyGravity(bool grounded, bool climbing, bool wall, bool wallSliding)
    {
        if (climbing || wallSliding)
            return;

        bool fastFalling = !grounded && !wall && canFastFall &&
            fastFallTimer >= FastFallDelay && moveY < -0.5f && !stompBounceActive;

        float gravityMultiplier = rb.velocity.y > 0f
            ? RisingGravityMultiplier
            : FallingGravityMultiplier;
        if (grounded && rb.velocity.y <= 0f)
            gravityMultiplier = 1f;
        if (fastFalling)
            gravityMultiplier = FastFallGravityMultiplier;

        rb.gravityScale = originalGravity * gravityMultiplier;
        float fallSpeedCap = wallSliding
            ? Mathf.Min(MaxFallSpeed, WallSlideSpeed)
            : MaxFallSpeed;
        if (rb.velocity.y < -fallSpeedCap)
            rb.velocity = new Vector2(rb.velocity.x, -fallSpeedCap);
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
        wallJumpDirectionLockTimer = 0f;
        wallJumpNeedsWallSeparation = false;
        wallIdleTimer = 0f;
        wallContactGraceTimer = 0f;
        coyoteTimer = 0f;
        jumpCutAvailable = false;
    }

    // Impulso ao derrotar um inimigo pisando nele
    public void BounceAfterStomp(float force)
    {
        rb.gravityScale = originalGravity;
        rb.velocity = new Vector2(rb.velocity.x, force);

        stompBounceActive = true;
        canFastFall = true;
        fastFallTimer = 0f;
        wallJumpDirectionLockTimer = 0f;
        wallJumpNeedsWallSeparation = false;
        wallIdleTimer = 0f;
        wallContactGraceTimer = 0f;
        coyoteTimer = 0f;
        jumpCutAvailable = false;
    }

    public void ResetAfterRespawn(Vector3 position)
    {
        rb.position = position;
        rb.velocity = Vector2.zero;
        rb.gravityScale = originalGravity;
        controlsLocked = false;
        moveX = 0f;
        moveY = 0f;
        coyoteTimer = 0f;
        TouchingWall = false;
        wallNormalX = 0f;
        wallJumpTimer = 0f;
        wallJumpDirectionLockTimer = 0f;
        wallJumpDirection = 0f;
        wallJumpNeedsWallSeparation = false;
        wallIdleTimer = 0f;
        wallContactGraceTimer = 0f;
        jumpBufferTimer = 0f;
        jumpHeld = false;
        jumpReleased = false;
        jumpCutAvailable = false;
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
            wallJumpTimer = 0f;
            wallJumpDirection = 0f;
            wallJumpNeedsWallSeparation = false;
            wallIdleTimer = 0f;
            wallContactGraceTimer = 0f;
            jumpHeld = false;
            jumpReleased = false;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            wallJumpDirectionLockTimer = 0f;
            jumpCutAvailable = false;
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

        if (collision.gameObject.CompareTag("ParedeEscalavel"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    float preferredDirection = Mathf.Abs(moveX) > 0.01f
                        ? -Mathf.Sign(moveX)
                        : 1f;
                    if (!TouchingWall ||
                        contact.normal.x * preferredDirection > wallNormalX * preferredDirection)
                        wallNormalX = contact.normal.x;

                    TouchingWall = true;
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

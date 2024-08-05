using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D cd;

    private bool canBeControlled = false;

    [Header("Movement detals")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private float defaultGravityScale;
    private bool canDoubleJump;

    [Header("buffer &Coyote Jump")]
    [SerializeField] private float bufferJumpWindow = .25f;
    private float bufferJumpActivated = -1;
    [SerializeField] private float coyoteJumpWindow = .5f;
    private float coyoteJumpActivated = -1;

    [Header("wall interactions")]
    [SerializeField] private float walJumpDuration = .6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 1;
    [SerializeField] private Vector2 knockbackPower;
    private bool isKnocked;

    [Header("Collison info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGround;
    private bool isAirborne;
    private bool isWallDetected;

    private float xInput;
    private float yInput;

    private bool faceRight = true;
    private int facingDir = 1;

    [Header("VFX")]
    [SerializeField] private GameObject deathVfx;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CapsuleCollider2D>();
        anim = GetComponentInChildren<Animator>();

    }

    private void Start()
    {
        defaultGravityScale = rb.gravityScale;
        RespawnFinished(false);
    }

    private void Update()
    {
        UpdateAirbornStatus();

        if (canBeControlled == false)
        {
            handleCollision();
            HandleAnimations();
            return;
        }

        if (isKnocked)
            return;

        handleInput();
        HandledWallSlide();
        HandleMovement();
        HandleFlip();
        handleCollision();
        HandleAnimations();

    }

    public void RespawnFinished(bool finishied)
    {
        
        if (finishied)
        {
            rb.gravityScale = defaultGravityScale;
            canBeControlled = true;
            cd.enabled = true;
        }
        else
        {
            rb.gravityScale = 0;
            canBeControlled = false;
            cd.enabled = false;
        }
    }

    public void Knockback(float sourceDamageXposition)
    {
        float knockbackDir = 1;

        if (transform.position.x < sourceDamageXposition)
            knockbackDir = -1;

        if (isKnocked)
            return;

        StartCoroutine(KockbackRoutine());
        rb.velocity = new Vector2(knockbackPower.x * knockbackDir, knockbackPower.y);
    }
    private IEnumerator KockbackRoutine()
    {
        isKnocked = true;
        anim.SetBool("isKocked", true);

        yield return new WaitForSeconds(knockbackDuration);

        isKnocked = false;
        anim.SetBool("isKocked", false);

    }

    public void Die()
    {
        GameObject newDeathVfx = Instantiate(deathVfx, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }


    public void Push(Vector2 direction,float duration = 0)
    {
        StartCoroutine(PushCouroutine(direction, duration));
    }

    private IEnumerator PushCouroutine(Vector2 direction,float duration)
    {
        canBeControlled = false;

        rb.velocity = Vector2.zero;
        rb.AddForce(direction, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        canBeControlled = true;
    }


    private void UpdateAirbornStatus()
    {
        if (isGround && isAirborne)
            HandleLanding();
        if (!isGround && !isAirborne)
            BecomeAirborn();
    }

    private void BecomeAirborn()
    {
        isAirborne = true;
        if (rb.velocity.y < 0)
            ActivateCoyoteJump();
    }

    private void HandleLanding()
    {
        isAirborne = false;
        canDoubleJump = true;

        AttemptBufferJump();
    }

    private void handleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpButton();
            RequestBufferJump();
        }

    }
    #region Buffer Coyote Jump
    private void RequestBufferJump()
    {
        if (isAirborne)
            bufferJumpActivated = Time.time;
    }
    private void AttemptBufferJump()
    {
        if (Time.time < bufferJumpActivated + bufferJumpWindow)
        {
            bufferJumpActivated = Time.time - 1;
            jump();
        }
    }
    private void ActivateCoyoteJump() => coyoteJumpActivated = Time.time;
    private void CancelCoyoteJump() => coyoteJumpActivated = Time.time - 1;
    #endregion

    private void JumpButton()
    {
        bool coyoteJumpAvalible = Time.time < coyoteJumpActivated + coyoteJumpWindow;
        if (isGround || coyoteJumpAvalible)
        {
            jump();
        }
        else if (isWallDetected && !isGround)
        {
            WallJump();
        }
        else if (canDoubleJump)
        {
            DoubleJump();
        }
        CancelCoyoteJump();

    }

    //简单表达式，函数只有一条语句用 => 可以不需要大括号
    private void jump() => rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    private void DoubleJump()
    {
        isWallJumping = false;
        canDoubleJump = false;
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
    }

    private void WallJump()
    {
        canDoubleJump = true;
        rb.velocity = new Vector2(wallJumpForce.x * -facingDir, wallJumpForce.y);
        Flip();
        StopAllCoroutines();
        StartCoroutine(WallJumpRoutine());
    }

    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(walJumpDuration);
        isWallJumping = false;
    }
    private void HandledWallSlide()
    {
        bool canWalSlide = isWallDetected && rb.velocity.y < 0;
        float yModifer = yInput < 0 ? 1 : .05f;

        if (canWalSlide == false)
            return;

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifer);//0.5倍

    }



    private void handleCollision()
    {
        isGround = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    private void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGround);
        anim.SetBool("isWallDeteced", isWallDetected);
    }

    private void HandleMovement()
    {
        if (isWallDetected)
            return;
        if (isWallJumping)
            return;

        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
    }

    private void HandleFlip()
    {
        if (xInput < 0 && faceRight || xInput > 0 && !faceRight)
            Flip();
    }

    private void Flip()
    {
        facingDir = facingDir * -1;
        transform.Rotate(0, 180, 0);
        faceRight = !faceRight;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDir), transform.position.y));

    }
}

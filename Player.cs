using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement detals")]
    [SerializeField]private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private bool canDoubleJump;

    [Header("wall interactions")]
    [SerializeField] private float walJumpDuration = .6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 1;
    [SerializeField] private Vector2 knockbackPower;
    private bool isKnocked;
    private bool canBeKnocked;

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
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren < Animator > ();

    }


    private void Update()
    {
        

        UpdateAirbornStatus();

        if (isKnocked)
            return;
       
        handleInput();
        HandledWallSlide();
        HandleMovement();
        HandleFlip();
        handleCollision();
        HandleAnimations();

    }

    public void Knockback()
    {
        if (isKnocked)
            return;

        StartCoroutine(KockbackRoutine());
        anim.SetTrigger("Knockback");
        rb.velocity = new Vector2(knockbackPower.x * -facingDir, knockbackPower.y);
    }

    private void HandledWallSlide()
    {
        bool canWalSlide = isWallDetected && rb.velocity.y < 0;
        float yModifer = yInput < 0 ? 1 : .05f;

        if (canWalSlide == false)
            return;

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifer);//0.5倍
        
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
    }

    private void HandleLanding()
    {
        isAirborne = false;
        canDoubleJump = true;
    }

    private void handleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.Space))
            JumpButton();//简单表达式，if只有一条语句紧跟在下一行，不需要大括号
        
    }

    private void JumpButton()
    {
        if (isGround)
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

    private IEnumerator KockbackRoutine() 
    {
        canBeKnocked = false;
        isKnocked = true;

        yield return new WaitForSeconds(knockbackDuration);

        canBeKnocked = true;
        isKnocked = false;
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

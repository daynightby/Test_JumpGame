using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Collison info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGround;
    private bool isAirborne;


    private float xInpt;
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
        handleCollision();
        handleInput();
        HandleMovement();
        HandleFlip();
        HandleAnimations();

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
        xInpt = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space))
            JumpButton();//简单表达式，if只有一条语句紧跟在下一行，不需要大括号
        
    }

    private void JumpButton()
    {
        if (isGround)
        {
            jump(); 
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
        canDoubleJump = false;
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
    }

    private void handleCollision()
    {
        isGround = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    private void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGround);
    }

    private void HandleMovement()
    {
        rb.velocity = new Vector2(xInpt * moveSpeed, rb.velocity.y);
    }

    private void HandleFlip()
    {
        if (rb.velocity.x < 0 && faceRight || rb.velocity.x > 0 && !faceRight)
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
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private GameObject jumpDirectionIndicator;
    [SerializeField] private SpriteRenderer triangle;
    [SerializeField] private Color indicatorColor;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxTime;
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private float rotSpeed;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float swaySpeed;
    [SerializeField] private float angleSway;
    private Vector2 jumpDirection;
    private bool isWalkingRight;
    private bool isWalkingLeft;
    private CountdownTimer jumpTimer;
    

    void OnEnable()
    {
        playerInput.Jump += HandleJump;
        playerInput.RightWalk += HandleWalkRight;
        playerInput.LeftWalk += HandleWalkLeft;
    }

    void OnDisable()
    {
        playerInput.Jump -= HandleJump;
        playerInput.RightWalk -= HandleWalkRight;
        playerInput.LeftWalk -= HandleWalkLeft;
    }


    void Start()
    {
        playerInput.EnablePlayerActions();
        jumpTimer = new CountdownTimer(maxTime);
        jumpTimer.OnTimerStart += HandleJumpStart;
        jumpTimer.OnTimerStop += HandleJumpStop;

        jumpDirection = Vector2.right;
    }


    void Update()
    {
        jumpTimer.Tick(Time.deltaTime);
        if(groundChecker.IsGroundedPrime && playerAnimation.playerState != PlayerState.PREPJUMP)
        {
            playerAnimation.playerState = PlayerState.IDLE;
        }
    }

    private void FixedUpdate()
    {
        JumpDirectionChange();
        if(rb.velocity.y < -0.3) { playerAnimation.playerState = PlayerState.FALL;}
        if (rb.velocity.y > 0.3) { playerAnimation.playerState = PlayerState.JUMP; }
        if (!groundChecker.CanWalkPrime) return;
        WalkRight();
        WalkLeft();
    }



    private void JumpDirectionChange()
    {

        float swayAngle = Mathf.Sin(Time.time * swaySpeed) * angleSway;

        jumpDirection = Quaternion.Euler(0,0,swayAngle) * Vector2.up;

        jumpDirectionIndicator.transform.up = jumpDirection;

    }


    void Jump()
    {
        if (!groundChecker.IsGroundedPrime) return;
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
        soundManager.PlayJumpSound();
    }
    

    void HandleJump(bool performed)
    {        
        if (performed)
        {
            jumpTimer.Start();
            playerAnimation.playerState = PlayerState.PREPJUMP;
        }
        else if(!performed)
        {
            jumpTimer.Stop();
        }
    }

    void HandleJumpStart()
    {
        Color color = triangle.color;
        color.a = 1;
        triangle.color = color;
    }

    void HandleJumpStop()
    {
        Jump();
        Color color = triangle.color;
        color.a = 0;
        triangle.color = color;
    }

    void WalkRight()
    {
        if (!isWalkingRight) return;
        rb.velocity = new Vector2 (movementSpeed, rb.velocity.y);
    }

    void WalkLeft()
    {
        if(!isWalkingLeft)return;
        rb.velocity = new Vector2(-movementSpeed, rb.velocity.y);
    }

    void HandleWalkRight(bool performed)
    {
        if (performed)
        {
            isWalkingRight = true;
        }
        else if (!performed)
        {
            isWalkingRight = false;
            
        }
    }

    void HandleWalkLeft(bool performed)
    {
        if (performed)
        {
            isWalkingLeft = true;
        }
        else if (!performed)
        {
            isWalkingLeft = false;
            
        }
    }

}

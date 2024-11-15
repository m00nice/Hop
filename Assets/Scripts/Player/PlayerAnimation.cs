using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite idle0;
    [SerializeField] private Sprite prepJump0;
    [SerializeField] private Sprite jump;
    [SerializeField] private Sprite fall;
    [SerializeField] private RuntimeAnimatorController idleAnimation;
    [SerializeField] private RuntimeAnimatorController prepJumpAnimation;

    public PlayerState playerState;

    private void Start()
    {
        playerState = PlayerState.IDLE;
    }

    private void Update()
    {
        CheckState();
    }

    private void CheckState()
    {
        if (playerState == PlayerState.IDLE)
        {
            spriteRenderer.sprite = idle0;
            animator.runtimeAnimatorController = idleAnimation;
            return;
        }

        if (playerState == PlayerState.PREPJUMP)
        {
            spriteRenderer.sprite = prepJump0;
            animator.runtimeAnimatorController = prepJumpAnimation;
            return;
        }

        if (playerState == PlayerState.JUMP)
        {
            spriteRenderer.sprite = jump;
            animator.runtimeAnimatorController = null;
            return;
        }

        if (playerState == PlayerState.FALL)
        {
            spriteRenderer.sprite = fall;
            animator.runtimeAnimatorController = null;
            return;
        }
    }


}

public enum PlayerState
{
    IDLE,
    PREPJUMP,
    JUMP,
    FALL,
}

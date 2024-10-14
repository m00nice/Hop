using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float topThreshold;
    public bool IsGroundedPrime { get; private set; }
    private bool IsGrounded1;
    public bool CanWalkPrime { get; private set; }
    private bool CanWalk1;

    private void Update()
    {
        IsGroundedPrime = (RaycastGroundCheck() || IsGrounded1 || OverlapCircleGroundCheck());
        CanWalkPrime = (RaycastGroundCheck() || CanWalk1 || OverlapCircleGroundCheck());
        //Debug.Log("IsGrounded: " + IsGroundedPrime + "   CanWalk: " + CanWalkPrime);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {                
                if (contact.normal.y > topThreshold)
                {
                    IsGrounded1 = true;
                    CanWalk1 = true;
                }
            }
            
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Sticky"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > topThreshold)
                {
                    IsGrounded1 = true;
                    CanWalk1 = false;
                }
            }

        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Moving"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > topThreshold)
                {
                    IsGrounded1 = true;
                    CanWalk1 = true;
                    transform.parent = collision.transform;
                }
            }

        }


    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IsGrounded1 = false;
            CanWalk1 = false;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Sticky"))
        {
            
             IsGrounded1 = false;
             CanWalk1 = false;
                
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Moving"))
        {
            
             IsGrounded1 = false;
            CanWalk1 = false;
            transform.SetParent(null);
            transform.SetSiblingIndex(0);

        }
    }


    private bool RaycastGroundCheck()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, transform.localScale.y/2 + 0.1f, groundLayer);
    }


    private bool OverlapCircleGroundCheck()
    {
        return Physics2D.OverlapCircle(transform.position, transform.localScale.y / 2 + 0.1f, groundLayer);

    }




}

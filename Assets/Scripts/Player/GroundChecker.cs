using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float topThreshold;
    public bool IsGrounded { get; private set; }
    public bool CanWalk { get; private set; }

    private void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {                
                if (contact.normal.y > topThreshold)
                {
                    IsGrounded = true;
                    CanWalk = true;
                }
            }
            
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Sticky"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > topThreshold)
                {
                    IsGrounded = true;
                    CanWalk = false;
                }
            }

        }


    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IsGrounded = false;
            CanWalk = false;
        }
    }


}

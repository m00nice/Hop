using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Camera m_Camera;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float topThreshold;

    private bool playerHasTouched;
    private bool playerInView;


    private void OnEnable()
    {
        playerInput.Jump += OnJump;
    }

    private void OnDisable()
    {
        playerInput.Jump -= OnJump;
    }


    private void Update()
    {
        if(playerMovement.transform.position.y < transform.position.y)
        {
            boxCollider.enabled = true;
            spriteRenderer.enabled = true;

            if (playerInView)
            {
                m_Camera.transform.position = new Vector3(m_Camera.transform.position.x, m_Camera.transform.position.y - 10.0f, -10.0f);
                playerInView = false;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -topThreshold)
                {
                    playerHasTouched = true;
                    if (playerInView) return;
                    m_Camera.transform.position = new Vector3(m_Camera.transform.position.x, m_Camera.transform.position.y + 10.0f, -10.0f);
                    playerInView = true;
                }
            }
        }
    }

    private void OnJump(bool performed)
    {
        if (!playerHasTouched) return;
        if (performed)return;
        boxCollider.enabled = false;
        spriteRenderer.enabled = false;
        playerHasTouched = false;
    }


}

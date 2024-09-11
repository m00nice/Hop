using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private float disappearTime;
    [SerializeField] private float reappearTime;
    [SerializeField] private float topThreshold;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -topThreshold)
                {
                    Debug.Log("Hit the top of the platform!");
                    StartCoroutine(DisappearTimer());
                }
            }
        }
    }

    IEnumerator DisappearTimer()
    {
        yield return new WaitForSeconds(disappearTime);
        spriteRenderer.enabled = false;
        boxCollider2D.enabled = false;
        yield return new WaitForSeconds(reappearTime);
        spriteRenderer.enabled = true;
        boxCollider2D.enabled = true;
    }
}

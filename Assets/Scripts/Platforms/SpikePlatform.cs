using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikePlatform : MonoBehaviour
{

    [SerializeField] private float spikeTime;
    [SerializeField] private float untilSpikeTime;
    [SerializeField] private float topThreshold;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -topThreshold)
                {

                }
            }
        }
    }
}

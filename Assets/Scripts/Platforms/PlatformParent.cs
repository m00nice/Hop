using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlatformParent : MonoBehaviour
{


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            transform.DOShakePosition(0.7f, new Vector3(0.01f,0.01f,0.0f), 1, 90f);
        }
    }


}

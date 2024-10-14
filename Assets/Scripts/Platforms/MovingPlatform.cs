using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 destination;
    private Vector3 startPosition;
    [SerializeField] private float duration;

    private void Start()
    {
        transform.DOLocalMove(destination, duration)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
        
    }

    
}

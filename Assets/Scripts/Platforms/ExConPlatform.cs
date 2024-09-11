using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ExConPlatform : MonoBehaviour
{

    [SerializeField] private Vector2 maxSize;
    private Vector2 minSize;
    [SerializeField] private float duration;
    [SerializeField] private Ease easeType;

    private bool isExpanding;

    private void Start()
    {
        minSize = transform.localScale;

        ExpandAndContract();
    }

    void ExpandAndContract()
    {
        transform.DOScale(maxSize, duration)
            .SetEase(easeType)
            .OnComplete(() =>
            {

                transform.DOScale(minSize, duration)
                    .SetEase(easeType);
            })
            .SetLoops(-1, LoopType.Yoyo);
    }

    
}

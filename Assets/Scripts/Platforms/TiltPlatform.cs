using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TiltPlatform : MonoBehaviour
{
    [SerializeField] private float tiltAngle;
    [SerializeField] private float duration;

    void Start()
    {
        Rotate();
    }

    void Rotate()
    {
        transform.DORotate(new Vector3(0, 0, tiltAngle), duration)
                 .SetEase(Ease.InOutSine)
                 .OnComplete(() => {
                     transform.DORotate(new Vector3(0, 0, -tiltAngle), duration)
                 .SetEase(Ease.InOutSine)
                 .OnComplete(() => Rotate());
                 });
    }
}

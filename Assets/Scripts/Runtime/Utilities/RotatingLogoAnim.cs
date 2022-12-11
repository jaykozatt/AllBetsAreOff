using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace AllBets
{
    public class RotatingLogoAnim : MonoBehaviour
    {
        public RectTransform chainDie;
        public RectTransform die;

        Tween orbitalRotation;
        Tween dieRotation;

        private void OnDisable()
        {
            orbitalRotation?.Kill();
            dieRotation?.Kill();
        }

        private void OnEnable() 
        {
            Vector3 fullRotation = Vector3.forward * 360;
            orbitalRotation = chainDie.DOLocalRotate(fullRotation, .75f, RotateMode.LocalAxisAdd)
                .SetLoops(-1)
                .SetEase(Ease.Linear)
            ;
            
            dieRotation = die.DOLocalRotate(fullRotation, .75f, RotateMode.LocalAxisAdd)
                .SetLoops(-1)
                .SetEase(Ease.Linear)
            ;
        }
    }
}

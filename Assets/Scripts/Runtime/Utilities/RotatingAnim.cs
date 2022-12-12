using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace AllBets
{
    public class RotatingAnim : MonoBehaviour
    {
        Tween scaleTween;
        Material material;

        #region Settings
            public float rotationDuration = 20;
            public float scaleDuration = 1;
        #endregion

        private void OnDestroy() {
            scaleTween.Kill();
        }

        private void OnEnable() {
            if (scaleTween != null) scaleTween.Play();
        }

        private void OnDisable() {
            scaleTween.Pause();
        }

        private void Awake() 
        {
            DOTween.Init();
            material = GetComponent<SpriteRenderer>().material;
        }

        void Start()
        {
            material.SetInt("_RotationEnabled", 1);
            scaleTween = transform.DOScale(2.6f,scaleDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
            ;
        }
    }
}

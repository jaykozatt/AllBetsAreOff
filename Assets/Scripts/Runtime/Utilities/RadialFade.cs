using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace AllBets
{
    public class RadialFade : MonoBehaviour
    {
        [SerializeField] RectTransform radialGradient;
        [SerializeField] RectTransform leftCurtain;
        [SerializeField] RectTransform rightCurtain;
        [SerializeField] RectTransform topCurtain;
        [SerializeField] RectTransform bottomCurtain;
        
        Sequence sequence;

        public Action OnComplete;

        public bool isPlaying {
            get => sequence.IsPlaying();
        }

        private void Awake() 
        {
            DOTween.Init();
            sequence = DOTween.Sequence();
            sequence.SetAutoKill(false);
            sequence.SetEase(Ease.InOutQuad);

            Vector2 horizontalCurtains = leftCurtain.sizeDelta;
            Vector2 verticalCurtains = topCurtain.sizeDelta;

            horizontalCurtains.x = Screen.width/2;
            verticalCurtains.y = Screen.height/2;

            leftCurtain.sizeDelta = horizontalCurtains;
            rightCurtain.sizeDelta = horizontalCurtains;
            topCurtain.sizeDelta = verticalCurtains;
            bottomCurtain.sizeDelta = verticalCurtains;

            // Begin shrinking the gradient
            sequence.Append(
                radialGradient.DOScale(0, 1)
            );

            // As the gradient shrinks, close in the curtains
            sequence.Join(
                leftCurtain.DOPivotX(0, 1)
            );
            sequence.Join(
                rightCurtain.DOPivotX(1, 1)
            );
            sequence.Join(
                topCurtain.DOPivotY(1, 1)
            );
            sequence.Join(
                bottomCurtain.DOPivotY(0, 1)
            );

            sequence.OnComplete(()=> OnComplete?.Invoke());
        }

        public void CloseCurtain()
        {
            if (sequence.IsPlaying()) 
                sequence.Pause();
            sequence.PlayForward();
        }

        public void OpenCurtain()
        {
            if (sequence.IsPlaying()) 
                sequence.Pause();
            sequence.PlayBackwards();
        }
    }
}

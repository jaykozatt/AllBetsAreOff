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
        }

        private void Start() 
        {
            sequence = DOTween.Sequence();
            sequence.SetAutoKill(false);
            sequence.SetEase(Ease.InOutQuad);

            Vector2 horizontalCurtains = leftCurtain.sizeDelta;
            Vector2 verticalCurtains = topCurtain.sizeDelta;

            // Begin shrinking the gradient
            sequence.Append(
                radialGradient.DOScale(Vector2.zero, 1)
            );

            // As the gradient shrinks, close in the curtains
            sequence.Join(
                leftCurtain.DOAnchorMax(new Vector2(.5f, leftCurtain.anchorMax.y), 1)
            );
            sequence.Join(
                rightCurtain.DOAnchorMin(new Vector2(.5f, rightCurtain.anchorMin.y), 1)
            );
            sequence.Join(
                topCurtain.DOAnchorMin(new Vector2(topCurtain.anchorMin.x, .5f), 1)
            );
            sequence.Join(
                bottomCurtain.DOAnchorMax(new Vector2(bottomCurtain.anchorMax.x, .5f), 1)
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

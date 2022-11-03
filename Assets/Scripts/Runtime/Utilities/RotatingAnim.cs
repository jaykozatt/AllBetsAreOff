using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace AllBets
{
    public class RotatingAnim : MonoBehaviour
    {
        RectTransform rectTransform;
        Sequence sequence;

        #region Settings
            public float rotationDuration = 20;
            public float scaleDuration = 1;
        #endregion

        private void OnDestroy() {
            sequence.Kill();
        }

        private void OnEnable() {
            if (sequence != null) sequence.Play();
        }

        private void OnDisable() {
            sequence.Pause();
        }

        private void Awake() 
        {
            DOTween.Init();
            sequence = DOTween.Sequence();
            rectTransform = transform as RectTransform;    
        }

        // Start is called before the first frame update
        void Start()
        {
            sequence.Append(
                rectTransform.DOLocalRotate(Vector3.forward * 180, rotationDuration)
                    .SetEase(Ease.Linear)
                    .SetRelative()
            );
            sequence.Join(
                rectTransform.DOScale(1.2f,scaleDuration)
                    .SetLoops((int)(rotationDuration / scaleDuration),LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
            );
            sequence.Append(
                rectTransform.DOLocalRotate(Vector3.forward * 180, rotationDuration)
                    .SetEase(Ease.Linear)
                    .SetRelative()
            );
            sequence.Join(
                rectTransform.DOScale(1.2f,scaleDuration)
                    .SetLoops((int)(rotationDuration / scaleDuration),LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
            );

            sequence.SetLoops(-1);
        }
    }
}

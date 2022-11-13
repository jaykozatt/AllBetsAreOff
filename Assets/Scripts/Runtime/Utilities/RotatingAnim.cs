using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace AllBets
{
    public class RotatingAnim : MonoBehaviour
    {
        // RectTransform transform;
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
            // transform = transform as RectTransform;    
        }

        // Start is called before the first frame update
        void Start()
        {
            sequence.Append(
                transform.DOLocalRotate(Vector3.forward * 180, rotationDuration)
                    .SetEase(Ease.Linear)
                    .SetRelative()
            );
            sequence.Join(
                transform.DOScale(2.6f,scaleDuration)
                    .SetLoops((int)(rotationDuration / scaleDuration),LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
            );
            sequence.Append(
                transform.DOLocalRotate(Vector3.forward * 180, rotationDuration)
                    .SetEase(Ease.Linear)
                    .SetRelative()
            );
            sequence.Join(
                transform.DOScale(2.6f,scaleDuration)
                    .SetLoops((int)(rotationDuration / scaleDuration),LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
            );

            sequence.SetLoops(-1);
        }
    }
}

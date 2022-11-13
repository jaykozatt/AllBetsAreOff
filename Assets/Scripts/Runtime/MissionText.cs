using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

namespace AllBets
{
    public class MissionText : StaticInstance<MissionText>
    {
        public float fadeDuration = .5f;
        public int secondsUntilFade = 5;
        public SpriteRenderer shine;
        public CanvasGroup titleGroup;
        public CanvasGroup missionGroup;
        public TextMeshProUGUI pressAnyKeyText;

        Coroutine mainTitleRoutine;

        bool isFading = false;

        private void OnDestroy() 
        {
            if (mainTitleRoutine != null) StopCoroutine(mainTitleRoutine);
        }

        protected override void Awake() 
        {
            base.Awake();
            titleGroup = GetComponentInChildren<CanvasGroup>();
            missionGroup.gameObject.SetActive(true);
            missionGroup.alpha = 0;
            // pressAnyKeyText = GetComponentsInChildren<TextMeshProUGUI>()[1];
        }

        private void Start() 
        {
            mainTitleRoutine = StartCoroutine(MainTitle());
        }

        IEnumerator Flasher(TextMeshProUGUI text) 
        {
            WaitForSeconds wait1second = new WaitForSeconds(1);
            while (true) 
            {
                text.enabled = true;
                yield return wait1second;

                text.enabled = false;
                yield return wait1second;
            }
        }

        IEnumerator MainTitle()
        {
            Coroutine flasher = StartCoroutine(Flasher(pressAnyKeyText));
            Tween tween;

            while (!Input.anyKeyDown && Input.touchCount == 0)
            {
                yield return null;

            }

            StopCoroutine(flasher);
            // StartCoroutine(FadeOut(titleGroup));
            tween = titleGroup.DOFade(0,fadeDuration);
            tween = shine.DOFade(0,fadeDuration);
            while (tween.IsPlaying()) yield return null;

            // StartCoroutine(FadeIn(missionGroup));
            tween = missionGroup.DOFade(1,fadeDuration);
            while (tween.IsPlaying()) yield return null;
            yield return new WaitForSeconds(secondsUntilFade);

            // StartCoroutine(FadeOut(missionGroup));
            tween = missionGroup.DOFade(0,fadeDuration);
            while (tween.IsPlaying()) yield return null;

            GameManager.Instance.EnableGUI();
            GameManager.Instance.BeginGame();

        }

        // IEnumerator FadeIn(CanvasGroup group)
        // {
        //     isFading = true;
        //     group.gameObject.SetActive(true);
        //     group.alpha = 0;
        //     while (group.alpha < 1)
        //     {
        //         group.alpha = Mathf.Clamp01(group.alpha + fadingRate * Time.deltaTime);

        //         yield return null;
        //     }
        //     isFading = false;
        // }

        // IEnumerator FadeOut(CanvasGroup group)
        // {
        //     isFading = true;
        //     while (group.alpha > 0)
        //     {
        //         group.alpha = Mathf.Clamp01(group.alpha - fadingRate * Time.deltaTime);

        //         yield return null;
        //     }
        //     group.gameObject.SetActive(false);
        //     isFading = false;
        // }
    }
}

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
            tween = titleGroup.DOFade(0,fadeDuration);
            tween = shine.DOFade(0,fadeDuration);
            while (tween.IsPlaying()) yield return null;

            tween = missionGroup.DOFade(1,fadeDuration);
            while (tween.IsPlaying()) yield return null;
            yield return new WaitForSeconds(secondsUntilFade);

            tween = missionGroup.DOFade(0,fadeDuration);
            while (tween.IsPlaying()) yield return null;

            GameManager.Instance.EnableGUI();
            GameManager.Instance.BeginGame();

        }
    }
}

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
        }

        private void Start() 
        {
            mainTitleRoutine = StartCoroutine(MainTitle());
        }

        IEnumerator Blinker(TextMeshProUGUI text) 
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
            #if UNITY_ANDROID || UNITY_IOS 
                pressAnyKeyText.text = "Tap anywhere to begin...";
            #endif

            Coroutine blinker = StartCoroutine(Blinker(pressAnyKeyText));
            Tween tween;

            while (!Input.anyKeyDown && Input.touchCount == 0)
            {
                yield return null;

            }

            StopCoroutine(blinker);

            tween = missionGroup.DOFade(0,fadeDuration);
            while (tween.IsActive()) yield return null;

            GameManager.Instance.EnableGUI();
            GameManager.Instance.BeginGame();

        }
    }
}

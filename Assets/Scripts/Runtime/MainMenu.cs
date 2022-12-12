using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace AllBets
{
    public class MainMenu : StaticInstance<MainMenu>
    {
        public CanvasGroup menuGroup;
        public SpriteRenderer redChip;
        public SpriteRenderer dustVFX;

        Sequence sequence;
        Coroutine showHideRoutine;

        public bool IsTransitioning 
        {
            get => MenuCameraController.Instance.IsBlending || sequence.IsActive();
        }

        IEnumerator ShowRoutine()
        {
            while (MenuCameraController.Instance.IsBlending || SettingsMenu.Instance.IsTransitioning) 
                yield return null;

            sequence = DOTween.Sequence(gameObject);

            sequence.Append(
                menuGroup.DOFade(1,MenuCameraController.Instance.BlendingTime)
            );
            sequence.Join(
                redChip.DOFade(1,MenuCameraController.Instance.BlendingTime)
            );
            sequence.Join(
                dustVFX.DOFade(1,MenuCameraController.Instance.BlendingTime)
            );

            sequence.Play();
        }

        IEnumerator HideRoutine()
        {
            sequence = DOTween.Sequence(gameObject);

            sequence.Append(
                menuGroup.DOFade(0,1)
            );
            sequence.Join(
                redChip.DOFade(0,1)
            );
            sequence.Join(
                dustVFX.DOFade(0,1)
            );

            sequence.OnComplete(
                ()=>this.gameObject.SetActive(false)
            );

            sequence.Play();
            yield break;
        }

        protected override void Awake()
        {
            base.Awake();

            menuGroup.alpha = 1;
        }

        public void Show()
        {
            this.gameObject.SetActive(true);

            if (sequence.IsActive()) sequence.Kill();
            showHideRoutine = StartCoroutine(ShowRoutine());
        }

        public void Hide()
        {
            if (sequence.IsActive()) sequence.Kill();
            showHideRoutine = StartCoroutine(HideRoutine());
        }

        public void PlayClassicMode()
        {
            SceneLoader.Instance.TryLoadScene(Scene.ClassicMode);
        }

        public void GoToSettings()
        {
            Hide();
            SettingsMenu.Instance.Show();
        }
    }
}
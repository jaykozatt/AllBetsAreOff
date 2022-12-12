using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace AllBets
{
    public class SettingsMenu : StaticInstance<SettingsMenu>
    {
        public FMODUnity.StudioEventEmitter sfx;
        public RectTransform blackBackground;
        public RectTransform titleBar;
        public CanvasGroup titleGroup;
        public CanvasGroup settingsGroup;

        public bool IsTransitioning
        {
            get => MenuCameraController.Instance.IsBlending || transitionSequence.IsActive();
        }

        Sequence transitionSequence;
        Coroutine showHideRoutine;

        IEnumerator ShowRoutine()
        {
            // Wait until the camera gets in position
            while (MenuCameraController.Instance.IsBlending || MainMenu.Instance.IsTransitioning) yield return null;
            
            transitionSequence = DOTween.Sequence(gameObject);

            // Fade the black background in
            transitionSequence.Append(
                blackBackground.DOPivotY(.5f, 1)
            );

            // Fade the title bar in
            transitionSequence.Join(
                titleBar.DOPivotX(.5f, 1)
            );

            // Fade the rest in
            transitionSequence.Append(
                titleGroup.DOFade(1,1)
            );
            transitionSequence.Join(
                settingsGroup.DOFade(1,1)
            );

            transitionSequence.Play();
        }

        IEnumerator HideRoutine()
        {
            transitionSequence = DOTween.Sequence(gameObject);

            // Fade the rest out
            transitionSequence.Append(
                titleGroup.DOFade(0, .5f)
            );
            transitionSequence.Join(
                settingsGroup.DOFade(0, .5f)
            );

            // Fade the title bar out
            transitionSequence.Join(
                titleBar.DOPivotX(1.5f, 1)
            );

            // Fade the black background out
            transitionSequence.Join(
                blackBackground.DOPivotY(-.5f, 1)
            );

            transitionSequence.OnComplete(
                ()=>{
                    this.gameObject.SetActive(false);
                }
            );
            
            // Make sure to play the sequence
            transitionSequence.Play();

            yield break;
        }

        protected override void Awake()
        {
            base.Awake();
            titleGroup.alpha = 0;
            settingsGroup.alpha = 0;

            this.gameObject.SetActive(false);
        }

        public void OnBGMVolumeChanged(float value)
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("BGM Volume", value);
        }

        public void OnSFXVolumeChanged(float value)
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("SFX Volume", value);
        }

        public void Show()
        {
            this.gameObject.SetActive(true);
            MenuCameraController.Instance.frames[1].SetActive();

            if (transitionSequence.IsActive()) 
                transitionSequence.Kill();
            if (showHideRoutine != null) 
                StopCoroutine(showHideRoutine);
            
            showHideRoutine = StartCoroutine(ShowRoutine());
        }

        public void Hide()
        {
            MenuCameraController.Instance.frames[1].SetActive(false);

            if (transitionSequence.IsActive()) 
                transitionSequence.Kill();
            if (showHideRoutine != null) 
                StopCoroutine(showHideRoutine);
            
            showHideRoutine = StartCoroutine(HideRoutine());
        }

        public void GoToMainMenu() 
        {
            Hide();
            MainMenu.Instance.Show();            
        }
    }
}

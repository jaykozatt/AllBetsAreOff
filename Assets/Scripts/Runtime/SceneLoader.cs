using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Linq;

namespace AllBets
{
    public enum Scene {
        None, MainMenu, Game
    }

    public class SceneLoader : StaticInstance<SceneLoader>
    {

        public CanvasGroup loadingGraphics;
        public RadialFade transitionFade;

        Scene currentScene;
        Coroutine loadSceneRoutine;
        Tween fadeTween;

        public Action<float> OnLoadingProgressUpdate; 

        protected override void Awake() 
        {
            base.Awake();
            transitionFade.OnComplete += (()=>loadSceneRoutine= null);
        }

        private void Start() 
        {
            TryLoadScene(Scene.MainMenu);
        }

        public bool TryLoadScene(Scene scene)
        {
            if (loadSceneRoutine == null)
            {
                loadSceneRoutine = StartCoroutine(TransitionTo(scene));
                return true;
            } 

            return false;
        }

        private IEnumerator TransitionTo(Scene scene)
        {
            // First, fade to black
            transitionFade.CloseCurtain();

            // Wait while the fade effect is running
            while (transitionFade.isPlaying) yield return null;

            // Fade in the loading bar
            OnLoadingProgressUpdate?.Invoke(0);
            loadingGraphics.gameObject.SetActive(true);
            Tween tween = loadingGraphics.DOFade(1,.25f);
            while (tween.IsActive()) yield return null;

            // Unload the currently loaded scene if there is one
            AsyncOperation tracker;
            if (currentScene != Scene.None)
            {
                tracker = SceneManager.UnloadSceneAsync((int) currentScene);
                while (!tracker.isDone) yield return null; 
            }

            // Start loading the scene
            currentScene = scene;
            tracker = SceneManager.LoadSceneAsync((int) scene, LoadSceneMode.Additive);
            while (!tracker.isDone) 
            {
                // Post the current load progress to all listeners
                OnLoadingProgressUpdate?.Invoke(tracker.progress);
                yield return null;
            }

            // Send the completion message to all listeners
            OnLoadingProgressUpdate?.Invoke(tracker.progress);

            // Fade out the loading bar
            loadingGraphics.DOFade(0,.25f)
                .OnComplete(()=>loadingGraphics.gameObject.SetActive(false))
            ;

            // Lastly, reveal the scene
            transitionFade.OpenCurtain();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace AllBets
{
    [RequireComponent(typeof(Image))]
    public class LoadingBar : MonoBehaviour
    {
        Image bar;

        private void Start() 
        {
            bar = GetComponent<Image>();
            SceneLoader.Instance.OnLoadingProgressUpdate += UpdateProgressBar;
        }

        public void UpdateProgressBar(float value)
        {
            bar.fillAmount = value;
        }
    }
}

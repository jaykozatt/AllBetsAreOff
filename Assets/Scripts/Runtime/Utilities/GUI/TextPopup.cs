using UnityEngine;
using DG.Tweening;
using TMPro;

namespace AllBets
{
    public class TextPopup : MonoBehaviour 
    {
        RectTransform rectTransform;
        TextMeshPro textMesh;

        public string text {
            get=>textMesh.text; 
            set=>textMesh.text=value;
        }

        private void Awake() 
        {
            rectTransform = transform as RectTransform;
            textMesh = GetComponent<TextMeshPro>();
            DOTween.Init();
        }

        private void Start() 
        {
            rectTransform.DOAnchorPos3DY(1, .5f)
                .SetEase(Ease.OutExpo)
                .SetRelative(true)
                .SetRecyclable()
                .OnComplete(()=>{
                    Destroy(gameObject);
                })
            ;
        }
    }
}
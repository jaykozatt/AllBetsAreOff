using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace AllBets
{
    public class ComboDisplay : MonoBehaviour
    {
        [SerializeField] RectTransform card;
        TextMeshProUGUI textMesh;
        Sequence sequence;

        private void OnDestroy() 
        {
            sequence.Kill();
        }

        private void Awake() 
        {
            DOTween.Init();
            DOTween.defaultAutoPlay = AutoPlay.AutoPlayTweeners;

            textMesh = card.GetComponentInChildren<TextMeshProUGUI>();

            sequence = DOTween.Sequence();
            sequence.SetAutoKill(false);

            sequence.Append(
                card.DOPivotY(0, .5f).SetEase(Ease.OutQuad)
            );
            sequence.Append(
                card.DOPivotY(-.65f, GameManager.Instance.timeUntilComboReset).SetEase(Ease.InOutCubic)
            );
        }

        private void Start() 
        {
            GameManager.Instance.OnComboUpdate += UpdateCombo;
        }

        public void UpdateCombo(int currentMultiplier)
        {
            textMesh.text = $"{currentMultiplier}x";
            sequence.Restart();
        }
    }
}

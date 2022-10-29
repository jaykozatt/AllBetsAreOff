using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ComboDisplay : MonoBehaviour
{
    RectTransform[] cards;
    Sequence[] tweens;

    private void Awake() 
    {
        DOTween.Init();
        DOTween.defaultAutoPlay = AutoPlay.AutoPlayTweeners;
        
        cards = new RectTransform[transform.childCount];
        tweens = new Sequence[transform.childCount];

        int index = 0;
        foreach(RectTransform child in transform)
        {
            cards[index] = child;
            tweens[index] = DOTween.Sequence();
            tweens[index].SetAutoKill(false);

            tweens[index].Append(
                child.DOPivotY(0, .5f).SetEase(Ease.OutQuad)
            );
            tweens[index].Append(
                child.DOPivotY(-.65f, GameManager.Instance.timeUntilComboReset).SetEase(Ease.InOutCubic)
            );

            index++;
        }
    }

    private void Start() 
    {
        GameManager.Instance.OnComboUpdate += UpdateCombo;
    }

    public void UpdateCombo(int currentMultiplier)
    {
        for (int i=0; i<=currentMultiplier-2; i++)
        {
            tweens[i].Restart();
        }
    }
}

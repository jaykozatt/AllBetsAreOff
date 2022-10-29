using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TouchUI : StaticInstance<ControlsUI>
{
    public Image shiftKey;
    public Image spaceKey;
    public Color normalColor;
    public Color highlightColor;

    DiceController dice;
    Coroutine flashRoutine;

    private void OnDestroy() {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
    }

    // Start is called before the first frame update
    void Start()
    {
        dice = DiceController.Instance;
        // flashRoutine = StartCoroutine(Flash());
    }

    // Update is called once per frame
    void Update()
    {
        if (dice.IsDeployed) 
        {
            spaceKey.color = normalColor;
        }
        else
        {
            spaceKey.color = highlightColor;
        } 

        if (dice.IsEntangled) 
        {
            shiftKey.color = highlightColor;
        }
        else
        {
            shiftKey.color = normalColor;
        } 
    }

    IEnumerator Flash() 
    {
        while (true)
        {
            shiftKey.color = shiftKey.color == normalColor ? highlightColor : normalColor;
            yield return new WaitForSeconds(.5f);
        }
    }
}

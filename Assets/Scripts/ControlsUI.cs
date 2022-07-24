using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlsUI : StaticInstance<ControlsUI>
{
    public TextMeshProUGUI wasd;
    public TextMeshProUGUI shift;
    public Image shiftKey;
    public TextMeshProUGUI space;
    public Image spaceKey;

    Color normalColor = new Color(248,244,235);
    Color highlight = Color.yellow;

    DiceController dice;

    // Start is called before the first frame update
    void Start()
    {
        dice = DiceController.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (dice.IsDeployed) 
        {
            space.text = "Lengthen";
            spaceKey.color = normalColor;
        }
        else
        {
            space.text = "Deploy";
            spaceKey.color = highlight;
        } 

        if (dice.IsTangled) 
        {
            shift.text = "Tackle";
            shiftKey.color = highlight;
        }
        else
        {
            shift.text = "Shorten";
            shiftKey.color = normalColor;
        } 
    }
}

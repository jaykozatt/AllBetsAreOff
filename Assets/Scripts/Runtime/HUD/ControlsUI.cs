using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AllBets
{
    public class ControlsUI : StaticInstance<ControlsUI>
    {
        #region Settings
        [Header("Settings")]
            public Color normalColor;
            public Color highlight; // #EAB543
        #endregion

        #region References
        [Header("References")]
            public TextMeshProUGUI wasd;
            public Image shiftKey;
            public TextMeshProUGUI shift;
            public Image spaceKey;
            public TextMeshProUGUI space;
            private DiceController dice;
        #endregion

        #region Monobehaviour Functions
            void Start()
            {
                dice = DiceController.Instance;
            }

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

                if (dice.IsEntangled) 
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
        #endregion
    }
}

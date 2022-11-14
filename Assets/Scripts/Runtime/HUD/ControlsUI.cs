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
            private DiceController die;
            private Wire wire;
        #endregion

        #region Monobehaviour Functions
            void Start()
            {
                die = DiceController.Instance;
                wire = Wire.Instance;
            }

            void Update()
            {
                if (die.IsDeployed) 
                {
                    space.text = "Lengthen";
                    spaceKey.color = normalColor;
                }
                else
                {
                    space.text = "Deploy";
                    spaceKey.color = highlight;
                } 

                if (wire.IsEntangled) 
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

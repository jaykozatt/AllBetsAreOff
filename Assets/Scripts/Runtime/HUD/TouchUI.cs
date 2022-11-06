using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AllBets
{
    public class TouchUI : StaticInstance<ControlsUI>
    {
        #region Settings
        [Header("Settings")]
            public Color normalColor;
            public Color highlightColor;
        #endregion

        #region References
        [Header("References")]
            public Image shiftKey;
            public Image spaceKey;
            private DiceController die;
        #endregion

        #region Coroutine References
            Coroutine flashRoutine;
        #endregion

        #region Coroutine Definitions
            IEnumerator Flash() 
            {
                while (true)
                {
                    shiftKey.color = shiftKey.color == normalColor ? highlightColor : normalColor;
                    yield return new WaitForSeconds(.5f);
                }
            }
        #endregion

        #region Monobehaviour Functions
            private void OnDestroy() {
                if (flashRoutine != null) StopCoroutine(flashRoutine);
            }

            void Start()
            {
                die = DiceController.Instance;
                // flashRoutine = StartCoroutine(Flash());
            }

            void Update()
            {
                // If the die is not deployed, highlight the deploy button
                if (!die.IsDeployed) spaceKey.color = highlightColor;
                else spaceKey.color = normalColor;

                // If the die is entangled, highlight the tackle button
                if (die.IsEntangled) shiftKey.color = highlightColor;
                else shiftKey.color = normalColor;
            }
        #endregion


    }
}

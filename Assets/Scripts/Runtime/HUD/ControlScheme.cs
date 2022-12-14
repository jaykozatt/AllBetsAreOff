using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllBets
{
    public class ControlScheme : MonoBehaviour
    {
        public GameObject TouchScheme;
        public GameObject KeyboardScheme;

        void Start()
        {
            #if UNITY_ANDROID || UNITY_IOS 
                TouchScheme.SetActive(true);
                KeyboardScheme.SetActive(false);
            #endif

            #if UNITY_STANDALONE || UNITY_WEBGL
                TouchScheme.SetActive(false);
                KeyboardScheme.SetActive(true);
            #endif
        }

    }
}

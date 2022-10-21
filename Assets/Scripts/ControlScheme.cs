using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlScheme : MonoBehaviour
{
    public GameObject TouchScheme;
    public GameObject KeyboardScheme;

    // Start is called before the first frame update
    void Start()
    {
        #if UNITY_ANDROID || UNITY_IOS 
            TouchScheme.SetActive(true);
            KeyboardScheme.SetActive(false);
        #endif

        #if UNITY_STANDALONE
            TouchScheme.SetActive(false);
            KeyboardScheme.SetActive(true);
        #endif
    }

}

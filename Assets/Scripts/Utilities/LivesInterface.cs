using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivesInterface : StaticInstance<LivesInterface>
{
    public GameObject life1;
    public GameObject life2;
    public GameObject life3;

    public GameObject hurtScreen;

    public void UpdateDisplay(int lives)
    {
        switch (PlayerController.Instance.lives)
        {
            case 3:
                life1.SetActive(true);
                life2.SetActive(true);
                life3.SetActive(true);
                break;
            case 2:
                life1.SetActive(false);
                life2.SetActive(true);
                life3.SetActive(true);
                break;
            case 1:
                life1.SetActive(false);
                life2.SetActive(false);
                life3.SetActive(true);
                break;
            default:
                life1.SetActive(false);
                life2.SetActive(false);
                life3.SetActive(false);
                break;
        }
    }

    public void HurtFlash()
    {
        StartCoroutine(FlashScreenRoutine());
    }

    IEnumerator FlashScreenRoutine()
    {
        hurtScreen.SetActive(true);
        yield return null;
        yield return null;
        hurtScreen.SetActive(false);
        
    }
}

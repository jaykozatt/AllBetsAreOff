using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionText : StaticInstance<MissionText>
{
    public int fadingRate = 10;
    public int secondsUntilFade = 5;
    public TextMeshProUGUI missionText;
    public Image fmod;

    Coroutine faderRoutine;

    private void OnDestroy() {
        if (faderRoutine != null) StopCoroutine(faderRoutine);
    }

    protected override void Awake() {
        base.Awake();
        missionText = GetComponentInChildren<TextMeshProUGUI>();
        fmod = GetComponentInChildren<Image>();
    }

    private void Start() {
        faderRoutine = StartCoroutine(Fader());
    }

    IEnumerator Fader()
    {
        yield return new WaitForSeconds(secondsUntilFade);

        Color color;
        while (missionText.alpha > 0)
        {
            missionText.alpha = Mathf.Max(0,missionText.alpha - fadingRate * Time.deltaTime);
            
            color = fmod.color;
            color.a = Mathf.Max(0, color.a - fadingRate * Time.deltaTime);
            fmod.color = color;

            yield return null;
        }

        gameObject.SetActive(false);
        GameManager.Instance.gameState = GameState.Started;
    }
}

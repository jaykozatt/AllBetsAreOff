using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MissionText : StaticInstance<MissionText>
{
    public int fadingRate = 10;
    public int secondsUntilFade = 5;
    public TextMeshProUGUI missionText;

    Coroutine faderRoutine;

    private void OnDestroy() {
        if (faderRoutine != null) StopCoroutine(faderRoutine);
    }

    protected override void Awake() {
        base.Awake();
        missionText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start() {
        faderRoutine = StartCoroutine(Fader());
    }

    IEnumerator Fader()
    {
        yield return new WaitForSeconds(secondsUntilFade);

        while (missionText.alpha > 0)
        {
            missionText.alpha = Mathf.Max(0,missionText.alpha - fadingRate * Time.deltaTime);
            yield return null;
        }

        gameObject.SetActive(false);
        GameManager.Instance.gameState = GameState.Started;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionText : StaticInstance<MissionText>
{
    public int fadingRate = 10;
    public int secondsUntilFade = 5;
    public CanvasGroup titleGroup;
    public CanvasGroup missionGroup;
    public TextMeshProUGUI pressAnyKeyText;
    public Image fmod;

    Coroutine mainTitleRoutine;

    bool isFading = false;

    private void OnDestroy() {
        if (mainTitleRoutine != null) StopCoroutine(mainTitleRoutine);
    }

    protected override void Awake() {
        base.Awake();
        titleGroup = GetComponentInChildren<CanvasGroup>();
        // pressAnyKeyText = GetComponentsInChildren<TextMeshProUGUI>()[1];
        fmod = GetComponentInChildren<Image>();
    }

    private void Start() {
        mainTitleRoutine = StartCoroutine(MainTitle());
    }

    IEnumerator Flasher(TextMeshProUGUI text) 
    {
        WaitForSeconds wait1second = new WaitForSeconds(1);
        while (true) 
        {
            text.enabled = true;
            yield return wait1second;

            text.enabled = false;
            yield return wait1second;
        }
    }

    IEnumerator MainTitle()
    {
        Coroutine flasher = StartCoroutine(Flasher(pressAnyKeyText));

        while (!Input.anyKeyDown)
        {
            yield return null;

        }

        StopCoroutine(flasher);
        StartCoroutine(FadeOut(titleGroup));
        while (isFading) yield return null;

        GameManager.Instance.BeginGame();
        StartCoroutine(FadeIn(missionGroup));
        while (isFading) yield return null;
        yield return new WaitForSeconds(2);

        StartCoroutine(FadeOut(missionGroup));
        while (isFading) yield return null;

        GameManager.Instance.EnableGUI();

    }

    IEnumerator FadeIn(CanvasGroup group)
    {
        isFading = true;
        group.gameObject.SetActive(true);
        group.alpha = 0;
        while (group.alpha < 1)
        {
            group.alpha = Mathf.Clamp01(group.alpha + fadingRate * Time.deltaTime);

            yield return null;
        }
        isFading = false;
    }

    IEnumerator FadeOut(CanvasGroup group)
    {
        isFading = true;
        while (group.alpha > 0)
        {
            group.alpha = Mathf.Clamp01(group.alpha - fadingRate * Time.deltaTime);

            yield return null;
        }
        group.gameObject.SetActive(false);
        isFading = false;
    }
}

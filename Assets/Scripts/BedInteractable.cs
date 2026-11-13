using System.Collections;
using TMPro;
using UnityEngine;

public class BedInteractable : Interactable
{
    [Header("Sleep UI")]
    public CanvasGroup sleepFade;
    public TMP_Text sleepDayText;

    [Header("Timing")]
    public float fadeDuration = 1.5f;
    public float blackScreenDuration = 2f;

    [Header("Player Controls")]
    [Tooltip("Assign movement and camera-look scripts only.")]
    public Behaviour[] playerControls;

    private bool sleeping;

    private void Awake()
    {
        if (sleepFade != null)
        {
            sleepFade.alpha = 0f;
            sleepFade.interactable = false;
            sleepFade.blocksRaycasts = false;
        }

        if (sleepDayText != null)
            sleepDayText.text = "";
    }

    public override void Interact()
    {
        if (sleeping)
            return;

        QuestManager manager = QuestManager.Instance;

        if (manager == null)
        {
            UnityEngine.Debug.LogError("QuestManager not found!");
            return;
        }

        if (!manager.CanSleep)
        {
            manager.TrySleep();
            return;
        }

        if (sleepFade == null || sleepDayText == null)
        {
            UnityEngine.Debug.LogError(
                "Assign Sleep Fade and Sleep Day Text on the bed."
            );
            return;
        }

        StartCoroutine(SleepRoutine(manager));
    }

    private IEnumerator SleepRoutine(QuestManager manager)
    {
        sleeping = true;

        bool[] previousStates = new bool[playerControls.Length];

        for (int i = 0; i < playerControls.Length; i++)
        {
            Behaviour control = playerControls[i];

            if (control == null || control == this)
                continue;

            previousStates[i] = control.enabled;
            control.enabled = false;
        }

        sleepFade.blocksRaycasts = true;
        sleepDayText.text = "";

        // Fade to black before showing the new day.
        yield return FadeTo(1f);

        // Start the next day while the screen is black.
        manager.TrySleep();

        sleepDayText.text = $"DAY {manager.CurrentDay}";

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, blackScreenDuration)
        );

        // Fade the black screen and day text away together.
        yield return FadeTo(0f);

        sleepDayText.text = "";
        sleepFade.blocksRaycasts = false;

        for (int i = 0; i < playerControls.Length; i++)
        {
            Behaviour control = playerControls[i];

            if (control != null && control != this)
                control.enabled = previousStates[i];
        }

        sleeping = false;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = sleepFade.alpha;
        float duration = Mathf.Max(0f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            sleepFade.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                elapsed / duration
            );

            yield return null;
        }

        sleepFade.alpha = targetAlpha;
    }
}
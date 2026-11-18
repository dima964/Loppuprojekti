using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RadioTuningMinigame : MonoBehaviour
{
    [Header("Tuning UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Slider frequencySlider;
    [SerializeField] private TMP_Text frequencyText;
    [SerializeField] private TMP_Text statusText;

    [Header("Radio Audio")]
    [SerializeField] private AudioSource staticSource;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip warningClip;

    [Header("Player Controls")]
    [SerializeField] private Behaviour playerController;

    [Header("Tuning Settings")]
    [SerializeField] private float frequencyStep = 0.1f;
    [SerializeField] private float repeatInterval = 0.1f;

    private const int RadioQuestIndex = 2;
    private const float TargetFrequency = 98.7f;
    private const float FrequencyTolerance = 0.05f;
    private const float HoldDuration = 2f;
    private const string Warning = "Don't hurt it. It remembers.";

    private bool isOpen;
    private bool isPlayingWarning;
    private bool playerWasEnabled;
    private bool cursorWasVisible;

    private CursorLockMode previousCursorLock;
    private Coroutine warningRoutine;

    private float heldTime;
    private float nextTuneTime;
    private int previousDirection;

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;
        heldTime = 0f;
        nextTuneTime = 0f;
        previousDirection = 0;

        previousCursorLock = Cursor.lockState;
        cursorWasVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
        {
            playerWasEnabled = playerController.enabled;
            playerController.enabled = false;
        }

        frequencySlider.minValue = 88f;
        frequencySlider.maxValue = 108f;
        frequencySlider.wholeNumbers = false;
        frequencySlider.SetValueWithoutNotify(92f);
        frequencySlider.interactable = false;

        panel.SetActive(true);

        if (staticSource != null)
        {
            staticSource.loop = true;
            staticSource.Play();
        }

        UpdateTuning();
    }

    private void Update()
    {
        if (!isOpen || isPlayingWarning)
            return;

        HandleTuningInput();
        UpdateTuning();
    }

    private void HandleTuningInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        int direction = 0;

        if (keyboard.aKey.isPressed)
            direction--;

        if (keyboard.dKey.isPressed)
            direction++;

        if (direction == 0)
        {
            previousDirection = 0;
            return;
        }

        bool directionChanged = direction != previousDirection;

        if (directionChanged || Time.unscaledTime >= nextTuneTime)
        {
            float frequency =
                frequencySlider.value + direction * frequencyStep;

            // Round to tenths to keep the displayed steps consistent.
            frequencySlider.value = Mathf.Round(frequency * 10f) / 10f;

            nextTuneTime =
                Time.unscaledTime + Mathf.Max(0.01f, repeatInterval);
        }

        previousDirection = direction;
    }

    private void UpdateTuning()
    {
        float frequency = frequencySlider.value;
        float distance = Mathf.Abs(frequency - TargetFrequency);

        frequencyText.text = $"{frequency:F1} MHz";

        if (staticSource != null)
        {
            staticSource.volume = Mathf.InverseLerp(
                FrequencyTolerance,
                3f,
                distance
            );
        }

        if (distance > FrequencyTolerance)
        {
            heldTime = 0f;
            statusText.text = "Hold A / D to tune until the static clears.";
            return;
        }

        heldTime += Time.unscaledDeltaTime;

        statusText.text =
            $"Signal found. Hold ({heldTime:F1}/{HoldDuration:F1}s)";

        if (heldTime >= HoldDuration)
        {
            isPlayingWarning = true;
            warningRoutine = StartCoroutine(PlayWarning());
        }
    }

    private IEnumerator PlayWarning()
    {
        if (staticSource != null)
            staticSource.Stop();

        statusText.text = Warning;

        if (voiceSource != null && warningClip != null)
        {
            voiceSource.clip = warningClip;
            voiceSource.loop = false;
            voiceSource.Play();

            while (voiceSource.isPlaying)
                yield return null;
        }
        else
        {
            yield return new WaitForSecondsRealtime(3f);
        }

        warningRoutine = null;
        Close();

        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest(RadioQuestIndex);
    }

    public void Close()
    {
        if (!isOpen)
            return;

        if (warningRoutine != null)
        {
            StopCoroutine(warningRoutine);
            warningRoutine = null;
        }

        if (staticSource != null)
            staticSource.Stop();

        if (voiceSource != null)
            voiceSource.Stop();

        panel.SetActive(false);

        Cursor.lockState = previousCursorLock;
        Cursor.visible = cursorWasVisible;

        if (playerController != null)
            playerController.enabled = playerWasEnabled;

        isOpen = false;
        isPlayingWarning = false;
    }
}
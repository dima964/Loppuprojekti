using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorMinigameInteractable : Interactable
{
    [Header("UI")]
    public GameObject minigamePanel;
    public TMP_Text instructionsText;

    [Tooltip("Order: Red, Blue, Yellow, Green")]
    public Button[] wireButtons;

    [Header("Player")]
    [Tooltip("Drag the player's movement and camera-look components here.")]
    public Behaviour[] playerControls;

    [Header("Difficulty")]
    public int sequenceLength = 6;
    public float previewSeconds = 4f;

    private readonly string[] colourNames =
        { "RED", "BLUE", "YELLOW", "GREEN" };

    private int[] sequence;
    private int currentStep;
    private bool isOpen;
    private bool acceptingInput;
    private bool[] previousControlStates;

    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    private void Start()
    {
        minigamePanel.SetActive(false);

        for (int i = 0; i < wireButtons.Length; i++)
        {
            int colourIndex = i;
            wireButtons[i].onClick.AddListener(
                () => SelectWire(colourIndex));
        }
    }

    public override void Interact()
    {
        if (isOpen || QuestManager.Instance == null)
            return;

        if (!QuestManager.Instance.GeneratorQuestStarted)
        {
            UnityEngine.Debug.Log("Listen to the radio first.");
            return;
        }

        if (QuestManager.Instance.GeneratorQuestCompleted)
            return;

        isOpen = true;

        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        previousControlStates = new bool[playerControls.Length];

        for (int i = 0; i < playerControls.Length; i++)
        {
            if (playerControls[i] == null)
                continue;

            previousControlStates[i] = playerControls[i].enabled;
            playerControls[i].enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        minigamePanel.SetActive(true);
        StartCoroutine(ShowSequence(false));
    }

    private IEnumerator ShowSequence(bool retry)
    {
        SetButtonsEnabled(false);

        if (retry)
        {
            instructionsText.text =
                "Wrong wire!\nResetting the circuit...";

            yield return new WaitForSecondsRealtime(1.2f);
        }

        sequence = new int[Mathf.Max(1, sequenceLength)];
        currentStep = 0;

        string preview = "MEMORISE THE SEQUENCE\n\n";

        for (int i = 0; i < sequence.Length; i++)
        {
            sequence[i] = UnityEngine.Random.Range(0, colourNames.Length);
            preview += colourNames[sequence[i]];

            if (i < sequence.Length - 1)
                preview += " → ";
        }

        instructionsText.text = preview;

        yield return new WaitForSecondsRealtime(previewSeconds);

        instructionsText.text =
            $"Repeat the sequence\n\n0 / {sequence.Length}";

        SetButtonsEnabled(true);
    }

    private void SelectWire(int colourIndex)
    {
        if (!isOpen || !acceptingInput)
            return;

        if (colourIndex != sequence[currentStep])
        {
            StartCoroutine(ShowSequence(true));
            return;
        }

        currentStep++;

        instructionsText.text =
            $"Repeat the sequence\n\n{currentStep} / {sequence.Length}";

        if (currentStep == sequence.Length)
        {
            SetButtonsEnabled(false);
            QuestManager.Instance.CompleteGeneratorQuest();
            StartCoroutine(FinishMinigame());
        }
    }

    private IEnumerator FinishMinigame()
    {
        instructionsText.text = "POWER RESTORED";

        yield return new WaitForSecondsRealtime(1.5f);

        CloseMinigame();
    }

    // Can also be connected to a Close button.
    public void CloseMinigame()
    {
        if (!isOpen)
            return;

        StopAllCoroutines();
        SetButtonsEnabled(false);
        minigamePanel.SetActive(false);

        for (int i = 0; i < playerControls.Length; i++)
        {
            if (playerControls[i] != null)
                playerControls[i].enabled = previousControlStates[i];
        }

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
        isOpen = false;
    }

    private void SetButtonsEnabled(bool enabled)
    {
        acceptingInput = enabled;

        foreach (Button button in wireButtons)
        {
            button.interactable = enabled;
        }
    }

    private void OnDisable()
    {
        if (isOpen)
            CloseMinigame();
    }
}
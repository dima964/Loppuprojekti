using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private enum QuestState
    {
        WaitingForRadio,
        Active,
        ReadyToSleep,
        Finished
    }

    [Header("Quest UI")]
    public GameObject questPopup;
    public TMP_Text questPopupText;
    public GameObject activeQuest;
    public TMP_Text activeQuestText;

    [Header("Optional Separate Day Text")]
    public TMP_Text dayText;

    [Header("Daily Objectives - Exactly Five")]
    public string[] questObjectives = new string[5]
    {
        "Fix the generator",
        "Find and collect the keys",
        "Repair the radio signal",
        "Investigate the suspicious transmission",
        "Complete the final assignment"
    };

    [Header("Ending")]
    public UnityEvent onAllQuestsCompleted;

    private const int TotalDays = 5;

    private int currentDay = 1;
    private QuestState state = QuestState.WaitingForRadio;

    public int CurrentDay => currentDay;

    public bool CanSleep => state == QuestState.ReadyToSleep;

    public bool AllQuestsFinished => state == QuestState.Finished;

    // Compatibility with the existing generator minigame.
    public bool GeneratorQuestStarted =>
        currentDay > 1 ||
        (currentDay == 1 && state != QuestState.WaitingForRadio);

    public bool GeneratorQuestCompleted =>
        currentDay > 1 ||
        (currentDay == 1 &&
         (state == QuestState.ReadyToSleep ||
          state == QuestState.Finished));

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        questPopup.SetActive(false);
        activeQuest.SetActive(true);

        UpdateUI();
    }

    // Compatibility with older radio scripts.
    public void StartGeneratorQuest()
    {
        ReceiveQuestFromRadio();
    }

    public void ReceiveQuestFromRadio()
    {
        switch (state)
        {
            case QuestState.WaitingForRadio:
                state = QuestState.Active;
                UpdateUI();

                ShowPopup(
                    $"RADIO - DAY {currentDay}/{TotalDays}\n\n" +
                    GetCurrentObjective()
                );
                break;

            case QuestState.Active:
                ShowPopup(
                    "CURRENT ASSIGNMENT\n\n" +
                    GetCurrentObjective()
                );
                break;

            case QuestState.ReadyToSleep:
                ShowPopup(
                    "Your assignment is complete.\n\n" +
                    "Return to bed and sleep."
                );
                break;

            case QuestState.Finished:
                ShowPopup("All five assignments are complete.");
                break;
        }
    }

    public bool IsQuestActive(int questIndex)
    {
        return state == QuestState.Active &&
               questIndex == currentDay - 1;
    }

    // Called by the existing generator minigame.
    public void CompleteGeneratorQuest()
    {
        CompleteQuest(0);
    }

    // Indices: Day 1 = 0, Day 2 = 1, and so on.
    public void CompleteQuest(int questIndex)
    {
        if (!IsQuestActive(questIndex))
            return;

        if (currentDay == TotalDays)
        {
            state = QuestState.Finished;
            UpdateUI();

            ShowPopup(
                "FINAL ASSIGNMENT COMPLETE\n\n" +
                "You survived all five days."
            );

            onAllQuestsCompleted?.Invoke();
            return;
        }

        state = QuestState.ReadyToSleep;
        UpdateUI();

        ShowPopup(
            $"DAY {currentDay} - ASSIGNMENT COMPLETE\n\n" +
            "Return to bed and sleep."
        );
    }

    // Called by the bed.
    public void TrySleep()
    {
        if (state == QuestState.Finished)
        {
            ShowPopup("All five assignments are complete.");
            return;
        }

        if (state == QuestState.WaitingForRadio)
        {
            ShowPopup(
                "You cannot sleep yet.\n\n" +
                "Use the radio to receive today's assignment."
            );
            return;
        }

        if (state == QuestState.Active)
        {
            ShowPopup(
                "You cannot sleep yet.\n\n" +
                "Finish today's assignment first."
            );
            return;
        }

        currentDay++;
        state = QuestState.WaitingForRadio;

        UpdateUI();

        ShowPopup(
            $"DAY {currentDay}/{TotalDays}\n\n" +
            "You wake up.\nUse the radio for today's assignment."
        );
    }

    private void UpdateUI()
    {
        string dayHeading = $"DAY {currentDay}/{TotalDays}";

        if (dayText != null)
            dayText.text = dayHeading;

        string objective;

        switch (state)
        {
            case QuestState.WaitingForRadio:
                objective = "Use the radio to receive your assignment.";
                break;

            case QuestState.Active:
                objective = GetCurrentObjective();
                break;

            case QuestState.ReadyToSleep:
                objective = "Assignment complete.\nReturn to bed and sleep.";
                break;

            default:
                objective = "All five assignments completed.";
                break;
        }

        // Show the day here if no separate day label is assigned.
        activeQuestText.text = dayText == null
            ? dayHeading + "\n\n" + objective
            : objective;
    }

    private string GetCurrentObjective()
    {
        int index = currentDay - 1;

        if (questObjectives != null &&
            index < questObjectives.Length &&
            !string.IsNullOrWhiteSpace(questObjectives[index]))
        {
            return questObjectives[index];
        }

        return "Objective to be added.";
    }

    private void ShowPopup(string message)
    {
        CancelInvoke(nameof(HidePopup));

        questPopupText.text = message;
        questPopup.SetActive(true);

        Invoke(nameof(HidePopup), 4f);
    }

    private void HidePopup()
    {
        questPopup.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
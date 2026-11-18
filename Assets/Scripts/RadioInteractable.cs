using System.Diagnostics;
using UnityEngine;

public class RadioInteractable : Interactable
{
    [SerializeField] private RadioTuningMinigame tuningMinigame;

    public override void Interact()
    {
        QuestManager quests = QuestManager.Instance;

        if (quests == null)
        {
            UnityEngine.Debug.LogError("QuestManager not found!");
            return;
        }

        if (quests.IsQuestActive(2) && tuningMinigame != null)
        {
            tuningMinigame.Open();
            return;
        }

        quests.ReceiveQuestFromRadio();
    }
}
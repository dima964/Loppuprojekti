using System.Diagnostics;
using UnityEngine;

public class KeyInteractable : Interactable
{
    private bool collected = false;

    public override void Interact()
    {
        if (collected)
            return;

        if (QuestManager.Instance == null)
        {
            UnityEngine.Debug.LogError("QuestManager not found!");
            return;
        }

        // Quest index 1 = Day 2.
        // Only allow collection after receiving the radio assignment.
        if (!QuestManager.Instance.IsQuestActive(1))
        {
            UnityEngine.Debug.Log("You do not need these keys yet.");
            return;
        }

        collected = true;

        QuestManager.Instance.CompleteQuest(1);

        gameObject.SetActive(false);
    }
}
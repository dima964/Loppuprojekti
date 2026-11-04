
using System.Diagnostics;
using UnityEngine;

public class RadioInteractable : Interactable
{
    private bool radioUsed = false;

    public override void Interact()
    {
        if (radioUsed)
            return;

        radioUsed = true;

        UnityEngine.Debug.Log("Radio activated!");

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StartGeneratorQuest();
        }
        else
        {
            UnityEngine.Debug.LogError("QuestManager not found!");
        }
    }
}

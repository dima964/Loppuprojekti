using UnityEngine;

public class RadioInteractable : Interactable
{
    public override void Interact()
    {
        if (QuestManager.Instance == null)
        {
            UnityEngine.Debug.LogError("QuestManager not found!");
            return;
        }

        QuestManager.Instance.ReceiveQuestFromRadio();
    }
}
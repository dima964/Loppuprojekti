
using UnityEngine;

public class RadioInteractable : Interactable
{
    public override void Interact()
    {
        base.Interact();

        Debug.Log("Radio activated!");
        Debug.Log("Fred receives his work instructions.");
    }
}

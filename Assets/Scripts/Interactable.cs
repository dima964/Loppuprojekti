
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    public UnityEvent onInteract = new UnityEvent();

    public void Interact()
    {
        Debug.Log("Interacted with: " + gameObject.name);

        // Trigger actions assigned in the Inspector
        onInteract.Invoke();
    }
}

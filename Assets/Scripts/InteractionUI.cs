
using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public TMP_Text interactionPrompt;

    [Header("Settings")]
    public float interactionDistance = 3f;

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsPlaying == false)
        {
            HidePrompt();
            return;
        }

        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        if (playerCamera == null || interactionPrompt == null)
            return;

        Ray ray = new Ray(
            playerCamera.position,
            playerCamera.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            Interactable interactable =
                hit.collider.GetComponentInParent<Interactable>();

            if (interactable != null)
            {
                RadioInteractable radio =
                    interactable as RadioInteractable;

                if (radio != null)
                {
                    ShowPrompt("Press E to use radio");
                }
                else
                {
                    ShowPrompt("Press E to interact");
                }

                return;
            }
        }

        HidePrompt();
    }

    void ShowPrompt(string message)
    {
        interactionPrompt.gameObject.SetActive(true);
        interactionPrompt.text = message;
    }

    void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }
}

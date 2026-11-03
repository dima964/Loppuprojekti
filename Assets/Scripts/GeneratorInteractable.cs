
using UnityEngine;

public class GeneratorInteractable : Interactable
{
    private bool generatorOn = false;

    public override void Interact()
    {
        base.Interact();

        if (generatorOn == false)
        {
            generatorOn = true;

            Debug.Log("Generator turned ON!");
        }
        else
        {
            generatorOn = false;

            Debug.Log("Generator turned OFF!");
        }
    }
}

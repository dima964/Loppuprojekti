
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest UI")]
    public GameObject questPopup;
    public TMP_Text questPopupText;
    public GameObject activeQuest;
    public TMP_Text activeQuestText;

    private bool questStarted;
    private bool questCompleted;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartGeneratorQuest()
    {
        if (questStarted)
            return;

        questStarted = true;

        questPopupText.text =
            "NEW OBJECTIVE\n\n" +
            "Restore the power\n\n" +
            "Find the generator and get it running again.";

        activeQuestText.text =
            "OBJECTIVE\n\nFix the generator";

        questPopup.SetActive(true);
        activeQuest.SetActive(true);

        Invoke(nameof(HidePopup), 4f);

        UnityEngine.Debug.Log("Generator quest started!");
    }

    public void CompleteGeneratorQuest()
    {
        if (!questStarted || questCompleted)
            return;

        questCompleted = true;

        activeQuestText.text =
            "OBJECTIVE COMPLETED\n\nGenerator restored";

        UnityEngine.Debug.Log("Generator quest completed!");
    }

    void HidePopup()
    {
        questPopup.SetActive(false);
    }
}

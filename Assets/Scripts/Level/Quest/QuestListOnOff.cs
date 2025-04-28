using UnityEngine;

public class QuestListOnOff : MonoBehaviour
{
    [SerializeField] private GameObject questBar;
    private bool _isOpen = true;

    private void OnEnable()
    {
        QuestUIController.OnQuestLoaded += QuestLoaded;
    }
    private void OnDisable()
    {
        QuestUIController.OnQuestLoaded -= QuestLoaded;
    }

    public void OnButtonClick()
    {
        _isOpen = !_isOpen;
        questBar.SetActive(_isOpen);
    }

    private void QuestLoaded()
    {
        questBar.SetActive(false);
        questBar.SetActive(true);
    }
}

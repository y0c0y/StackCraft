using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionUI: MonoBehaviour
{
    public enum DescriptionType
    {
        None,
        Confirm,
        YesOrNo
    }
    
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    public void SetDescription(DescriptionType descriptionType, string message, Action yesCallback = null, Action noCallback = null)
    {
        SetText(message);
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
        switch (descriptionType)
        {
            case DescriptionType.Confirm:
                yesButton.onClick.AddListener(ChangeToDefaultUI);
                noButton.gameObject.SetActive(false);
                break;
            case DescriptionType.YesOrNo:
                if (yesCallback != null)
                {
                    yesButton.onClick.AddListener(() => { yesCallback.Invoke(); ChangeToDefaultUI(); });
                }
                else
                {
                    yesButton.onClick.AddListener(ChangeToDefaultUI);
                }

                if (noCallback != null)
                {
                    noButton.onClick.AddListener(() => { noCallback.Invoke(); ChangeToDefaultUI(); });
                }
                else
                {
                    noButton.onClick.AddListener(ChangeToDefaultUI);
                }
                
                noButton.gameObject.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(descriptionType), descriptionType, null);
        }
    }
    
    
    private void Awake()
    {
        Debug.Assert(descriptionText);
        Debug.Assert(yesButton);
        Debug.Assert(noButton);
    }

    private void ChangeToDefaultUI()
    {
        UIManager.Instance.ChangeUI(UIManager.defaultUI);
    }
    
    private void SetText(string message)
    {
        descriptionText.text = message;
    }
}
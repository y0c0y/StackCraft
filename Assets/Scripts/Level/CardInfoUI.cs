using System;
using TMPro;
using UnityEngine;

public class CardInfoUI : MonoBehaviour
{
    public static CardInfoUI Instance;
    
    [SerializeField] private TMP_Text descriptionText;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }

    public void ShowStackInfo(Card card)
    {
        var stack = card.owningStack;
        string output = "";
        foreach (var entry in stack.CardCounts)
        {
            output += $"{entry.Key.description} x {entry.Value} ";
        }
            
        descriptionText.text = output;
    }

    public void HideStackInfo()
    {
        descriptionText.text = "";
    }
}

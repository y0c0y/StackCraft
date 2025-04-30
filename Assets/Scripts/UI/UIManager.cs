using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static event Action<bool> OnUIChanged;
    
    public static UIManager Instance;
    
    [SerializeField] private List<Canvas> canvasUI;
    [SerializeField] private DescriptionUI descriptionUI;
    private Dictionary<string, Canvas> canvasDict;
    public static string defaultUI = "Level Canvas";
    
    public string currentUI {get; private set;}
    public bool isDefaultUI {get; private set;}
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        canvasDict = new Dictionary<string, Canvas>();
        foreach (var canvas in canvasUI)
        {
            canvasDict.Add(canvas.name, canvas);
        }
    }

    private void Start()
    {
        currentUI = defaultUI;
        isDefaultUI = true;
    }

    public void ChangeUI(string canvasName)
    {
        foreach (var kv in canvasDict)
        {
            if (kv.Key == canvasName)
            {
                kv.Value.enabled = true;
                currentUI = canvasName;
                isDefaultUI = (canvasName == defaultUI);
            }
            else
            {
                kv.Value.enabled = false;
            }
        }
        
        OnUIChanged?.Invoke(isDefaultUI);
    }

    public void OpenConfirmMessage(string message, Action yesCallback = null)
    {
        descriptionUI.SetDescription(DescriptionUI.DescriptionType.Confirm, message, yesCallback);
        ChangeUI("Description Canvas");
    }
    
    public void OpenYesOrNoMessage(string message, Action yesCallback = null, Action noCallback = null)
    {
        descriptionUI.SetDescription(DescriptionUI.DescriptionType.YesOrNo, message, yesCallback, noCallback);
        ChangeUI("Description Canvas");
    }
}

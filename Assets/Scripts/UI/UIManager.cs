using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{ 
    public static UIManager Instance;
    
    [SerializeField] private List<GameObject> canvasUI;
    private Dictionary<string, GameObject> canvasDict;
    private const string defaultUI = "Level Canvas";
    
    public string currentUI {get; private set;}
    public bool isDefaultUI {get; private set;}
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        canvasDict = new Dictionary<string, GameObject>();
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
                kv.Value.SetActive(true);
                currentUI = canvasName;
                isDefaultUI = (canvasName == defaultUI);
            }
            else
            {
                kv.Value.SetActive(false);
            }
        }
    }
}

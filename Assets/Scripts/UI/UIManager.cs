using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{ 
    public static UIManager Instance;
    
    [SerializeField] private List<GameObject> canvasUI;
    private Dictionary<string, GameObject> canvasDict;
    
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

    public void ChangeUI(string canvasName)
    {
        foreach (var kv in canvasDict)
        {
            kv.Value.SetActive(kv.Key == canvasName);
        }
    }
}

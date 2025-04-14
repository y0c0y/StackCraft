using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SidebarController : MonoBehaviour
{
    
    public RectTransform sidebarGroup;
    public float slideAmount;
    public float slideSpeed;
    
    public TMP_Text toggleButtonText;
    
    private bool _isOpen = true;
    private Vector2 _targetPos;
    private void Start()
    {
       _targetPos = sidebarGroup.anchoredPosition;
       toggleButtonText.text = _isOpen ? "<" : ">";
    }

    private void Update()
    {
        sidebarGroup.anchoredPosition = Vector2.Lerp(sidebarGroup.anchoredPosition, 
           _targetPos, slideSpeed * Time.deltaTime);
    }

    public void OnButtonClick()
    {
        var offset = _isOpen? -slideAmount : slideAmount;
        _isOpen = !_isOpen;
        toggleButtonText.text = _isOpen ? "<" : ">";
        _targetPos += new Vector2(offset, 0);
        
        Debug.Log(_targetPos);
        
    }
    
}

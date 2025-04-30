using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceAction : MonoBehaviour
{
    [SerializeField] private GameObject stopPanel;
    private InputAction _spaceAction;
    
    private const string SPACE_STRING = "<Keyboard>/space";
    
    private void OnEnable()
    {
        _spaceAction = new InputAction(binding: SPACE_STRING);
        _spaceAction.performed += OnStop;
        _spaceAction.Enable();
    }
    
    private void OnDisable()
    {
        _spaceAction.performed -= OnStop;
        _spaceAction.Disable();
    }
    
    private void OnStop(InputAction.CallbackContext ctx)
    {
        if (!UIManager.Instance.isDefaultUI) return;
        
        if (TimeManager.Instance.IsPausedByUser)
            TimeManager.Instance.ResumeTimeByUser();
        else
            TimeManager.Instance.PauseTimeByUser();
    }
}

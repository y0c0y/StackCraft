using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceAction : MonoBehaviour
{
    [SerializeField] private GameObject stopPanel;
    private InputAction _spaceAction;
    
    private const string SPACE_STRING = "<Keyboard>/space";
    private bool isPaused = false;
    
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
        
        if (isPaused)
        {
            OnResume();
        }
        else
        {
            OnPause();
        }
    }

    private void OnResume()
    {
        isPaused = false;
        stopPanel.SetActive(false);
        TimeManager.Instance.ResumeTime();
    }

    private void OnPause()
    {
        if (isPaused) return;
        isPaused = true;
        stopPanel.SetActive(true);
        TimeManager.Instance.PauseTime();
    }
}

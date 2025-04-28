using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EscAction : MonoBehaviour
{
    private InputAction _cancleAction;
    
    [SerializeField] private string defaultUI;
    [SerializeField] private string escTargetUI;

    private const string CANCEL_STRING = "Cancel";

    private void OnEnable()
    {
        var cancel = InputSystem.actions.FindAction(CANCEL_STRING);

        if (cancel == null)
        {
            Debug.LogError("Cancel 액션이 없습니다! 이름 오타 확인 필요.");
            return;
        }

        cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        var cancel = InputSystem.actions.FindAction(CANCEL_STRING);
        if (cancel != null)
        {
            cancel.performed -= OnCancel;
        }
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (UIManager.Instance.currentUI == defaultUI)
        {
            UIManager.Instance.ChangeUI(escTargetUI);
        }
    }
}

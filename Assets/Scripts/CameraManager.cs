using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    private Camera cam;
    private InputAction scrollwheelAction;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        scrollwheelAction = InputSystem.actions.FindAction("ScrollWheel");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 scrollDelta = scrollwheelAction.ReadValue<Vector2>();
        if (scrollDelta.y != 0)
        {
            float zoomAmount = scrollDelta.y * 0.1f;
            cam.orthographicSize -= zoomAmount;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 2f, 10f);

        }
    }
}

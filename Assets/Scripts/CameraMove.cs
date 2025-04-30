using System;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask camLayerMask;
    [SerializeField] private BoxCollider moveArea;
    
    [SerializeField] private float scrollSpeed = 6f;
    [SerializeField] private float minFOV = 55f;
    [SerializeField] private float maxFOV = 120f;

    private InputAction pointAction;
    private InputAction clickAction;
    private InputAction scrollwheelAction;

    private bool isDragging = false;
    private bool isDraggingCard = false;
    private float baseFOV = 80f;
    private float targetFOV = 80f;

    private float zoomInTargetFOV = 75f;
    private float zoomInDuration = 0.5f;
    private bool isZoomingIn = false;
    
    private Vector2 lastMousePosition;

    public void ZoomIn()
    {
        DOTween.To(() => cam.Lens.FieldOfView,
                x => cam.Lens.FieldOfView = x,
                zoomInTargetFOV,
                zoomInDuration)
               .SetEase(Ease.InQuad)
               .OnStart(() =>
               {
                   targetFOV = zoomInTargetFOV;
                   isZoomingIn = true;
               })
               .OnComplete(() => isZoomingIn = false)
               .SetLink(gameObject);
    }

    private void Awake()
    {
        pointAction = InputSystem.actions.FindAction("Point");
        clickAction = InputSystem.actions.FindAction("Click");
        scrollwheelAction = InputSystem.actions.FindAction("ScrollWheel");

        isDragging = false;
        isDraggingCard = false;
        
        pointAction?.Enable();
        clickAction?.Enable();
        scrollwheelAction?.Enable();

        GameTableManager.Instance.FieldChanged += OnFieldChanged;
    }

    private void Update()
    {
        if (!UIManager.Instance.isDefaultUI) return;
        if (cam.IsParticipatingInBlend()) return;
        HandleDrag();
        HandleScroll();
        MoveFov();
    }

    private void HandleDrag()
    {
        if (clickAction == null || pointAction == null)
            return;
        
        bool isPressed = clickAction.ReadValue<float>() > 0;

        if (isPressed && !isDragging)
        {
            isDragging = true;
            lastMousePosition = pointAction.ReadValue<Vector2>();

            // 마우스 위치를 월드 좌표로 변환
            Vector3 mouseScreenPos = new Vector3(lastMousePosition.x, lastMousePosition.y, Mathf.Abs(Camera.main.transform.position.z));
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            Vector2 origin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            // 2D 레이캐스트 수행
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, 11f, camLayerMask);
            
            isDraggingCard = (hit.collider != null);
        }
        else if (!isPressed && isDragging)
        {
            isDragging = false;
            isDraggingCard = false;
        }

        if (isDragging && !isDraggingCard)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                lastMousePosition = pointAction.ReadValue<Vector2>();
                return;
            }

            Vector2 currentMousePosition = pointAction.ReadValue<Vector2>();
            Vector2 delta = currentMousePosition - lastMousePosition;
            float fovScale = cam.Lens.FieldOfView / baseFOV;
            delta *= 0.01f * fovScale;

            Vector3 newPosition = target.position - new Vector3(delta.x, delta.y, 0f);
            Bounds bounds = moveArea.bounds;
            
            newPosition.x = Mathf.Clamp(newPosition.x, bounds.min.x, bounds.max.x);
            newPosition.y = Mathf.Clamp(newPosition.y, bounds.min.y, bounds.max.y);

            target.position = newPosition;
            lastMousePosition = currentMousePosition;
        }
    }

    private void HandleScroll()
    {
        if (EventSystem.current.IsPointerOverGameObject()) 
        {
            var uiInputModule = EventSystem.current.currentInputModule as InputSystemUIInputModule;
            var result = uiInputModule?.GetLastRaycastResult(Pointer.current.deviceId);
            if (!result?.gameObject.GetComponent<Card>()) return;
        }
        
        Vector2 scrollDelta = scrollwheelAction.ReadValue<Vector2>();
        if (scrollDelta.y != 0)
        {
            float zoomAmount = scrollDelta.y * scrollSpeed / 2f;
            targetFOV -= zoomAmount;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        }
    }
    
    private void MoveFov()
    {
        if (isZoomingIn) return;
        if (Mathf.Approximately(cam.Lens.FieldOfView, targetFOV)) return;
        
        cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFOV, scrollSpeed * Time.deltaTime);
    }
    
    private void OnFieldChanged(Field field)
    {
        if (field == null) return;

        moveArea = field.confineArea;
        var targetPosition = target.position;
        targetPosition.x = field.transform.position.x;
        targetPosition.y = field.transform.position.y;
        target.position = targetPosition;
    }
}

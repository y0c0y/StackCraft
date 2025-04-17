using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardDrag : MonoBehaviour
{
    public event Action<Card> CardDragStarted;
    public event Action<Card> CardDragEnded;

    private const float SELECTED_Z_OFFSET = -0.5f;
    
    [SerializeField] private float speed = 30f;
    
    private Vector3 _targetPosition;
    
    private Card _card;
    private InputAction _pointAction;
    private Vector2 _dragOrigin;
    private bool _isDragging = false;
    private bool _wasDragging = false;

    private void Awake()
    {
        _card = GetComponent<Card>();
        _pointAction = InputSystem.actions.FindAction("Point");
        _targetPosition = transform.position;
    }

    private void Update()
    {
        if (_card.IsChild)
        {
            if (_isDragging || _wasDragging)
            {
                _isDragging = false;
                _wasDragging = false;
                CardDragEnded?.Invoke(_card);
            }
            return;
        }

        if (_isDragging)
        {
            if (!_wasDragging)
            {
                CardDragStarted?.Invoke(_card);
            }
            _wasDragging = true;
            
            var mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 screenPos = Camera.main.ScreenToWorldPoint(mousePos);
            screenPos.z = SELECTED_Z_OFFSET;
            
            _targetPosition = screenPos - new Vector3(_dragOrigin.x * transform.localScale.x,
                _dragOrigin.y * transform.localScale.y, 0);
        }
        else
        {
            if (_wasDragging)
            {
                _wasDragging = false;
                CardDragEnded?.Invoke(_card);
            }
        }

        transform.position = Vector3.Lerp(transform.position, _targetPosition, speed * Time.deltaTime);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var inverseTransformPoint = transform.InverseTransformPoint(eventData.pointerPressRaycast.worldPosition);
        _dragOrigin = new Vector2(inverseTransformPoint.x, inverseTransformPoint.y);
        _isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
        var nowPos = transform.position;
        nowPos.z = 0f;
        _targetPosition = nowPos;
    }
}

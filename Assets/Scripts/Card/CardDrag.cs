using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardDrag : MonoBehaviour
{
    public event Action<Card> CardDragStarted;
    public event Action<Card> CardDragEnded;
    
    private Card _card;
    private InputAction _pointAction;
    private Vector2 _dragOrigin;
    private bool _isDragging = false;
    private bool _wasDragging = false;

    private void Awake()
    {
        _card = GetComponent<Card>();
        _pointAction = InputSystem.actions.FindAction("Point");
    }

    private void Update()
    {
        if (_card.IsChild) return;

        if (_isDragging)
        {
            if (!_wasDragging)
            {
                CardDragStarted?.Invoke(_card);
            }
            _wasDragging = true;
            
            Vector2 movement = _pointAction.ReadValue<Vector2>();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(movement);
            mousePos.z = 0;

            transform.position = mousePos - new Vector3(_dragOrigin.x * transform.localScale.x,
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
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // TODO: Maybe use a better way to get the transform?
        var inverseTransformPoint = transform.InverseTransformPoint(eventData.pointerPressRaycast.worldPosition);
        _dragOrigin = new Vector2(inverseTransformPoint.x, inverseTransformPoint.y);
        _isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
    }
}

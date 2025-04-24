using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDrag : MonoBehaviour
{
    public event Action<Card> CardDragStarted;
    public event Action<Card> CardDragEnded;

    private const float SELECTED_Z_OFFSET = -0.5f;
    private const float DRAG_TELEPORT_THRESHOLD = 0.05f;
    
    [SerializeField] private float speed = 30f;
    
    private Vector3 _targetPosition;
    
    private Card _card;
    private Vector2 _dragOrigin;
    private bool _isDragging = false;
    private bool _wasDragging = false;

    private Vector2 _fieldSize;
    private Vector2 _cardClamp;
    private const float CARD_CLAMP_OFFSET = 0.5f;
    
    private void Awake()
    {
        _card = GetComponent<Card>();
        var field = GameObject.FindGameObjectWithTag("Field");
        if (field)
        {
            _fieldSize = field.GetComponent<SpriteRenderer>().size;
        }
        else
        {
            _fieldSize = new Vector2(36, 24);
        }
        
        _targetPosition = transform.position;
    }

    private void Update()
    {
        // 가장 위쪽 카드가 아닐시 위치는 SlowParentConstraint에서 관리함
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
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, -_fieldSize.x / 2 + Card.CARD_SIZE.x / 2 + CARD_CLAMP_OFFSET,
                                                                _fieldSize.x / 2 - Card.CARD_SIZE.x / 2 - CARD_CLAMP_OFFSET);
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, -_fieldSize.y / 2 + Card.CARD_SIZE.y / 2 + CARD_CLAMP_OFFSET,
                                                                _fieldSize.y / 2 - Card.CARD_SIZE.y / 2 - CARD_CLAMP_OFFSET);
        }
        else
        {
            if (_wasDragging)
            {
                _wasDragging = false;
                CardDragEnded?.Invoke(_card);
            }
        }

        if (Vector3.SqrMagnitude(transform.position - _targetPosition) > DRAG_TELEPORT_THRESHOLD)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, speed * Time.deltaTime);
        }
        else
        {
            var pos = transform.position;
            pos.z = _targetPosition.z;
            transform.position = pos;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var inverseTransformPoint = transform.InverseTransformPoint(eventData.pointerPressRaycast.worldPosition);
        _dragOrigin = new Vector2(inverseTransformPoint.x, inverseTransformPoint.y);
        _card.owningStack?.ReorderZOrder(1);
        
        _isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        var nowPos = transform.position;
        nowPos.z = 0f;
        _targetPosition = nowPos;
        _card.owningStack?.ReorderZOrder(0);
        
        _isDragging = false;
    }
}

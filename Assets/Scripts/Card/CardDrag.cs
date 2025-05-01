using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDrag : MonoBehaviour
{
    public event Action<Card> CardDragStarted;
    public event Action<Card> CardDragEnded;

    private const float SELECTED_Z_OFFSET = -0.25f;
    private const float DRAG_TELEPORT_THRESHOLD = 0.05f;
    private const float SPEED = 30f;
    
    private Vector3 _targetPosition;
    
    private Card _card;
    private Field _currentField => _card.owningStack.currentField;
    private Vector2 _dragOrigin;
    private bool _isDragging = false;
    private bool _wasDragging = false;

    private Tween _dragOffTween;

    private const float CARD_CLAMP_OFFSET = 0.35f;
    
    private void Awake()
    {
        _card = GetComponent<Card>();
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
                AudioManager.PlaySound(SoundType.PICKUP);
            }
            _wasDragging = true;
            
            var mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 screenPos = Camera.main.ScreenToWorldPoint(mousePos);
            screenPos.z = SELECTED_Z_OFFSET;
            
            _targetPosition = screenPos - new Vector3(_dragOrigin.x * transform.localScale.x,
                _dragOrigin.y * transform.localScale.y, 0);
            _targetPosition.x = Mathf.Clamp(
                                    _targetPosition.x, 
                                _currentField.MinX + Card.CARD_SIZE.x / 2 + CARD_CLAMP_OFFSET,
                                _currentField.MaxX - Card.CARD_SIZE.x / 2 - CARD_CLAMP_OFFSET
                                    );
            _targetPosition.y = Mathf.Clamp(
                                    _targetPosition.y, 
                                _currentField.MinY + _card.owningStack.bounds.size.y - Card.CARD_SIZE.y / 2 + CARD_CLAMP_OFFSET,
                                _currentField.MaxY - Card.CARD_SIZE.y / 2 - CARD_CLAMP_OFFSET
                                    );
            
            if (Vector3.SqrMagnitude(transform.position - _targetPosition) > DRAG_TELEPORT_THRESHOLD)
            {
                transform.position = Vector3.Lerp(transform.position, _targetPosition, SPEED * Time.unscaledDeltaTime);
                Physics2D.SyncTransforms();
            }
            else
            {
                var pos = transform.position;
                pos.z = _targetPosition.z;
                transform.position = pos;
            }
        }
        else
        {
            if (_wasDragging)
            {
                _wasDragging = false;
                CardDragEnded?.Invoke(_card);
                AudioManager.PlaySound(SoundType.PICKDOWN);

                _dragOffTween?.Kill();
                _dragOffTween = transform.DOMoveZ(_targetPosition.z, 0.2f)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() =>
                    {
                        _dragOffTween = null;
                    })
                    .SetLink(gameObject);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var inverseTransformPoint = transform.InverseTransformPoint(eventData.pointerPressRaycast.worldPosition);
        _dragOrigin = new Vector2(inverseTransformPoint.x, inverseTransformPoint.y);
        _card.owningStack?.ReorderZOrder(2);
        
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

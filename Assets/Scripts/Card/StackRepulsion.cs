using UnityEngine;
using System.Linq;

public class StackRepulsion : MonoBehaviour
{
    private const float DAMPING = 0.85f;
    private const float REPULSION_FORCE = 110f;
    private const float XY_BOUNCE_ELASITICTY = 0.35f;
    
    public Vector3 velocity;
    
    private Stack _stack;
    private Card TopCard => _stack.TopCard;
    private Bounds bounds => _stack.bounds;
    private Field _currentField => _stack.currentField;
    
    private Vector3 _netForceThisFrame;
    
    private void Awake()
    {
        _stack = GetComponent<Stack>();
    }

    private void Update()
    {
        if (!TopCard) return;
        if (TopCard.gameObject.layer == LayerMask.NameToLayer("DraggingCard")) return;
        ApplyRepulsion();
        
        velocity += _netForceThisFrame * Time.unscaledDeltaTime;
        velocity *= DAMPING;
        if (!TopCard.IsChild && velocity.magnitude > 0.05f)
        {
            TopCard.transform.position += velocity * Time.unscaledDeltaTime;
            ApplyBoundAndBounce();
            Physics2D.SyncTransforms();
        }
        else
        {
            velocity = Vector3.zero;
        }
        
        _netForceThisFrame = Vector3.zero;
    }

    private void ApplyBoundAndBounce()
    {
        var currentPosition = TopCard.transform.position;
        if (bounds.min.x < _currentField.MinX)
        {
            velocity.x *= -1f * XY_BOUNCE_ELASITICTY;
            TopCard.transform.position = new Vector3(_currentField.MinX + Card.CARD_SIZE.x / 2f, currentPosition.y, currentPosition.z);
        }
        else if (bounds.max.x > _currentField.MaxX)
        {
            velocity.x *= -1f * XY_BOUNCE_ELASITICTY;
            TopCard.transform.position = new Vector3(_currentField.MaxX - Card.CARD_SIZE.x / 2f, currentPosition.y, currentPosition.z);
        }
        if (bounds.min.y < _currentField.MinY)
        {
            velocity.y *= -1f * XY_BOUNCE_ELASITICTY;
            TopCard.transform.position = new Vector3(currentPosition.x, _currentField.MinY + bounds.size.y - Card.CARD_SIZE.y / 2f, currentPosition.z);
        }
        else if (bounds.max.y > _currentField.MaxY)
        {
            velocity.y *= -1f * XY_BOUNCE_ELASITICTY;
            TopCard.transform.position = new Vector3(currentPosition.x, _currentField.MaxY - Card.CARD_SIZE.y / 2f, currentPosition.z);
        }
    }

    private void ApplyRepulsion()
    {
        var stacksOnTable = GameTableManager.Instance.GetAllStacksInField(_stack.currentField);

        const float checkDistanceRange = 100f;
        var closeStacks = 
            stacksOnTable.Where(s => s != _stack && s.TopCard?.gameObject.layer != LayerMask.NameToLayer("DraggingCard")
                                               && (s.bounds.center - _stack.bounds.center).magnitude <= checkDistanceRange);
        
        var myBounds = _stack.bounds;
        var myCenter = myBounds.center;
        Vector3 myCenter2D = new Vector3(myCenter.x, myCenter.y, 0f);
        
        foreach (var s in closeStacks)
        {
            if (!s.bounds.Intersects(_stack.bounds)) continue;

            var otherBounds = s.bounds;
            
            float overlapX = (myBounds.extents.x + otherBounds.extents.x) - Mathf.Abs(myCenter.x - otherBounds.center.x);
            float overlapY = (myBounds.extents.y + otherBounds.extents.y) - Mathf.Abs(myCenter.y - otherBounds.center.y);

            overlapX = Mathf.Max(0f, overlapX);
            overlapY = Mathf.Max(0f, overlapY);

            Vector2 penetrationVector = new Vector2(overlapX, overlapY);
            float penetrationMagnitude = penetrationVector.magnitude;

            var minimalMovement = 0.05f;
            float proximityFactor = Mathf.Clamp01((penetrationMagnitude / (myBounds.size.y + Mathf.Epsilon)) + minimalMovement);

            Vector3 otherCenter2D = new Vector3(otherBounds.center.x, otherBounds.center.y, 0f);
            Vector3 direction = myCenter2D - otherCenter2D;

            if (direction.sqrMagnitude < 0.001f)
            {
                // 방향 계산 실패시 우측으로
                direction = Vector3.right;
            }
            direction.Normalize();

            float forceMagnitude = REPULSION_FORCE * proximityFactor;

            ApplyForce(direction * forceMagnitude);

            var otherRepulsion = s.GetComponent<StackRepulsion>();
            if (otherRepulsion != null)
            {
                otherRepulsion.ApplyForce(-direction * forceMagnitude);
            }
        }
    }

    public void ApplyForce(Vector3 force)
    {
        _netForceThisFrame += force;
    }
    
}

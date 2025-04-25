using UnityEngine;
using System.Linq;

public class StackRepulsion : MonoBehaviour
{
    private const float DAMPING = 0.85f;
    private const float REPULSION_FORCE = 110f;
    private const float XZ_BOUNCE_ELASITICTY = 0.35f;

    public Vector2 MinBounds = new Vector2(-18f, -12f);
    public Vector2 MaxBounds = new Vector2(18f, 12f);
    
    public Vector3 velocity;
    private Stack _stack;
    private Card TopCard => _stack.TopCard;
    private Bounds bounds => _stack.bounds;
    
    private Vector3 _netForceThisFrame;
    
    private void Awake()
    {
        _stack = GetComponent<Stack>();
        
        var field = GameObject.FindGameObjectWithTag("Field");
        if (field)
        {
            var fieldSize = field.GetComponent<SpriteRenderer>().size;
            MinBounds = new Vector2(-fieldSize.x / 2, -fieldSize.y / 2);
            MaxBounds = new Vector2(fieldSize.x / 2, fieldSize.y / 2);
        }
    }

    private void FixedUpdate()
    {
        if (!TopCard) return;
        if (TopCard.gameObject.layer == LayerMask.NameToLayer("DraggingCard")) return;
        ApplyRepulsion();
        
        velocity += _netForceThisFrame * Time.fixedUnscaledDeltaTime;
        velocity *= DAMPING;
        if (!TopCard.IsChild && velocity.magnitude > 0.05f)
        {
            TopCard.transform.position += velocity * Time.fixedUnscaledDeltaTime;
            ApplyBounce();
        }
        else
        {
            velocity = Vector3.zero;
        }
        
        _netForceThisFrame = Vector3.zero;
    }

    private void ApplyBounce()
    {
        var currentPosition = TopCard.transform.position;
        if (bounds.min.x < MinBounds.x)
        {
            velocity.x *= -1f * XZ_BOUNCE_ELASITICTY;
            TopCard.transform.position = new Vector3(MinBounds.x + Card.CARD_SIZE.x / 2f, currentPosition.y, currentPosition.z);
        }
        else if (bounds.max.x > MaxBounds.x)
        {
            velocity.x *= -1f * XZ_BOUNCE_ELASITICTY;
            TopCard.transform.position = new Vector3(MaxBounds.x - Card.CARD_SIZE.x / 2f, currentPosition.y, currentPosition.z);
        }
        if (bounds.min.y < MinBounds.y)
        {
            velocity.y *= -1f * XZ_BOUNCE_ELASITICTY;
            TopCard.transform.position = new Vector3(currentPosition.x, MinBounds.y + bounds.size.y - Card.CARD_SIZE.y / 2f, currentPosition.z);
        }
        else if (bounds.max.y > MaxBounds.y)
        {
            velocity.y *= -1f * XZ_BOUNCE_ELASITICTY;
            TopCard.transform.position = new Vector3(currentPosition.x, MaxBounds.y - Card.CARD_SIZE.y / 2f, currentPosition.z);
        }
    }

    private void ApplyRepulsion()
    {
        var stacksOnTable = GameTableManager.Instance.stacksOnTable;
        
        var closeStacks = 
            stacksOnTable.Where(s => s != _stack && s.TopCard?.gameObject.layer != LayerMask.NameToLayer("DraggingCard")
                                               && (s.bounds.center - _stack.bounds.center).magnitude <= 100f);
        
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

using System;
using System.Linq;
using UnityEngine;

public class CardRepulsion : MonoBehaviour
{
    [SerializeField] private float damping = 0.85f;
    [SerializeField] private float repulsionForce = 1000f;
    
    private Card _card;
    public Vector3 velocity;
    private Vector3 _netForceThisFrame;

    private void Awake()
    {
        _card = GetComponent<Card>();
    }

    private void FixedUpdate()
    {
        if (transform.hasChanged)
        {
            ApplyRepulsion();
            transform.hasChanged = false;
        }
        
        velocity += _netForceThisFrame;
        velocity *= damping;
        //Debug.Log($"{name} velocity: {velocity}");
        if (velocity.magnitude > 0.05f)
        {
            transform.position += velocity * Time.fixedUnscaledDeltaTime;
        }
        else
        {
            velocity = Vector3.zero;
        }
        
        _netForceThisFrame = Vector3.zero;
    }

    private void ApplyRepulsion()
    {
        // if in stack, skip
        if (_card.IsChild) return;
        if (_card.gameObject.layer == LayerMask.NameToLayer("DraggingCard")) return;

        var cardsOnTable = GameTableManager.Instance.cardsOnTable;

        var closeCards =
            cardsOnTable.Where(c => c != _card && !c.IsChild && c.owningStack != _card.owningStack
                                               && (transform.position - c.transform.position).magnitude <= 4.5f);

        foreach (var c in closeCards)
        {
            var otherPosition = new Vector3(c.transform.position.x, c.transform.position.y, 0f);
            var thisPosition = new Vector3(transform.position.x, transform.position.y, 0f);
            var direction = otherPosition - thisPosition;
            float proximityFactor = 1f - (direction.magnitude / 4.5f);
            float forceMagnitude = repulsionForce * proximityFactor;
            
            if (direction.magnitude < 0.05f) continue;

            ApplyForce(-direction.normalized * forceMagnitude);
            
            var cRepulsion = c.GetComponent<CardRepulsion>();
            if (cRepulsion)
            {
                cRepulsion.ApplyForce(direction.normalized * forceMagnitude);
            }
        }
    }

    public void ApplyForce(Vector3 force)
    {
        _netForceThisFrame += force;
    }
}

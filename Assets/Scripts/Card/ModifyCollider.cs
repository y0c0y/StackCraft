using System;
using System.Collections;
using UnityEngine;

public class ModifyCollider : MonoBehaviour
{
    private Card myCard;
    private BoxCollider2D boxCollider;
    private Vector2 OtherOffset = new Vector2(0f, 1.65f);
    private Vector2 IdleOffset = new Vector2(0f, 0f);
    private Vector2 OtherSize = new Vector2(3.05f, 0.75f);
    private Vector2 IdleSize = new Vector2(3.05f, 4.05f);

    private void Awake()
    {
        myCard = GetComponent<Card>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private IEnumerator Start()
    {
        yield return null;
        SetIdle();
        Register();
    }
    
    private void Register()
    {
        CardColliderManager.Instance?.Register(myCard, this);
    }

    private void OnDisable()
    {
        CardColliderManager.Instance?.Unregister(myCard);
    }

    public void SetColliderMode(bool isTail)
    {
        if (isTail)
        {
            SetIdle();
        }
        else
        {
            SetOther();
        }
    }

    private void SetIdle()
    {
        boxCollider.size = IdleSize;
        boxCollider.offset = IdleOffset;
    }

    private void SetOther()
    {
        boxCollider.size = OtherSize;
        boxCollider.offset = OtherOffset;
    }
}

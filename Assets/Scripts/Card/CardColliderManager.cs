using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardColliderManager : MonoBehaviour
{
    public static CardColliderManager Instance;
    
    private Dictionary<Card, ModifyCollider> cardToCollider = new();
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        GameTableManager.Instance.StackAddedOnTable += AddStack;
        GameTableManager.Instance.StackRemovedFromTable += RemoveStack;
    }
    
    public void Register(Card card, ModifyCollider modifier)
    {
        if (!cardToCollider.ContainsKey(card))
        {
            cardToCollider.Add(card, modifier);
        }
    }

    public void Unregister(Card card)
    {
        cardToCollider.Remove(card);
    }

    private void AddStack(Stack stack)
    {
        stack.OnStackModified += ModifyColliders;
    }

    private void RemoveStack(Stack stack)
    {
        stack.OnStackModified -= ModifyColliders;
    }

    public void ModifyColliders(Stack stack)
    {
        if (stack.cards.Count <= 0) return;
        foreach (var card in stack.cards)
        {
            if (!cardToCollider.TryGetValue(card, out var modifier)) continue;
            modifier.SetColliderMode(card.IsLastCard);
        }
    }
}

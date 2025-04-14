using System;
using System.Collections.Generic;

[Serializable]
public class Stack
{
    public List<Card> cards = new List<Card>();
    public int Length => cards.Count;
    public Card TopCard => cards.Count > 0 ? cards[0] : null;
    public Card LastCard => cards.Count > 0 ? cards[^1] : null;

    public void Start()
    {
        GameTableManager.Instance.AddStackToTable(this);    
    }
    
    public void OnDestroy()
    {
        GameTableManager.Instance.RemoveStackFromTable(this);
    }
    
    public void AddCard(Card card)
    {
        cards.Add(card);
        card.owningStack = this;
        ReorderZOrder();
    }
    
    private void ReorderZOrder()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].SetSortingLayer(i * 3);
        }
    }
}

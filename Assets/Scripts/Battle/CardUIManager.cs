using System.Collections.Generic;
using UnityEngine;

public class CardUIManager : MonoBehaviour
{
	public static CardUIManager Instance;
	private Dictionary<Card, CardBattle> _cardToUI = new();

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
	}

	private void Start()
	{
		GameTableManager.Instance.StackAddedOnTable   += AddStack;
		GameTableManager.Instance.StackRemovedFromTable += RemoveStack;
	}

	private void OnDestroy()
	{
		GameTableManager.Instance.StackAddedOnTable   -= AddStack;
		GameTableManager.Instance.StackRemovedFromTable -= RemoveStack;
	}

	public void Register(Card card, CardBattle uiModifier)
		=> _cardToUI.TryAdd(card, uiModifier);

	public void Unregister(Card card)
		=> _cardToUI.Remove(card);

	private void AddStack(Stack stack)
		=> stack.OnStackModified += ModifyUIs;

	private void RemoveStack(Stack stack)
		=> stack.OnStackModified -= ModifyUIs;

	private void ModifyUIs(Stack stack)
	{
		if (stack.cards.Count <= 0) return;
		foreach (var card in stack.cards)
		{
			if (! _cardToUI.TryGetValue(card, out var modifier)) continue;
			modifier.ChangeBattleUI(card.IsLastCard);
		}
	}
}
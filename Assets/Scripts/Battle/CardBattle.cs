using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardBattle : MonoBehaviour
{
	public CardAbility ability;
	public CardBattleUI battleUI;

	private Card _card;
	private bool _stackSubscribed;
	
	private IEnumerator Start()
	{
		yield return null;
		Register();
	}
	
	public void Setup(Card card)
	{
		_card = card;

		ability.InitAbility(card.cardData);
		battleUI.Init(ability);

		card.GetComponent<CardDrag>().CardDragEnded += OnDragEnded;
	}

	private void Register()
	{
		CardUIManager.Instance?.Register(_card, this);
	}

	private void OnDisable()
	{
		CardUIManager.Instance?.Unregister(_card);
	}


	private void OnDestroy()
	{
		if (_card != null)
		{
			_card.GetComponent<CardDrag>().CardDragEnded -= OnDragEnded;
		}
	}

	private void OnDragEnded(Card card)
	{
		var battleMgr = BattleManager.Instance;

		if (card != _card) return;
		if (battleMgr.Flag(card)) return;
		
		var allStacksInField = GameTableManager.Instance.GetAllStacksInField(card.owningStack.currentField);
		
		foreach (var stack in allStacksInField)
		{
			if (stack == card.owningStack) continue;
			if (stack.TopCard == null) continue;
			if (stack.TopCard.cardData.cardType == card.cardData.cardType) continue;
			if (!battleMgr.IsValidCardType(stack.TopCard)) continue;
			if (battleMgr.Flag(stack.TopCard)) continue;
			if (!stack.bounds.Intersects(card.owningStack.bounds)) continue;

			if (card.cardData.cardType == CardType.Person)
				battleMgr.TryEngageBattle(card.owningStack, stack).Forget();
			else
				battleMgr.TryEngageBattle(stack, card.owningStack).Forget();
			break;
		}
	}

}
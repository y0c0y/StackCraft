using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardBattle : MonoBehaviour
{
	public CardAbility ability;
	public CardBattleUI battleUI;

	private Card _card;

	private Collider2D[] _results = new Collider2D[5];
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

		for (var i = 0; i < _results.Length; i++)
		{
			_results[i] = null;
		}

		var count = Physics2D.OverlapCollider(
			card.GetComponent<Collider2D>(),
			new ContactFilter2D().NoFilter(),
			_results);

		for (int i = 0; i < count; i++)
		{
			var other = _results[i].GetComponent<Card>();
			if (other == null) continue;
			if (!battleMgr.IsValidCardType(other)) continue;
			if (battleMgr.Flag(other)) continue;

			if (other.cardData.cardType == card.cardData.cardType) return;

			if (card.cardData.cardType == CardType.Person)
				battleMgr.TryEngageBattle(card.owningStack, other.owningStack).Forget();
			else
				battleMgr.TryEngageBattle(other.owningStack, card.owningStack).Forget();
			break;

		}
	}

}
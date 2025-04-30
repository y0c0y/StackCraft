using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardBattle : MonoBehaviour
{
	[SerializeField] private GameObject    cardBattleUIPrefab;
	
	private CardBattleUI _battleUI;
	private CardAbility _ability;
	private SpriteRenderer _artworkSprite;
	private Card         _card;
	
	public void Setup(Card card)
	{
		_card = card;

		_ability = gameObject.AddComponent<CardAbility>();
		_ability.InitAbility(card.cardData);

		var cardDataSprite = card.gameObject.GetComponentInChildren<CardDataSprite>();
		_artworkSprite = cardDataSprite.gameObject.GetComponentInChildren<SpriteRenderer>();
		var uiGO = Instantiate(cardBattleUIPrefab, _artworkSprite.transform);
		_battleUI = uiGO.GetComponent<CardBattleUI>();

		_battleUI.Init(_ability);
		
		Register();
		ChangeBattleUI(true);

		var drag = card.GetComponent<CardDrag>();
		
		if (card.cardData.cardType == CardType.Enemy)
		{
			drag.enabled = false;
			return;
		}
		
		drag.CardDragEnded += OnDragEnded;
	}
	 
	public async UniTask<bool> ReceiveDamage(int damage)
	{
		_ability.CurrentHp -= damage;
		
		if (_ability.CurrentHp <= 0) _ability.CurrentHp = 0;
		
		_battleUI.ChangeHpText(_ability.CurrentHp);
		
		await UniTask.Delay(300);
		
		return _ability.CurrentHp <= 0;
	}
	
	public int GetDamage() => _ability.TotalDamage;

	public void ChangeBattleUI(bool isTail)
	{
		_battleUI.ChangeUIEnabled(isTail);
	}

	public void ResetArtWorkLocalPos()
	{
		_artworkSprite.transform.localPosition = Vector3.zero;
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
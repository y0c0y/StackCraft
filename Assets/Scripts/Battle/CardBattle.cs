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
	}
	
	public async UniTask<bool> ReceiveDamage(int damage)
	{
		_ability.CurrentHp -= damage;
		
		_battleUI.ChangeHpText(_ability.CurrentHp);
		
		await UniTask.Delay(300);
		
		return _ability.CurrentHp <= 0;
	}
	
	public int GetDamage() => _ability.TotalDamage;

	public void ChangeBattleUI(bool isTail)
	{
		_battleUI.ChangeUIEnabled(isTail);
		_artworkSprite.gameObject.transform.localPosition = Vector3.zero;
	}
	
	private void Register()
	{
		CardUIManager.Instance?.Register(_card, this);
	}

	private void OnDisable()
	{
		CardUIManager.Instance?.Unregister(_card);
	}
	
	

}
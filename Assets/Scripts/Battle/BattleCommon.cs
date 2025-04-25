using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattleCommon
{
	
	public static bool IsValidCardType(Card card)
	{
		return card != null && 
		       (card.cardData.cardType == CardType.Person || card.cardData.cardType == CardType.None);
	}

	public static (List<Card> group, bool isEnemy) GetCardTargetList(Card card, List<Card> persons, List<Card> enemies)
	{
		if (!IsValidCardType(card))
			throw new ArgumentException($"Invalid card type: {card}");

		var isEnemy = card.cardData.cardType == CardType.None;
		return (isEnemy ? enemies : persons, isEnemy);
	}
	
	// public static void UpdateCardHpUI(Card card, int hp)
	// {
	// 	card?.UpdateHpBar(hp);
	// }
}
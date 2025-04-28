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
	
	// public static void UpdateCardHpUI(Card card, int hp)
	// {
	// 	card?.UpdateHpBar(hp);
	// }
}
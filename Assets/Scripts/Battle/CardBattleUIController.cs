using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardBattleUIController : MonoBehaviour
{
	[Header("UI Elements")]
	public Canvas canvas;
	
	public Image damageImage;
	public TMP_Text damageText;
	
	public Image hpImage;
	public TMP_Text hpText;
	

	public void Init(Card card,BattleAbility ba)
	{        
		damageText.text = ba.TotalDamage.ToString();
		hpText.text = ba.CurrentHp.ToString();
	}

	public void ChangeHpText(int hp)
	{
		hpText.text = hp.ToString();
	}
	
	
	
	
	
	
	
	
}
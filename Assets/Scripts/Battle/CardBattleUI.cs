using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardBattleUI : MonoBehaviour
{
	[Header("UI Elements")]
	public Canvas canvas;
	
	public Image damageImage;
	public TMP_Text damageText;
	
	public Image hpImage;
	public TMP_Text hpText;
	
	public void Init(CardAbility ba)
	{        
		damageText.text = ba.TotalDamage.ToString();
		hpText.text = ba.CurrentHp.ToString();
	}

	public void ChangeHpText(int hp)
	{
		hpText.text = hp.ToString();
	}
	
	public void ChangeUIEnabled(bool isTail)
	{
		canvas.enabled = isTail;
	}
	
}
using UnityEngine;

public class CardAbility : MonoBehaviour
{
	public int TotalDamage { get; private set; }
	public int CurrentHp   { get; set; }

	private void Init(int baseDmg, int weapDmg, int weapHp, int baseHp)
	{
		TotalDamage = baseDmg + weapDmg;
		CurrentHp   = baseHp + weapHp;
	}
	
	public void InitAbility(CardData data)
	{
		var type = data.sprite.name.Split('_');

		if (type.Length < 2)
		{
			Init(1, 0, 0, 5);
			return;
			
		}

		switch(type[1])
		{
			case "spear" :
				Init(1, 2, 1, 5);
				break;
				
			case "bow" :
				Init(1, 4, 0, 5);
				break;
			case "throw" :
				Init(1, 4, 20, 5);
				break;
		};
	}
}
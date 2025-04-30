public class BattleAbility
{
	public int TotalDamage { get; }
	public int CurrentHp   { get; set; }

	private BattleAbility(int totalDamage, int currentHp)
	{
		TotalDamage = totalDamage;
		CurrentHp   = currentHp;
	}
	
	public static BattleAbility Create(int baseDmg, int weapDmg, int weapHp, int baseHp)
	{
		return new BattleAbility(baseDmg + weapDmg, baseHp + weapHp);
	}

	public static BattleAbility FindAbility(CardData data)
	{
		var type = data.sprite.name.Split('_');
		if (type.Length < 2)
			return Create(1, 0, 0, 5);

		return type[1] switch
		{
			"spear" => Create(1, 2, 1, 5),
			"bow"   => Create(1, 4, 0, 5),
			"throw" => Create(1, 4,20, 5),
			_       => Create(1, 0, 0, 5)
		};
	}
}
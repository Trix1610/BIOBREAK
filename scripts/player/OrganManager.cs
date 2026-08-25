using Godot;
using Godot.Collections;

public partial class OrganManager : Node
{
	// Список всех экипированных/подобранных органов
	private Array<OrganData> _organs = new();

	// Метод добавления органа в список
	public void AddOrgan(OrganData organ)
	{
		if (organ == null) return;
		
		_organs.Add(organ);
		GD.Print($"[OrganManager] Подобран орган: {organ.Name} | Бонус к скорости: +{organ.BonusSpeed}");
	}

	// Метод суммирует бонус скорости от всех органов в списке
	public float GetTotalBonusSpeed()
	{
		float total = 0.0f;
		foreach (var organ in _organs)
		{
			total += organ.BonusSpeed;
		}
		return total;
	}

	// Метод суммирует бонус к дополнительным прыжкам от всех органов в списке
	public int GetTotalExtraJumps()
	{
		int extraJumps = 0;
		
		foreach (var organ in _organs)
		{
			extraJumps += organ.ExtraJumps;
		}

		return extraJumps;
	}
}

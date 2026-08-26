using Godot;

public partial class HealthUI : CanvasLayer
{
	[Export] public int MaxHealth { get; set; } = 6;
	[Export] public int CurrentHealth { get; set; } = 6;

	[Export] public Texture2D FullHeartTexture { get; set; }
	[Export] public Texture2D HalfHeartTexture { get; set; }
	[Export] public Texture2D EmptyHeartTexture { get; set; }

	private HBoxContainer _container;

	public override void _Ready()
	{
		// Находим наш HBoxContainer внутри CanvasLayer
		_container = GetNodeOrNull<HBoxContainer>("HBoxContainer");
		
		if (_container == null)
		{
			GD.PrintErr("HealthUI: Не найден узел HBoxContainer!");
		}

		UpdateHealth(CurrentHealth);
	}

	public void UpdateHealth(int newHealth)
	{
		CurrentHealth = Mathf.Clamp(newHealth, 0, MaxHealth);
		if (_container == null) return;

		var hearts = _container.GetChildren();

		for (int i = 0; i < hearts.Count; i++)
		{
			if (hearts[i] is TextureRect heartRect)
			{
				int heartValue = CurrentHealth - (i * 2);

				if (heartValue >= 2)
				{
					heartRect.Texture = FullHeartTexture;
				}
				else if (heartValue == 1)
				{
					heartRect.Texture = HalfHeartTexture;
				}
				else
				{
					heartRect.Texture = EmptyHeartTexture;
				}
			}
		}
	}
}

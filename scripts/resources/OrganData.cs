using Godot;
using Godot.Collections;

public enum OrganType
{
	Heart,
	Lungs,
	Nervous,
	Stomach,
	Eyes,
	Blood
}

[GlobalClass]
public partial class OrganData : Resource
{
	[Export] public string Id { get; set; } = "";
	[Export] public string Name { get; set; } = "";
	[Export] public OrganType Type { get; set; } = OrganType.Heart;
	
	[Export(PropertyHint.MultilineText)] 
	public string Description { get; set; } = "";

	[Export] public float BonusSpeed { get; set; } = 0.0f;
	[Export] public int ExtraJumps { get; set; } = 0;

	[Export] public Array<string> ElementTags { get; set; } = new();
}

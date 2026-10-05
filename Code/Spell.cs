using Godot;

public enum SpellType
{
	Fire,
	Lightning,
	Water
}

public abstract partial class Spell : Node
{
	[Export] public float Cooldown = 1.0f;
	[Export] public PackedScene ProjectileScene;

	public abstract SpellType Type { get; }
	public virtual bool IsContinuous => false;

	public abstract void Cast(Player caster, Vector3 origin, Vector3 direction);
}

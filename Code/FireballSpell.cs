using Godot;

public partial class FireballSpell : Spell
{
	[Export] public float BaseDamage = 15.0f;

	public override SpellType Type => SpellType.Fire;

	public override void Cast(Player caster, Vector3 origin, Vector3 direction)
	{
		if (caster.IsInWater)
		{
			GD.Print("Fireball fizzles out in the water!");
			return;
		}

		var projectile = ProjectileScene.Instantiate<Projectile>();
		projectile.SourceSpell = Type;
		projectile.CasterPlayer = caster;
		projectile.Damage = BaseDamage;

		caster.GetTree().CurrentScene.AddChild(projectile);
		projectile.GlobalPosition = origin;
		projectile.LookAt(origin + direction, Vector3.Up);
	}
}

using Godot;

public partial class LightningSpell : Spell
{
	[Export] public float BaseDamage = 10.0f;
	[Export] public float WetBonusMultiplier = 2.0f; // at 100% wet, adds 2x bonus damage

	public override SpellType Type => SpellType.Lightning;

	public override void Cast(Player caster, Vector3 origin, Vector3 direction)
	{
		var projectile = ProjectileScene.Instantiate<Projectile>();
		projectile.SourceSpell = Type;
		projectile.CasterPlayer = caster;
		projectile.Damage = BaseDamage;
		projectile.LightningWetBonus = WetBonusMultiplier;

		caster.GetTree().CurrentScene.AddChild(projectile);
		projectile.GlobalPosition = origin;
		projectile.LookAt(origin + direction, Vector3.Up);

		if (caster.IsInWater)
		{
			GD.Print("Lightning arcs through the water - player takes self-damage!");
			// TODO: hook this into Player's own Health once you're ready for self-damage
		}
	}
}

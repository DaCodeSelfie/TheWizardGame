using Godot;
using System;

public partial class Projectile : Area3D
{
	[Export] public float Speed = 20.0f;
	[Export] public float Lifetime = 5.0f;

	public SpellType SourceSpell;
	public Player CasterPlayer;
	public float Damage = 0.0f;
	public float LightningWetBonus = 1.0f;

	private float _timeAlive = 0.0f;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 forward = -GlobalTransform.Basis.Z;
		GlobalPosition += forward * Speed * (float)delta;

		_timeAlive += (float)delta;
		if (_timeAlive >= Lifetime) QueueFree();
	}

	private void OnBodyEntered(Node3D body)
	{
		GD.Print("Projectile (", SourceSpell, ") hit: ", body.Name);

		if (body is Torch torchDirect) ApplyTorchEffect(torchDirect);
		else if (body.GetParent() is Torch torchParent) ApplyTorchEffect(torchParent);

		if (body is Rat rat)
		{
			if (SourceSpell == SpellType.Fire) rat.ApplyFireDamage(Damage);
			else if (SourceSpell == SpellType.Lightning) rat.ApplyLightningDamage(Damage, LightningWetBonus);
		}

		if (SourceSpell == SpellType.Lightning)
		{
			if (body is Gearbox gearbox) gearbox.Activate();
			if (body is BrokenGearbox brokenGearbox) brokenGearbox.Activate();
		}

		if (SourceSpell == SpellType.Fire)
		{
			if (body is HayStack hay) hay.Ignite();
			if (body is Engine engine && CasterPlayer != null) engine.TryActivate(CasterPlayer);
			if (body is CrackedWall crackedWall && CasterPlayer != null) crackedWall.TryActivate(CasterPlayer);
			if (body is LadderRope rope) rope.Burn();
			else if (body.GetParent() is Ladder ladder) ladder.IgniteFromHazard();
		}

		if (body is SpecialAutomaton automaton) automaton.RegisterHit();

		QueueFree();
	}

	private void ApplyTorchEffect(Torch torch)
	{
		if (SourceSpell == SpellType.Water) torch.Extinguish();
		else if (SourceSpell == SpellType.Fire) torch.Ignite();
	}
}

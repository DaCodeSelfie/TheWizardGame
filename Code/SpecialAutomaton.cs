using Godot;
using System;

public partial class SpecialAutomaton : StaticBody3D
{
	[Export] public PackedScene CogScene;
	[Export] public int HitsToKill = 3;

	private bool _isActive = false;
	private int _hitsTaken = 0;

	public void Activate()
	{
		if (_isActive) return;
		_isActive = true;
		GD.Print(Name, " activated and hostile!");
		// TODO: once real AI/health exists, replace this placeholder with proper behavior
	}

	public void RegisterHit()
	{
		if (!_isActive) return;

		_hitsTaken++;
		GD.Print(Name, " hit ", _hitsTaken, "/", HitsToKill);

		if (_hitsTaken >= HitsToKill)
		{
			Die();
		}
	}

	private void Die()
	{
		GD.Print(Name, " destroyed, dropping cog.");

		if (CogScene != null)
		{
			var cog = CogScene.Instantiate<RigidBody3D>();
			GetTree().CurrentScene.AddChild(cog);
			cog.GlobalPosition = GlobalPosition;
		}

		QueueFree();
	}
}

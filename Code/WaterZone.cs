using Godot;
using System;
using System.Collections.Generic;

public partial class WaterZone : Area3D
{
	[Export] public float TimeToFullyWet = 2.5f;

	private Dictionary<Rat, float> _ratTimers = new();

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	public override void _Process(double delta)
	{
		var keys = new List<Rat>(_ratTimers.Keys);
		foreach (var rat in keys)
		{
			if (!IsInstanceValid(rat)) { _ratTimers.Remove(rat); continue; }

			_ratTimers[rat] += (float)delta;
			if (_ratTimers[rat] >= TimeToFullyWet)
			{
				rat.ApplyFullWetness();
				_ratTimers.Remove(rat);
			}
		}
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is Player player)
		{
			player.IsInWater = true;
		}
		else if (body is Rat rat)
		{
			_ratTimers[rat] = 0f;
		}
	}

	private void OnBodyExited(Node3D body)
	{
		if (body is Player player)
		{
			player.IsInWater = false;
		}
		else if (body is Rat rat)
		{
			_ratTimers.Remove(rat);
		}
	}
}

using Godot;
using System;

public partial class Wetness : Node
{
	[Export] public float DecayRate = 0.15f; // fraction per second, drying off over time
	[Export] public float SprayFillDuration = 3.0f; // seconds of direct spray to reach 100%, same feel as WaterTank

	public float CurrentWetness { get; private set; } = 0.0f; // 0.0 to 1.0

	private bool _updatedThisFrame = false;

	public override void _Process(double delta)
	{
		if (!_updatedThisFrame && CurrentWetness > 0f)
		{
			CurrentWetness = Mathf.Max(0f, CurrentWetness - DecayRate * (float)delta);
		}
		_updatedThisFrame = false;
	}

	public void AddFromSpray(float delta)
	{
		CurrentWetness = Mathf.Clamp(CurrentWetness + delta / SprayFillDuration, 0f, 1f);
		_updatedThisFrame = true;
	}

	public void SetFullyWet()
	{
		CurrentWetness = 1.0f;
		_updatedThisFrame = true;
		GD.Print(GetParent().Name, " is now fully wet (100%).");
	}

	public void ReduceFromFire(float amount)
	{
		CurrentWetness = Mathf.Max(0f, CurrentWetness - amount);
		GD.Print(GetParent().Name, " steams off some wetness. Now: ", (CurrentWetness * 100f).ToString("F0"), "%");
	}
}

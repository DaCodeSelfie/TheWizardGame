using Godot;
using System;

public partial class WaterTank : StaticBody3D
{
	[Export] public float FillDuration = 3.0f; // seconds of continuous spray needed to fill
	[Export] public float DrainRate = 0.5f;     // how fast it drains per second if not actively sprayed

	private float _fillAmount = 0.0f; // 0.0 to 1.0
	private double _lastFillTime = 0.0;

	public bool IsFull { get; private set; } = false;

	public void FillWithWater(float delta)
	{
		if (IsFull) return;

		_fillAmount += delta / FillDuration;
		_fillAmount = Mathf.Clamp(_fillAmount, 0.0f, 1.0f);
		_lastFillTime = Time.GetTicksMsec() / 1000.0;

		GD.Print(Name, " filling: ", (_fillAmount * 100).ToString("F0"), "%");

		if (_fillAmount >= 1.0f && !IsFull)
		{
			IsFull = true;
			GD.Print(Name, " is now FULL.");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsFull) return;

		double timeSinceLastFill = (Time.GetTicksMsec() / 1000.0) - _lastFillTime;

		// If it hasn't been sprayed recently, slowly drain back down
		if (timeSinceLastFill > 0.2 && _fillAmount > 0.0f)
		{
			_fillAmount -= DrainRate * (float)delta;
			_fillAmount = Mathf.Clamp(_fillAmount, 0.0f, 1.0f);
		}
	}
}

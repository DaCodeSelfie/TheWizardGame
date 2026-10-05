using Godot;
using System;

public partial class HayStack : StaticBody3D
{
	[Export] public PackedScene GasolineScene;
	[Export] public float BurnDuration = 1.5f;

	private bool _isBurning = false;

	public void Ignite()
	{
		if (_isBurning) return;
		_isBurning = true;

		GD.Print(Name, " catches fire!");

		Timer timer = new Timer();
		AddChild(timer);
		timer.WaitTime = BurnDuration;
		timer.OneShot = true;
		timer.Timeout += OnBurnFinished;
		timer.Start();
	}

	private void OnBurnFinished()
	{
		GD.Print(Name, " burned away, dropping gasoline.");

		if (GasolineScene != null)
		{
			var gasoline = GasolineScene.Instantiate<Node3D>();
			GetTree().CurrentScene.AddChild(gasoline);
			gasoline.GlobalPosition = GlobalPosition;
		}

		QueueFree();
	}
}

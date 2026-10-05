using Godot;
using System;

public partial class GasolinePickup : Area3D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is Player player)
		{
			player.HasGasoline = true;
			GD.Print("Picked up gasoline!");
			QueueFree();
		}
	}
}

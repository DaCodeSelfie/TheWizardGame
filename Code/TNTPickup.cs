using Godot;
using System;

public partial class TNTPickup : Area3D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is Player player)
		{
			player.HasTNT = true;
			GD.Print("Picked up TNT!");
			QueueFree();
		}
	}
}

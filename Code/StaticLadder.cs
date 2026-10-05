using Godot;
using System;

public partial class StaticLadder : Node3D
{
	private Area3D _climbZone;
	
	public bool IsClimbable = true;
	
	public override void _Ready()
	{
		_climbZone = GetNode<Area3D>("ClimbZone");
	}
	
	public void OnClimbZoneBodyEntered(Node3D body)
	{
		if (body is Player player && IsClimbable)
		{
			player.CurrentStaticLadder = this;
			GD.Print("Player entered ladder climb zone.");
		}
	}
	
		public void OnClimbZoneBodyExited(Node3D body)
	{
		if (body is Player player && player.CurrentStaticLadder == this)
		{
			player.CurrentStaticLadder = null;
			GD.Print("Player left ladder climb zone.");
		}
	}
}

using Godot;
using System;

public partial class LadderRope : StaticBody3D
{
	[Export] public Ladder LinkedLadder;

	public void Burn()
	{
		GD.Print(Name, " rope burns through!");
		LinkedLadder?.Deploy();
		QueueFree();
	}
}

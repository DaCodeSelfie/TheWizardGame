using Godot;
using System;

public partial class GateTrigger : Area3D
{
	public enum TriggerAction
	{
		OpenGate,
		SealGate
	}

	[Export] public BossGate LinkedGate;
	[Export] public TriggerAction Action;

	public override void _Ready()
	{
		GD.Print(Name, " ready. LinkedGate=", (LinkedGate != null ? LinkedGate.Name : "NULL"), " Action=", Action);
	}

	public void OnBodyEntered(Node3D body)
	{
		GD.Print(Name, " detected body: ", body.Name, " InPlayerGroup=", body.IsInGroup("player"));

		if (!body.IsInGroup("player")) return;

		if (LinkedGate == null)
		{
			GD.PrintErr(Name, " has no LinkedGate assigned!");
			return;
		}

		if (Action == TriggerAction.OpenGate)
		{
			LinkedGate.Open();
		}
		else if (Action == TriggerAction.SealGate)
		{
			LinkedGate.SealShut();
		}
	}
}

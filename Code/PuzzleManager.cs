using Godot;
using System;
using System.Linq;

public partial class PuzzleManager : Node3D
{
	[Export] public Gate Gate1; // entrance - open only while ALL levers are down
	[Export] public Gate Gate2; // key gate - open once ALL levers are up
	[Export] public Godot.Collections.Array<Lever> Levers = new();

	[Signal] public delegate void AllLeversActivatedEventHandler();
	[Signal] public delegate void KeyCollectedEventHandler();

	public override void _Ready()
	{
		foreach (Lever lever in Levers)
		{
			lever.Toggled += OnLeverToggled;
		}

		UpdateGates();
		GD.Print("PuzzleManager ready. Levers registered: ", Levers.Count);
	}

	private void OnLeverToggled(Lever lever, bool isPulled)
	{
		UpdateGates();
	}

	private void UpdateGates()
	{
		bool anyPulled = Levers.Any(l => l.IsPulled);
		bool allPulled = Levers.All(l => l.IsPulled);

		GD.Print("Lever state changed. AnyPulled=", anyPulled, " AllPulled=", allPulled);

		if (anyPulled)
			Gate1.Close();
		else
			Gate1.Open();

		if (allPulled)
		{
			Gate2.Open();
			EmitSignal(SignalName.AllLeversActivated);
			GD.Print("All levers activated - key gate open!");
		}
		else
		{
			Gate2.Close();
		}
	}

	// Call this from your Key pickup script once the player grabs the key
	public void OnKeyCollected()
	{
		GD.Print("Key collected - waking automatons for the trip back.");
		EmitSignal(SignalName.KeyCollected);
		GetTree().CallGroup("automaton", "WakeUp");
	}
}

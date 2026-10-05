using Godot;
using System;

public partial class Engine : StaticBody3D
{
	[Export] public WaterTank LinkedTank;
	[Export] public SpecialAutomaton LinkedAutomaton;
	[Export] public NodePath FuelAttachPointPath = "FuelAttachPoint";

	private bool _playerInRange = false;
	private bool _hasFuelAttached = false;
	private Node3D _attachPoint;

	public bool HasFuel => _hasFuelAttached;

	public override void _Ready()
	{
		_attachPoint = GetNode<Node3D>(FuelAttachPointPath);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Interact") && _playerInRange && !_hasFuelAttached)
		{
			TryAttachFuel();
		}
	}

	private void TryAttachFuel()
	{
		Player player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null || player.HeldItem == null || player.HeldItem.ItemId != "Gasoline")
		{
			GD.Print(Name, " needs gasoline first.");
			return;
		}

		CarriableItem fuel = player.HeldItem;
		player.HeldItem = null;

		fuel.AttachTo(_attachPoint);

		_hasFuelAttached = true;
		GD.Print(Name, " fueled up!");
	}

	public void TryActivate(Player player)
	{
		if (LinkedTank == null || LinkedAutomaton == null)
		{
			GD.PrintErr(Name, " missing LinkedTank or LinkedAutomaton!");
			return;
		}

		if (LinkedTank.IsFull && _hasFuelAttached)
		{
			GD.Print(Name, " roars to life - automaton activating!");
			player.HeldItem = null;
			LinkedAutomaton.Activate();
		}
		else
		{
			GD.Print(Name, " sputters - missing requirements. TankFull=", LinkedTank.IsFull, " HasFuel=", _hasFuelAttached);
		}
	}

	public void OnInteractAreaBodyEntered(Node3D body)
	{
		if (body.IsInGroup("player")) _playerInRange = true;
	}

	public void OnInteractAreaBodyExited(Node3D body)
	{
		if (body.IsInGroup("player")) _playerInRange = false;
	}
}

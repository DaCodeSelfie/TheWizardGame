using Godot;
using System;

public partial class GearboxCogSlot : StaticBody3D
{
	[Export] public MovingPlatform LinkedPlatform;
	[Export] public NodePath CogAttachPointPath = "CogAttachPoint";

	private bool _playerInRange = false;
	private bool _isUsed = false;
	private Node3D _attachPoint;

	public override void _Ready()
	{
		_attachPoint = GetNode<Node3D>(CogAttachPointPath);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Interact") && _playerInRange && !_isUsed)
		{
			TryInsertCog();
		}
	}

	private void TryInsertCog()
	{
		Player player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null || player.HeldItem == null || player.HeldItem.ItemId != "Cog")
		{
			GD.Print(Name, " needs a cog first.");
			return;
		}

		CarriableItem cog = player.HeldItem;
		player.HeldItem = null;
		_isUsed = true;

		// Snap the cog into place on the gearbox instead of destroying it
		cog.AttachTo(_attachPoint);

		LinkedPlatform?.Toggle();
		GD.Print(Name, " cog inserted - platform lowering!");
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

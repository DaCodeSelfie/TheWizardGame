using Godot;
using System;

public partial class CrackedWall : StaticBody3D
{
	[Export] public NodePath TNTAttachPointPath = "TNTAttachpoint";
	
	private bool _playerInRange = false;
	private bool _hasTNTAttached = false;
	private Node3D _attachPoint;
	private CarriableItem _attachedTNT;

	public bool HasTNT => _hasTNTAttached;

	public override void _Ready()
	{
		_attachPoint = GetNode<Node3D>(TNTAttachPointPath);

		Area3D area = GetNode<Area3D>("InteractArea");

		GD.Print("Monitoring: ", area.Monitoring);
		GD.Print("Monitorable: ", area.Monitorable);

		CollisionShape3D shape = area.GetNode<CollisionShape3D>("CollisionShape3D");
		GD.Print("Disabled: ", shape.Disabled);
		GD.Print("Shape: ", shape.Shape);
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Interact") && _playerInRange && !_hasTNTAttached)
		{
			TryAttachTNT();
		}
	}
	
	private void TryAttachTNT()
	{
		Player player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null || player.HeldItem == null || player.HeldItem.ItemId != "TNT")
		{
			GD.Print("No Bomb");
			return;
		}
		
		CarriableItem tnt = player.HeldItem;
		player.HeldItem = null;
		
		tnt.AttachTo(_attachPoint);
		
		_hasTNTAttached = true;
		GD.Print("BoomBoom?");
	}
	
	public void TryActivate(Player player)
	{
		if (_hasTNTAttached)
		{
			GD.Print("BOOM! No more wall");
			QueueFree(); 
		}
		else
		{
			GD.Print("No boom:(");
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

using Godot;
using System;

public partial class CarriableItem : RigidBody3D
{
	[Export] public string ItemId = "Gasoline";
	[Export] public float HoldDistance = 1.5f;
	[Export] public float CarrySpringStrength = 20.0f;
	[Export] public float CarryDamping = 10.0f;
	[Export] public float ThrowForce = 8.0f;
	[Export] public float MaxCarryDistance = 3.0f; // if it gets yanked further than this (blocked by wall), drop it

	private bool _playerInRange = false;
	private bool _isHeld = false;
	private Player _holdingPlayer;
	private Camera3D _camera;

	public override void _Ready()
	{
		GD.Print(Name, " ready. ItemId=", ItemId);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Interact"))
		{
			GD.Print(Name, " Interact pressed. PlayerInRange=", _playerInRange, " IsHeld=", _isHeld);
		}

		if (@event.IsActionPressed("Interact") && _playerInRange && !_isHeld)
		{
			Player player = GetTree().GetFirstNodeInGroup("player") as Player;

			if (player == null)
			{
				GD.PrintErr(Name, " could not find Player in 'player' group!");
				return;
			}

			if (player.HeldItem != null)
			{
				GD.Print(Name, " player already holding something: ", player.HeldItem.Name);
				return;
			}

			PickUp(player);
		}

	// Right click to throw while held - unchanged
	if (_isHeld && @event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Right)
	{
		Drop(throwIt: true);
	}
}

	private void PickUp(Player player)
	{
		_isHeld = true;
		_holdingPlayer = player;
		_camera = player.GetNode<Camera3D>("Head/Camera3D");
		player.HeldItem = this;

		GravityScale = 0.0f; // temporarily weightless while carried, so the spring can control it cleanly
		LinearDamp = CarryDamping;

		GD.Print(Name, " picked up.");
	}
	
		public void AttachTo(Node3D attachPoint)
	{
		_isHeld = false; // no longer "held" by the player specifically
		Freeze = true;   // RigidBody3D property - stops all physics simulation on it
		Reparent(attachPoint);
		Position = Vector3.Zero;
		Rotation = Vector3.Zero;
		
		CollisionShape3D collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		collisionShape.Disabled = true;
		
		GD.Print(Name, " attached to ", attachPoint.Name);
	}

	public void Drop(bool throwIt)
	{
		_isHeld = false;
		if (_holdingPlayer != null)
		{
			_holdingPlayer.HeldItem = null;
		}

		GravityScale = 1.0f;
		LinearDamp = 0.0f;

		if (throwIt && _camera != null)
		{
			Vector3 throwDir = -_camera.GlobalTransform.Basis.Z;
			LinearVelocity = throwDir * ThrowForce;
			GD.Print(Name, " thrown!");
		}
		else
		{
			GD.Print(Name, " dropped.");
		}

		_holdingPlayer = null;
		_camera = null;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_isHeld || _camera == null) return;

		Vector3 targetPos = _camera.GlobalPosition + (-_camera.GlobalTransform.Basis.Z * HoldDistance);
		Vector3 toTarget = targetPos - GlobalPosition;

		// Safety: if something's blocking it and it's stuck too far away, drop it instead of clipping/yeeting through walls
		if (toTarget.Length() > MaxCarryDistance)
		{
			Drop(throwIt: false);
			return;
		}

		// Spring force pulling the item toward the hold point - this is what gives it "weight" and slight lag/sway
		Vector3 force = toTarget * CarrySpringStrength;
		ApplyCentralForce(force);
	}

	public void OnInteractAreaBodyEntered(Node3D body)
	{
		GD.Print(Name, " detected: ", body.Name);
		if (body.IsInGroup("player")) _playerInRange = true;
	}

	public void OnInteractAreaBodyExited(Node3D body)
	{
		if (body.IsInGroup("player")) _playerInRange = false;
	}
}

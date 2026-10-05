using Godot;
using System;

public partial class BrokenGearbox : StaticBody3D
{
	[Export] public MovingPlatform LinkedPlatform;
	[Export] public float ActivationCooldown = 0.5f;
	[Export] public NodePath CogAttachPointPath = "CogAttachPoint";

	private bool _hasCog = false;
	private bool _playerInRange = false;
	private float _cooldownTimer = 0.0f;

	private Node3D _attachPoint;
	private AnimationPlayer _animPlayer;
	private GpuParticles3D _smokeParticles;
	private GpuParticles3D _sparkParticles;
	private SparkLight _sparkLight;
	private Node3D _bigGear2;

	public override void _Ready()
	{
		_attachPoint = GetNode<Node3D>(CogAttachPointPath);
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_smokeParticles = GetNode<GpuParticles3D>("SmokeParticels");
		_sparkParticles = GetNode<GpuParticles3D>("SparkParticels");
		_sparkLight = GetNode<SparkLight>("SparkLight");
		_bigGear2 = GetNode<Node3D>("BigGear2");

		_smokeParticles.Emitting = false;
		_sparkParticles.Emitting = false;
		_bigGear2.Visible = false;

		if (LinkedPlatform != null)
		{
			LinkedPlatform.MovementStarted += OnPlatformMovementStarted;
			LinkedPlatform.MovementFinished += OnPlatformMovementFinished;
		}
	}

	public override void _Process(double delta)
	{
		if (_cooldownTimer > 0.0f)
		{
			_cooldownTimer -= (float)delta;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Interact") && _playerInRange && !_hasCog)
		{
			TryAttachCog();
		}
	}

	private void TryAttachCog()
	{
		Player player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null || player.HeldItem == null || player.HeldItem.ItemId != "Cog")
		{
			GD.Print(Name, " needs a cog first.");
			return;
		}

		CarriableItem cog = player.HeldItem;
		player.HeldItem = null;
		_bigGear2.Visible = true;

		cog.AttachTo(_attachPoint);

		_hasCog = true;
		GD.Print(Name, " cog attached - gearbox now functional!");
	}

	public void Activate()
	{
		if (!_hasCog)
		{
			GD.Print(Name, " has no cog, can't activate.");
			return;
		}

		if (_cooldownTimer > 0.0f)
		{
			GD.Print(Name, " on cooldown, ignoring hit.");
			return;
		}

		if (LinkedPlatform == null)
		{
			GD.PrintErr(Name, " has no LinkedPlatform assigned!");
			return;
		}

		_cooldownTimer = ActivationCooldown;
		LinkedPlatform.Toggle();
		GD.Print(Name, " activated!");
	}

	private void OnPlatformMovementStarted(bool movingToEnd)
	{
		_smokeParticles.Emitting = true;
		_sparkParticles.Emitting = true;
		_sparkLight?.StartFlickering();

		if (movingToEnd)
		{
			_animPlayer.Play("CogWheels");
		}
		else
		{
			_animPlayer.PlayBackwards("CogWheels");
		}
	}

	private void OnPlatformMovementFinished()
	{
		_smokeParticles.Emitting = false;
		_sparkParticles.Emitting = false;
		_sparkLight?.StopFlickering();
	}

	public void OnInteractAreaBodyEntered(Node3D body)
	{
		if (body.IsInGroup("player")) _playerInRange = true;
		GD.Print("Inside Gearbox");
	}

	public void OnInteractAreaBodyExited(Node3D body)
	{
		if (body.IsInGroup("player")) _playerInRange = false;
	}
}

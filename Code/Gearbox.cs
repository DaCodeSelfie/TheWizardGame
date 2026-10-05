using Godot;
using System;

public partial class Gearbox : StaticBody3D
{
	[Export] public MovingPlatform LinkedPlatform;
	[Export] public float ActivationCooldown = 0.5f;

	private float _cooldownTimer = 0.0f;
	private AnimationPlayer _animPlayer;
	private GpuParticles3D _smokeParticles;
	private GpuParticles3D _sparkParticles;
	private SparkLight _sparkLight;

	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer"); // adjust path if nested differently
		_smokeParticles = GetNode<GpuParticles3D>("SmokeParticels");
		_sparkParticles = GetNode<GpuParticles3D>("SparkParticels");
		_sparkLight = GetNode<SparkLight>("SparkLight"); // adjust path if nested differently

		_smokeParticles.Emitting = false;
		_sparkParticles.Emitting = false;

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

	public void Activate()
	{
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
		GD.Print(Name, " sees platform moving. MovingToEnd=", movingToEnd);

		_smokeParticles.Emitting = true;
		_sparkParticles.Emitting = true;
		_sparkLight.StartFlickering();

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
		GD.Print(Name, " sees platform stopped.");

		_smokeParticles.Emitting = false;
		_sparkParticles.Emitting = false;
		_sparkLight.StopFlickering();
	}
}

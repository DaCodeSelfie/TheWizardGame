using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 7.5f;
	public const float SprintMultiplier = 1.7f;
	public const float JumpVelocity = 4.5f;
	public const float MouseSensitivity = 0.0015f;
	public static readonly float PitchLimit = Mathf.DegToRad(89.0f);

	[Export] public float ProjectileSpawnOffset = 0.5f;
	[Export] public float WaterSpeedMultiplier = 0.5f;
	[Export] public float ClimbSpeed = 3.0f;

	[Export] public Godot.Collections.Array<Spell> Spells = new();

	[Export] public float GroundAcceleration = 12.0f;
	[Export] public float AirAcceleration = 4.0f;
	[Export] public float GroundDeceleration = 14.0f;
	[Export] public float AirDeceleration = 1.0f;
	[Export] public float FallGravityMultiplier = 1.6f;
	[Export] public float LowJumpGravityMultiplier = 2.2f;

	public bool IsInWater = false;
	public CarriableItem HeldItem;
	public Ladder CurrentLadder;
	public StaticLadder CurrentStaticLadder;
	public bool HasGasoline = false;
	public bool HasCog = false;
	public bool HasTNT = false;

	private int _currentSpellIndex = 0;
	private float _cooldownTimer = 0.0f;
	private bool _isFiring = false;

	private Node3D _head;
	private Camera3D _camera;
	private float _pitch = 0.0f;

	public override void _Ready()
	{
		GD.Print("PLAYER START ROTATION: ", Rotation);
		GD.Print("PLAYER START GLOBAL ROTATION: ", GlobalRotation);

		_head = GetNode<Node3D>("Head");
		_camera = GetNode<Camera3D>("Head/Camera3D");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetNode<Health>("Health").Died += () => GD.Print("Player has died!");

		GD.Print("Player ready. Spells loaded: ", Spells.Count);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);
			_pitch -= mouseMotion.Relative.Y * MouseSensitivity;
			_pitch = Mathf.Clamp(_pitch, -PitchLimit, PitchLimit);
			Vector3 headRotation = _head.Rotation;
			headRotation.X = _pitch;
			_head.Rotation = headRotation;
		}

		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.Escape)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			else if (keyEvent.Keycode == Key.Key1 && Spells.Count > 0)
			{
				_currentSpellIndex = 0;
				GD.Print("Switched to spell: ", Spells[0].Type);
			}
			else if (keyEvent.Keycode == Key.Key2 && Spells.Count > 1)
			{
				_currentSpellIndex = 1;
				GD.Print("Switched to spell: ", Spells[1].Type);
			}
			else if (keyEvent.Keycode == Key.Key3 && Spells.Count > 2)
			{
				_currentSpellIndex = 2;
				GD.Print("Switched to spell: ", Spells[2].Type);
			}
		}

		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.Pressed && Input.MouseMode == Input.MouseModeEnum.Visible)
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
				return;
			}

			if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
			{
				_isFiring = true;
				if (Spells.Count > 0 && !Spells[_currentSpellIndex].IsContinuous)
				{
					ShootOnce();
				}
			}
			else if (mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed)
			{
				_isFiring = false;
			}
		}
	}

	private void ShootOnce()
	{
		GD.Print("=== SHOOOOOOOT ===");
		if (Spells.Count == 0) return;
		if (_cooldownTimer > 0.0f) return;

		Spell current = Spells[_currentSpellIndex];
		Vector3 forward = -_camera.GlobalTransform.Basis.Z;
		Vector3 spawnPos = _camera.GlobalPosition + forward * ProjectileSpawnOffset;

		current.Cast(this, spawnPos, forward);
		_cooldownTimer = current.Cooldown;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (_cooldownTimer > 0.0f)
		{
			_cooldownTimer -= dt;
		}

		if (CurrentLadder != null && CurrentLadder.IsClimbable)
		{
			HandleClimbing(dt);
			return;
		}
		if (CurrentStaticLadder != null && CurrentStaticLadder.IsClimbable)
		{
			HandleClimbing(dt);
			return;
		}

		if (_isFiring && Spells.Count > 0 && Spells[_currentSpellIndex].IsContinuous)
		{
			Spell current = Spells[_currentSpellIndex];
			Vector3 forward = -_camera.GlobalTransform.Basis.Z;
			Vector3 origin = _camera.GlobalPosition;
			current.Cast(this, origin, forward);
		}

		Vector3 velocity = Velocity;
		bool onFloor = IsOnFloor();

		if (!onFloor)
		{
			float gravityMultiplier = 1.0f;
			if (velocity.Y < 0)
			{
				gravityMultiplier = FallGravityMultiplier;
			}
			else if (velocity.Y > 0 && !Input.IsActionPressed("ui_accept"))
			{
				gravityMultiplier = LowJumpGravityMultiplier;
			}

			velocity += GetGravity() * gravityMultiplier * dt;
		}

		if (Input.IsActionJustPressed("ui_accept") && onFloor)
		{
			velocity.Y = JumpVelocity;
		}

		bool isSprinting = onFloor && Input.IsActionPressed("Run");
		float targetSpeed = isSprinting ? Speed * SprintMultiplier : Speed;
		if (IsInWater)
		{
			targetSpeed *= WaterSpeedMultiplier;
		}

		Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		Vector3 targetVelocity = direction * targetSpeed;

		float accel;
		if (direction != Vector3.Zero)
		{
			accel = onFloor ? GroundAcceleration : AirAcceleration;
		}
		else
		{
			accel = onFloor ? GroundDeceleration : AirDeceleration;
			targetVelocity = Vector3.Zero;
		}

		velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, accel * dt * Speed);
		velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, accel * dt * Speed);

		Velocity = velocity;
		MoveAndSlide();
	}

	private void HandleClimbing(float dt)
	{
		Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Backward");

		Vector3 velocity = Velocity;
		velocity.Y = -inputDir.Y * ClimbSpeed;
		velocity.X = inputDir.X * (ClimbSpeed * 0.5f);
		velocity.Z = 0;

		if (Input.IsActionJustPressed("ui_accept"))
		{
			CurrentStaticLadder = null;
			CurrentLadder = null;
			velocity.Y = JumpVelocity;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}

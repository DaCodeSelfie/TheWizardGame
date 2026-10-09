using Godot;
using System;

public partial class Rat : CharacterBody3D
{
	private enum State { Roaming, Chasing, Attacking }

	[Export] public float MoveSpeed = 2.0f;
	[Export] public float ChaseSpeedMultiplier = 1.6f;
	[Export] public float DetectionRange = 5.0f;
	[Export] public float AttackRange = 1.2f;
	[Export] public float AttackCooldown = 0.5f;
	[Export] public float SwipeDamage = 10.0f;
	[Export] public float RoamWaitTime = 2.0f;
	[Export] public float LoseSightGraceTime = 3.0f;

	[Export] public GpuParticles3D SteamParticles; // optional - plays when fire steams off wetness

	private Health _health;
	private Wetness _wetness;
	private RoamArea _roamArea;
	private Player _player;

	private State _state = State.Roaming;
	private Vector3 _roamTarget;
	private float _roamWaitTimer = 0.0f;
	private float _attackCooldownTimer = 0.0f;
	private float _loseSightTimer = 0.0f;

	public override void _Ready()
	{
		_health = GetNode<Health>("Health");
		_wetness = GetNode<Wetness>("Wetness");
		_health.Died += OnDied;

		_player = GetTree().GetFirstNodeInGroup("player") as Player;

		FindRoamArea();
		PickNewRoamTarget();
	}

	private void FindRoamArea()
	{
		foreach (Node node in GetTree().GetNodesInGroup("roam_areas"))
		{
			if (node is RoamArea area && area.Contains(GlobalPosition))
			{
				_roamArea = area;
				break;
			}
		}

		if (_roamArea == null)
		{
			GD.PrintErr(Name, " could not find a RoamArea containing its spawn position!");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_health.IsDead) return;

		float dt = (float)delta;
		if (_attackCooldownTimer > 0f) _attackCooldownTimer -= dt;

		switch (_state)
		{
			case State.Roaming: ProcessRoaming(dt); break;
			case State.Chasing: ProcessChasing(dt); break;
			case State.Attacking: ProcessAttacking(dt); break;
		}

		if (!IsOnFloor())
		{
			Velocity += GetGravity() * dt;
		}

		MoveAndSlide();
	}

	private void ProcessRoaming(float dt)
	{
		if (CanSeePlayer())
		{
			GD.Print(Name, " spotted the player!");
			_state = State.Chasing;
			_loseSightTimer = 0f;
			return;
		}

		Vector3 toTarget = _roamTarget - GlobalPosition;
		toTarget.Y = 0;

		if (toTarget.Length() < 0.3f)
		{
			Velocity = new Vector3(0, Velocity.Y, 0);
			_roamWaitTimer -= dt;
			if (_roamWaitTimer <= 0f) PickNewRoamTarget();
			return;
		}

		Vector3 direction = toTarget.Normalized();
		Velocity = new Vector3(direction.X * MoveSpeed, Velocity.Y, direction.Z * MoveSpeed);
		LookTowards(direction);
	}

	private void ProcessChasing(float dt)
	{
		if (_player == null) { _state = State.Roaming; return; }

		float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);

		if (distance <= AttackRange)
		{
			_state = State.Attacking;
			Velocity = new Vector3(0, Velocity.Y, 0);
			return;
		}

		if (CanSeePlayer())
		{
			_loseSightTimer = 0f;
		}
		else
		{
			_loseSightTimer += dt;
			if (_loseSightTimer >= LoseSightGraceTime)
			{
				GD.Print(Name, " lost the player, returning to roam.");
				_state = State.Roaming;
				PickNewRoamTarget();
				return;
			}
		}

		Vector3 toPlayer = _player.GlobalPosition - GlobalPosition;
		toPlayer.Y = 0;
		Vector3 direction = toPlayer.Normalized();

		float speed = MoveSpeed * ChaseSpeedMultiplier;
		Velocity = new Vector3(direction.X * speed, Velocity.Y, direction.Z * speed);
		LookTowards(direction);
	}

	private void ProcessAttacking(float dt)
	{
		if (_player == null) { _state = State.Roaming; return; }

		float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
		if (distance > AttackRange) { _state = State.Chasing; return; }

		Vector3 toPlayer = _player.GlobalPosition - GlobalPosition;
		toPlayer.Y = 0;
		LookTowards(toPlayer.Normalized());
		Velocity = new Vector3(0, Velocity.Y, 0);

		if (_attackCooldownTimer <= 0f)
		{
			Swipe();
			_attackCooldownTimer = AttackCooldown;
		}
	}

	private void Swipe()
	{
		GD.Print(Name, " swipes at the player for ", SwipeDamage, " damage!");
		_player.GetNode<Health>("Health").TakeDamage(SwipeDamage);
	}

	private bool CanSeePlayer()
	{
		if (_player == null) return false;

		float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
		if (distance > DetectionRange) return false;

		Vector3 eyePos = GlobalPosition + Vector3.Up * 0.3f;
		Vector3 playerPos = _player.GlobalPosition + Vector3.Up * 1.0f;

		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(eyePos, playerPos);
		query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

		var result = spaceState.IntersectRay(query);
		if (result.Count == 0) return false;

		Node3D hitBody = result["collider"].As<Node3D>();
		return hitBody.IsInGroup("player");
	}

	private void PickNewRoamTarget()
	{
		_roamTarget = _roamArea != null ? _roamArea.GetRandomPoint() : GlobalPosition;
		_roamWaitTimer = RoamWaitTime;
	}

	private void LookTowards(Vector3 direction)
	{
		if (direction.LengthSquared() < 0.001f) return;
		Vector3 lookTarget = GlobalPosition + direction;
		LookAt(new Vector3(lookTarget.X, GlobalPosition.Y, lookTarget.Z), Vector3.Up);
	}

	public void ApplyFireDamage(float baseDamage)
	{
		float multiplier = 1.0f - _wetness.CurrentWetness;
		_health.TakeDamage(baseDamage * multiplier);

		if (_wetness.CurrentWetness > 0f)
		{
			_wetness.ReduceFromFire(0.4f);
			SteamParticles?.Restart();
		}
	}

	public void ApplyLightningDamage(float baseDamage, float wetBonusMultiplier)
	{
		float multiplier = 1.0f + (_wetness.CurrentWetness * wetBonusMultiplier);
		_health.TakeDamage(baseDamage * multiplier);
	}

	public void ApplyWaterSpray(float delta)
	{
		_wetness.AddFromSpray(delta);
	}

	public void ApplyFullWetness()
	{
		_wetness.SetFullyWet();
	}

	private void OnDied()
	{
		QueueFree();
	}
}

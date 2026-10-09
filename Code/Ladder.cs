using Godot;
using System;

public partial class Ladder : Node3D
{
	[Export] public float BurnDuration = 5.0f;
	[Export] public float RequiredWaterToExtinguish = 3.0f;

	private AnimationPlayer _animPlayer;
	private Area3D _climbZone;
	private GpuParticles3D _smokeParticles;

	private bool _isDeployed = false;
	private bool _isBurning = false;
	private bool _isDestroyed = false;
	private bool _hasInitialized = false;
	private float _burnTimer = 0.0f;
	private float _waterReceived = 0.0f;

	public bool IsClimbable => _isDeployed && !_isDestroyed;

	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("LadderAnimation");
		_climbZone = GetNode<Area3D>("ClimbZone");
		GD.Print("Ladder scene path: ", SceneFilePath);
		_smokeParticles = GetNode<GpuParticles3D>("SmokeParticles");
		GD.Print("Current animation: ", _animPlayer.CurrentAnimation);
		GD.Print("Is playing: ", _animPlayer.IsPlaying());

		_climbZone.Monitoring = false;
		_smokeParticles.Emitting = false;
		_animPlayer.Stop(true);
		_animPlayer.Seek(0.0, true);

		_animPlayer.AnimationFinished += OnAnimationFinished;

		//GD.Print("Available animations: ", string.Join(", ", _animPlayer.GetAnimationList()));
	}

	public override void _PhysicsProcess(double delta)
	{
		// Backup fix: force the pose to frame 0 one extra time on the very first
		// physics frame, in case something (like Autoplay) overrides the _Ready() Seek.
		if (!_hasInitialized)
		{
				_animPlayer.Seek(0.0, true);
			_hasInitialized = true;
			//GD.Print("Forced ladder pose to frame 0 on first physics frame.");
		}

		if (!_isBurning) return;

		_burnTimer += (float)delta;

		if (_waterReceived >= RequiredWaterToExtinguish)
		{
			ExtinguishFire();
			return;
		}

		if (_burnTimer >= BurnDuration)
		{
			DestroyLadder();
		}
	}

	public void Deploy()
	{
		 //GD.Print("Deploy() called!");
		if (_isDeployed) return;
		_isDeployed = true;
		_animPlayer.Play("LadderAnim");
		_smokeParticles.Emitting = true;
		_climbZone.Monitoring = true;
		
		//GD.Print(Name, " deployed - now climbable.");
	}

	public void IgniteFromHazard()
	{
		if (_isBurning || _isDestroyed || !_isDeployed) return;
		_isBurning = true;
		_burnTimer = 0.0f;
		_waterReceived = 0.0f;
		GD.Print(Name, " catches fire! Spray water within ", BurnDuration, "s to save it.");
	}

	public void ReceiveWater(float delta)
	{
		if (!_isBurning) return;
		_waterReceived += delta;
		GD.Print(Name, " being doused: ", _waterReceived.ToString("F1"), "/", RequiredWaterToExtinguish);
	}

	private void ExtinguishFire()
	{
		_isBurning = false;
		GD.Print(Name, " fire extinguished, ladder saved.");
	}

	private void DestroyLadder()
	{
		_isBurning = false;
		_isDestroyed = true;
		_climbZone.Monitoring = false;
		GD.Print(Name, " burned down completely - no longer usable.");
	}

	public void OnClimbZoneBodyEntered(Node3D body)
	{
		//GD.Print(Name, " ClimbZone detected: ", body.Name, " IsClimbable=", IsClimbable);

		if (body is Player player && IsClimbable)
		{
			player.CurrentLadder = this;
			//GD.Print("Player entered ladder climb zone.");
		}
	}

	public void OnClimbZoneBodyExited(Node3D body)
	{
		if (body is Player player && player.CurrentLadder == this)
		{
			player.CurrentLadder = null;
			//GD.Print("Player left ladder climb zone.");
		}
	}

	private void OnAnimationFinished(StringName animName)
	{
		if (animName == "LadderAnim")
		{
			_smokeParticles.Emitting = false;
		}
	}
}

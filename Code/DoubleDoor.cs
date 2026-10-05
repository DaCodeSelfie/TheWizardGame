using Godot;
using System;

public partial class DoubleDoor : Node3D
{
	[Export] public float ShakeStrength = 0.03f;
	[Export] public float ShakeDuration = 0.3f;

	private bool _playerInRange = false;
	private bool _isOpen = false;
	private bool _isAnimating = false;
	private bool _isPlayingAudio = false;

	private AnimationPlayer _animPlayer;
	private GpuParticles3D _dustParticles;
	private AudioStreamPlayer3D _audioPlayer;

	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_animPlayer.AnimationFinished += OnAnimationFinished;
		
		_audioPlayer = GetNode<AudioStreamPlayer3D>("AudioPlayer");
		_audioPlayer.Finished += OnAudioFinished;

		_dustParticles = GetNode<GpuParticles3D>("DustParticles"); // adjust path to match your setup
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Interact") && _playerInRange && !_isAnimating && !_isPlayingAudio)
		{
			ToggleDoor();
		}
	}

	private void ToggleDoor()
	{
		_isOpen = !_isOpen;
		_isAnimating = true;
		_isPlayingAudio = true;

		if (_isOpen)
		{
			_animPlayer.Play("DoorOpening");
		}
		else
		{
			_animPlayer.PlayBackwards("DoorOpening");
		}

		_audioPlayer.Play();
		ShakeOnImpact();

		GD.Print(Name, " toggled. IsOpen=", _isOpen);
	}
	
	private void OnAudioFinished()
	{
		_isPlayingAudio = false;
	}

	private void OnAnimationFinished(StringName animName)
	{
		_isAnimating = false;
		GD.Print(Name, " animation finished, triggering dust burst.");
		ShakeOnImpact();
		_dustParticles.Restart();
	}

	private void ShakeOnImpact()
	{
		Tween shakeTween = CreateTween();
		Vector3 originalPos = Position;

		for (int i = 0; i < 5; i++)
		{
			Vector3 offset = new Vector3(
				(float)GD.RandRange(-ShakeStrength, ShakeStrength),
				(float)GD.RandRange(-ShakeStrength, ShakeStrength),
				(float)GD.RandRange(-ShakeStrength, ShakeStrength)
			);
			shakeTween.TweenProperty(this, "position", originalPos + offset, ShakeDuration / 5.0f);
		}
		shakeTween.TweenProperty(this, "position", originalPos, ShakeDuration / 5.0f);
	}

	public void OnInteractAreaBodyEntered(Node3D body)
	{
		GD.Print("Something entered the area: ", body.Name);
		if (body.IsInGroup("player"))
		{
			GD.Print("It's the player! Player is now in range.");
			_playerInRange = true;
		}
	}

	public void OnInteractAreaBodyExited(Node3D body)
	{
		GD.Print("Something exited the area: ", body.Name);
		if (body.IsInGroup("player"))
		{
			GD.Print("Player left range.");
			_playerInRange = false;
		}
	}
}

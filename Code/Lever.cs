using Godot;
using System;

public partial class Lever : Node3D
{
	[Signal] public delegate void ToggledEventHandler(Lever lever, bool isPulled);

	public bool IsPulled = false;

	private bool _playerInRange = false;
	private bool _isAnimating = false;
	private AnimationPlayer _animPlayer;

	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("HandelPivot/Handle/AnimationPlayer"); // adjust path if it's nested differently
		_animPlayer.AnimationFinished += OnAnimationFinished;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Interact") && _playerInRange && !_isAnimating)
		{
			Pull();
		}
	}

	private void Pull()
	{
		IsPulled = !IsPulled;
		_isAnimating = true;

		if (IsPulled)
		{
			_animPlayer.Play("PullLever");
		}
		else
		{
			_animPlayer.PlayBackwards("PullLever");
		}

		EmitSignal(SignalName.Toggled, this, IsPulled);
		GD.Print("Lever ", Name, " toggled. IsPulled=", IsPulled);
	}

	private void OnAnimationFinished(StringName animName)
	{
		_isAnimating = false;
	}

	public void OnInteractAreaBodyEntered(Node3D body)
	{
		if (body.IsInGroup("player"))
		{
			_playerInRange = true;
		}
	}

	public void OnInteractAreaBodyExited(Node3D body)
	{
		if (body.IsInGroup("player"))
		{
			_playerInRange = false;
		}
	}
}

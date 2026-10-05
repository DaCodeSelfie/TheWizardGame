using Godot;
using System;

public partial class MovingPlatform : AnimatableBody3D
{
	[Signal] public delegate void MovementStartedEventHandler(bool movingToEnd);
	[Signal] public delegate void MovementFinishedEventHandler();

	[Export] public float MoveDuration = 2.0f;
	[Export] public bool StartAtEndPoint = false;

	private Vector3 _startPos;
	private Vector3 _endPos;
	private bool _isAtEnd;
	private bool _isMoving = false;

	public override void _Ready()
	{
		Marker3D startPoint = GetNode<Marker3D>("StartPoint");
		Marker3D endPoint = GetNode<Marker3D>("EndPoint");

		_startPos = startPoint.GlobalPosition;
		_endPos = endPoint.GlobalPosition;

		_isAtEnd = StartAtEndPoint;
		GlobalPosition = _isAtEnd ? _endPos : _startPos;

		GD.Print(Name, " ready. Starting at ", (_isAtEnd ? "EndPoint" : "StartPoint"));
	}

	public void Toggle()
	{
		if (_isMoving) return;

		_isMoving = true;
		Vector3 target = _isAtEnd ? _startPos : _endPos;
		bool movingToEnd = !_isAtEnd; // true if we're heading toward EndPoint
		_isAtEnd = !_isAtEnd;

		EmitSignal(SignalName.MovementStarted, movingToEnd);

		Tween tween = CreateTween();
		tween.TweenProperty(this, "global_position", target, MoveDuration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);

		tween.Finished += () =>
		{
			_isMoving = false;
			EmitSignal(SignalName.MovementFinished);
		};

		GD.Print(Name, " moving to ", (_isAtEnd ? "EndPoint" : "StartPoint"));
	}
}

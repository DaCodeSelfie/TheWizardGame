using Godot;
using System;

public partial class BossGate : StaticBody3D
{
	[Export] public float SlideDistance = 3.0f;
	[Export] public float SlideDuration = 1.2f;
	[Export] public Vector3 SlideDirection = Vector3.Up;

	private Vector3 _closedPos;
	private bool _isOpen = false;
	private bool _isSealed = false; // once sealed shut, ApproachTrigger can no longer reopen it

	public override void _Ready()
	{
		_closedPos = Position;
	}

	public void Open()
	{
		if (_isOpen || _isSealed) return;
		_isOpen = true;

		Tween tween = CreateTween();
		tween.TweenProperty(this, "position", _closedPos + SlideDirection * SlideDistance, SlideDuration)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);

		GD.Print(Name, " opening.");
	}

	public void SealShut()
	{
		if (_isSealed) return;
		_isSealed = true;
		_isOpen = false;

		Tween tween = CreateTween();
		tween.TweenProperty(this, "position", _closedPos, SlideDuration)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);

		GD.Print(Name, " sealed shut behind the player!");
	}
}

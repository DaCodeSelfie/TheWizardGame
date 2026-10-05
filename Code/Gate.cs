using Godot;
using System;

public partial class Gate : Node3D
{
	[Export] public Node3D GatePanel;
	[Export] public float SlideDistance = 3.0f;
	[Export] public float SlideDuration = 1.0f;
	[Export] public Vector3 SlideDirection = Vector3.Up;
	[Export] public bool StartOpen = false;

	private Vector3 _closedPos;
	private bool _isOpen;

	public override void _Ready()
	{
		_closedPos = GatePanel.Position;
		_isOpen = StartOpen;

		if (_isOpen)
		{
			GatePanel.Position = _closedPos + SlideDirection * SlideDistance;
		}
	}

	public void Open()
	{
		if (_isOpen) return;
		_isOpen = true;
		AnimateTo(_closedPos + SlideDirection * SlideDistance);
		GD.Print(Name, " opening.");
	}

	public void Close()
	{
		if (!_isOpen) return;
		_isOpen = false;
		AnimateTo(_closedPos);
		GD.Print(Name, " closing.");
	}

	private void AnimateTo(Vector3 target)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(GatePanel, "position", target, SlideDuration)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
	}
}

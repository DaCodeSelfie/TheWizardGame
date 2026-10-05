using Godot;
using System;

public partial class Torch : Node3D
{
	[Export] public float BaseEnergy = 2.0f;
	[Export] public float FlickerAmount = 0.3f;
	[Export] public float FlickerSpeed = 8.0f;

	[Export] public float ExtinguishDuration = 0.3f;
	[Export] public float IgniteDuration = 1.5f;

	public bool IsLit = true;

	private OmniLight3D _flameLight;
	private GpuParticles3D _smokeParticles;
	private float _noiseOffset;
	private float _currentTargetEnergy;
	private Tween _fadeTween;

	public override void _Ready()
	{
		_flameLight = GetNode<OmniLight3D>("FlameLight");
		_smokeParticles = GetNode<GpuParticles3D>("SmokeParticles");
		_noiseOffset = (float)GD.RandRange(0.0, 100.0);

		_currentTargetEnergy = IsLit ? BaseEnergy : 0.0f;
		_flameLight.LightEnergy = _currentTargetEnergy;
		_smokeParticles.Emitting = IsLit;
	}

	public override void _Process(double delta)
	{
		if (!IsLit)
			return;

		float flicker = (float)Mathf.Sin((Time.GetTicksMsec() / 1000.0f + _noiseOffset) * FlickerSpeed);
		_flameLight.LightEnergy = _currentTargetEnergy + flicker * FlickerAmount;
	}

	public void Extinguish()
	{
		if (!IsLit) return;
		IsLit = false;
		_smokeParticles.Emitting = false;

		_fadeTween?.Kill();
		_fadeTween = CreateTween();
		_fadeTween.TweenMethod(
			Callable.From((float value) => { _currentTargetEnergy = value; _flameLight.LightEnergy = value; }),
			_currentTargetEnergy,
			0.0f,
			ExtinguishDuration
		).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);

		GD.Print(Name, " extinguishing.");
	}

	public void Ignite()
	{
		if (IsLit) return;
		IsLit = true;
		_smokeParticles.Emitting = true;

		_fadeTween?.Kill();
		_fadeTween = CreateTween();
		_fadeTween.TweenMethod(
			Callable.From((float value) => { _currentTargetEnergy = value; }),
			_currentTargetEnergy,
			BaseEnergy,
			IgniteDuration
		).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

		GD.Print(Name, " igniting.");
	}
}

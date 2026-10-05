using Godot;
using System;

public partial class SparkLight : Node
{
	[Export] public NodePath LightPath = "SparkLightNode";
	[Export] public float BaseEnergy = 2.0f;
	[Export] public float FlickerAmount = 1.5f;
	[Export] public float FlickerSpeed = 20.0f;

	private OmniLight3D _light;
	private float _noiseOffset;
	private bool _isFlickering = false;

	public override void _Ready()
	{
		_light = GetNode<OmniLight3D>(LightPath);
		_noiseOffset = (float)GD.RandRange(0.0, 100.0);
		_light.LightEnergy = 0.0f;
	}

	public override void _Process(double delta)
	{
		if (!_isFlickering)
			return;

		float flicker = (float)Mathf.Sin((Time.GetTicksMsec() / 1000.0f + _noiseOffset) * FlickerSpeed);
		_light.LightEnergy = Mathf.Max(0.0f, BaseEnergy + flicker * FlickerAmount);
	}

	public void StartFlickering()
	{
		_isFlickering = true;
	}

	public void StopFlickering()
	{
		_isFlickering = false;
		_light.LightEnergy = 0.0f;
	}
}

using Godot;
using System;

public partial class RoamArea : Area3D
{
	private BoxShape3D _boxShape;

	public override void _Ready()
	{
		AddToGroup("roam_areas");

		CollisionShape3D collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		_boxShape = collisionShape.Shape as BoxShape3D;

		if (_boxShape == null)
		{
			GD.PrintErr(Name, " RoamArea requires a BoxShape3D!");
		}
	}

	public bool Contains(Vector3 worldPosition)
	{
		if (_boxShape == null) return false;

		Vector3 localPos = GlobalTransform.AffineInverse() * worldPosition;
		Vector3 half = _boxShape.Size / 2.0f;

		return Mathf.Abs(localPos.X) <= half.X
			&& Mathf.Abs(localPos.Y) <= half.Y
			&& Mathf.Abs(localPos.Z) <= half.Z;
	}

	public Vector3 GetRandomPoint()
	{
		if (_boxShape == null) return GlobalPosition;

		Vector3 half = _boxShape.Size / 2.0f;
		Vector3 localPoint = new Vector3(
			(float)GD.RandRange(-half.X, half.X),
			0,
			(float)GD.RandRange(-half.Z, half.Z)
		);

		return GlobalTransform * localPoint;
	}
}

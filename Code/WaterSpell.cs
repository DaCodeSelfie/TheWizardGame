using Godot;

public partial class WaterSpell : Spell
{
	[Export] public float SprayRange = 8.0f;

	public override SpellType Type => SpellType.Water;
	public override bool IsContinuous => true;

	public override void Cast(Player caster, Vector3 origin, Vector3 direction)
	{
		var spaceState = caster.GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(origin, origin + direction * SprayRange);
		query.CollideWithBodies = true;
		query.Exclude = new Godot.Collections.Array<Rid> { caster.GetRid() };

		var result = spaceState.IntersectRay(query);
		if (result.Count == 0) return;

		Node3D hitBody = result["collider"].As<Node3D>();
		float delta = (float)caster.GetPhysicsProcessDeltaTime();

		if (hitBody is Torch torchDirect) torchDirect.Extinguish();
		else if (hitBody.GetParent() is Torch torchParent) torchParent.Extinguish();

		if (hitBody is WaterTank tank) tank.FillWithWater(delta);

		if (hitBody is Rat rat) rat.ApplyWaterSpray(delta);

		GD.Print("Water spray hit: ", hitBody.Name);
	}
}

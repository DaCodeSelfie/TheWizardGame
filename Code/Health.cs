using Godot;
using System;

public partial class Health : Node
{
	[Signal] public delegate void DiedEventHandler();
	[Signal] public delegate void DamagedEventHandler(float amount, float currentHealth);

	[Export] public float MaxHealth = 100.0f;

	public float CurrentHealth { get; private set; }
	public bool IsDead { get; private set; } = false;

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
	}

	public void TakeDamage(float amount)
	{
		if (IsDead || amount <= 0f) return;

		CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
		GD.Print(GetParent().Name, " took ", amount.ToString("F1"), " damage. HP: ", CurrentHealth.ToString("F1"), "/", MaxHealth);

		EmitSignal(SignalName.Damaged, amount, CurrentHealth);

		if (CurrentHealth <= 0f)
		{
			IsDead = true;
			GD.Print(GetParent().Name, " died.");
			EmitSignal(SignalName.Died);
		}
	}

	public void Heal(float amount)
	{
		if (IsDead || amount <= 0f) return;
		CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
		GD.Print(GetParent().Name, " healed ", amount, ". HP: ", CurrentHealth, "/", MaxHealth);
	}
}

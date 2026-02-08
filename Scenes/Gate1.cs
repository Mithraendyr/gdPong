using Godot;
using System;

public partial class Gate1 : Area3D
{
	[Export]
	public string Side = "Left";

	[Signal]
	public delegate void GoalScoredEventHandler(string side);

	public override void _Ready()
	{
		// Ensure monitoring is enabled
		Monitoring = true;
		Monitorable = true;
		BodyEntered += OnBodyEntered;
		GD.Print($"Gate1 ({Side}) ready and monitoring");
	}

	private void OnBodyEntered(Node3D body)
	{
		GD.Print($"Gate1: Body entered - {body.Name} (Type: {body.GetType().Name})");
		if (body is Ball)
		{
			GD.Print($"Gate1 ({Side}): Ball detected! Emitting goal signal");
			EmitSignal(SignalName.GoalScored, Side);
		}
	}
}

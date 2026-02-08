using Godot;
using System;

public partial class Gate2 : Area3D
{
	[Export]
	public string Side = "Right";

	[Signal]
	public delegate void GoalScoredEventHandler(string side);

	public override void _Ready()
	{
		// Ensure monitoring is enabled
		Monitoring = true;
		Monitorable = true;
		BodyEntered += OnBodyEntered;
		GD.Print($"Gate2 ({Side}) ready and monitoring");
	}

	private void OnBodyEntered(Node3D body)
	{
		GD.Print($"Gate2: Body entered - {body.Name} (Type: {body.GetType().Name})");
		if (body is Ball)
		{
			GD.Print($"Gate2 ({Side}): Ball detected! Emitting goal signal");
			EmitSignal(SignalName.GoalScored, Side);
		}
	}
}

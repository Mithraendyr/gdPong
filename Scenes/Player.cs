using Godot;
using System;

public partial class Player : RigidBody3D
{
	public float Speed = 20.0f;

	public override void _PhysicsProcess(double delta)
	{
		var velocity = Vector3.Zero;

		if (Input.IsActionPressed("ui_up"))
			velocity.Y += Speed;
		if (Input.IsActionPressed("ui_down"))
			velocity.Y -= Speed;
		if (Input.IsActionPressed("ui_left"))
			velocity.X -= Speed;
		if (Input.IsActionPressed("ui_right"))
			velocity.X += Speed;

		LinearVelocity = velocity;
	}

	public override void _Ready()
	{
		// Enable collision detection
		ContactMonitor = true;
		MaxContactsReported = 4;
		BodyEntered += OnBodyEntered;
	}
	
	private void OnBodyEntered(Node body)
	{
		if (body is Ball ball)
		{
			GD.Print("Character: Ball collision detected, pushing ball off");
			PushBallOff(ball, 10.0f);
		}
	}
	
	public void PushBallOff(Ball ball, float speedBoost = 10.0f)
	{
		if (ball != null)
		{
			// Calculate push direction from paddle center to ball
			var pushDirection = (ball.GlobalPosition - GlobalPosition).Normalized();
			
			// Add paddle velocity influence for physics-based return angle
			var paddleInfluence = LinearVelocity.Normalized() * 0.3f;
			pushDirection = (pushDirection + paddleInfluence).Normalized();
			
			ball.Serve(pushDirection, ball.LinearVelocity.Length() + speedBoost);
			ball.AddSpeed(speedBoost);
		}
	}
}

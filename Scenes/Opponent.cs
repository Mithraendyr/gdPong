using Godot;
using System;

public partial class Opponent : RigidBody3D
{
	[Export]
	public float Speed = 20.0f;

	private Ball _ball;

	public override void _Ready()
	{
		// Enable collision detection
		ContactMonitor = true;
		MaxContactsReported = 4;
		BodyEntered += OnBodyEntered;

		// Find the ball in the scene by type, searching all nodes
		_ball = GetTree().GetNodesInGroup("ball").Count > 0 ? (Ball)GetTree().GetNodesInGroup("ball")[0] : null;
		if (_ball == null)
		{
			foreach (var node in GetTree().Root.GetChildren())
			{
				if (node is Ball ball)
				{
					_ball = ball;
					break;
				}
			}
		}
	}

	private void OnBodyEntered(Node body)
	{
		if (body is Ball ball)
		{
			GD.Print("Opponent: Ball collision detected, pushing ball off");
			PushBallOff(ball, 10.0f);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_ball == null)
			return;

		// Track ball on X and Y axes, like Character
		float ballX = _ball.GlobalPosition.X;
		float ballY = _ball.GlobalPosition.Y;
		float currentX = GlobalPosition.X;
		float currentY = GlobalPosition.Y;

		var velocity = Vector3.Zero;

		if (Mathf.Abs(ballX - currentX) > 0.1f)
			velocity.X = (ballX > currentX) ? Speed : -Speed;
		if (Mathf.Abs(ballY - currentY) > 0.1f)
			velocity.Y = (ballY > currentY) ? Speed : -Speed;

		LinearVelocity = velocity;
	}

	public void ServeBack(float speedBoost = 10.0f)
	{
		if (_ball != null)
		{
			var newDirection = -_ball.LinearVelocity.Normalized();
			_ball.Serve(newDirection);
			_ball.AddSpeed(speedBoost);
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

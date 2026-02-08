using Godot;
using System;

public partial class Ball : RigidBody3D
{
	[Export]
	public float InitialSpeed = 20f;
	
	[Export]
	public float MaxSpeed = 1000f;

	[Export]
	public float DirectionalForce = 5f; // Constant force to prevent getting stuck

	private float _targetSpeed;
	private Vector3 _lastDirection = Vector3.Back;

	public override void _Ready()
	{
		AddToGroup("ball");
		// Configure physics properties
		GravityScale = 0f; // Disable gravity for pong ball
		ContactMonitor = true; // Enable collision detection
		MaxContactsReported = 10;
		
		// Serve the ball initially
		Serve(Vector3.Back);
	}

	public override void _PhysicsProcess(double delta)
	{
		// Apply constant force in the last direction to prevent getting stuck
		ApplyCentralForce(_lastDirection * DirectionalForce);

		// Maintain speed on collisions - restore velocity magnitude if reduced
		var currentSpeed = LinearVelocity.Length();
		if (currentSpeed > 0 && currentSpeed < _targetSpeed * 0.95f)
		{
			LinearVelocity = LinearVelocity.Normalized() * _targetSpeed;
		}
	}



	/// <summary>
	/// Serve the ball in a given direction with initial speed
	/// </summary>
	public void Serve(Vector3 direction, float speed = -1f)
	{
		_lastDirection = direction.Normalized();
		_targetSpeed = speed > 0 ? speed : InitialSpeed;
		LinearVelocity = _lastDirection * _targetSpeed;
	}

	/// <summary>
	/// Add speed boost on paddle hit
	/// </summary>
	public void AddSpeed(float speedBoost)
	{
		_targetSpeed = Mathf.Min(_targetSpeed + speedBoost, MaxSpeed);
		var currentSpeed = LinearVelocity.Length();
		if (currentSpeed > 0)
			LinearVelocity = LinearVelocity.Normalized() * _targetSpeed;
	}
}

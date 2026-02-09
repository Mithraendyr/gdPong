using Godot;
using System;

public partial class Player : RigidBody3D
{
	[Export] public float Speed = 20.0f;
	[Export] public float FollowSmoothing = 10.0f;
	
	// Arena boundaries (based on arena.tscn: walls at X=±16, floor at Y=0, ceiling at Y=10)
	[Export] public float MinX = -14.0f;
	[Export] public float MaxX = 14.0f;
	[Export] public float MinY = 1.0f;
	[Export] public float MaxY = 9.0f;

	private Camera3D _camera;

	public override void _PhysicsProcess(double delta)
	{
		if (_camera == null) return;
		
		// Get mouse position in screen space
		var mousePos = GetViewport().GetMousePosition();
		
		// Project ray from camera through mouse position
		var rayOrigin = _camera.ProjectRayOrigin(mousePos);
		var rayDirection = _camera.ProjectRayNormal(mousePos);
		
		// Find intersection with player's Z plane
		var targetPos = IntersectRayWithZPlane(rayOrigin, rayDirection, GlobalPosition.Z);
		
		if (targetPos.HasValue)
		{
			// Clamp target position to arena boundaries
			var clampedTarget = new Vector3(
				Mathf.Clamp(targetPos.Value.X, MinX, MaxX),
				Mathf.Clamp(targetPos.Value.Y, MinY, MaxY),
				GlobalPosition.Z
			);
			
			// Calculate direction to target
			var toTarget = clampedTarget - GlobalPosition;
			toTarget.Z = 0; // Ensure no Z movement
			
			// Apply smoothed velocity toward target
			var distance = toTarget.Length();
			if (distance > 0.1f)
			{
				// Velocity scales with distance for smooth deceleration near target
				var targetSpeed = Mathf.Min(distance * FollowSmoothing, Speed);
				LinearVelocity = toTarget.Normalized() * targetSpeed;
			}
			else
			{
				LinearVelocity = Vector3.Zero;
			}
		}
	}
	
	private Vector3? IntersectRayWithZPlane(Vector3 rayOrigin, Vector3 rayDirection, float planeZ)
	{
		// Plane equation: z = planeZ (normal is (0, 0, 1))
		// Ray: P = rayOrigin + t * rayDirection
		// Solve for t: rayOrigin.Z + t * rayDirection.Z = planeZ
		
		if (Mathf.Abs(rayDirection.Z) < 0.0001f)
		{
			// Ray is parallel to the plane
			return null;
		}
		
		var t = (planeZ - rayOrigin.Z) / rayDirection.Z;
		
		if (t < 0)
		{
			// Intersection is behind the camera
			return null;
		}
		
		return rayOrigin + rayDirection * t;
	}

	public override void _Ready()
	{
		// Get camera reference
		_camera = GetViewport().GetCamera3D();
		
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

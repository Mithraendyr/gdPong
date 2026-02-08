using Godot;
using System;

public partial class Arena : Node3D
{
	private int _player1Score = 0;
	private int _player2Score = 0;
	private string _lastScoringSide = "";

	private Ball _ball;
	private Vector3 _ballStartPosition;
	private Label _scoreLabel;

	public override void _Ready()
	{
		// Find the ball
		_ball = GetTree().GetNodesInGroup("ball").Count > 0 ? (Ball)GetTree().GetNodesInGroup("ball")[0] : null;
		if (_ball != null)
		{
			_ballStartPosition = _ball.GlobalPosition;
			GD.Print($"Arena: Ball found at position {_ballStartPosition}");
		}
		else
		{
			GD.PrintErr("Arena: Ball not found!");
		}

		// Find the score label
		_scoreLabel = GetNodeOrNull<Label>("ScoreLabel");
		if (_scoreLabel == null)
		{
			// Try to find it in a CanvasLayer
			var canvasLayer = GetNodeOrNull<CanvasLayer>("CanvasLayer");
			if (canvasLayer != null)
			{
				_scoreLabel = canvasLayer.GetNodeOrNull<Label>("ScoreLabel");
			}
		}

		if (_scoreLabel != null)
		{
			GD.Print("Arena: Score label found");
			UpdateScoreDisplay();
		}
		else
		{
			GD.PrintErr("Arena: Score label not found - add a Label node named 'ScoreLabel'");
		}

		// Connect to gate signals - try to find them in the scene tree
		Gate1 gate1 = null;
		Gate2 gate2 = null;

		// Try direct child first
		gate1 = GetNodeOrNull<Gate1>("Gate1");
		gate2 = GetNodeOrNull<Gate2>("Gate2");

		// If not found, search the entire tree
		if (gate1 == null)
		{
			foreach (var node in GetTree().Root.GetChildren())
			{
				gate1 = FindChildRecursive<Gate1>(node);
				if (gate1 != null) break;
			}
		}

		if (gate2 == null)
		{
			foreach (var node in GetTree().Root.GetChildren())
			{
				gate2 = FindChildRecursive<Gate2>(node);
				if (gate2 != null) break;
			}
		}

		// Connect signals if found
		if (gate1 != null)
		{
			gate1.GoalScored += OnGoalScored;
			GD.Print($"Arena: Gate1 connected (node: {gate1.Name})");
		}
		else
		{
			GD.PrintErr("Arena: Gate1 not found in scene tree!");
		}

		if (gate2 != null)
		{
			gate2.GoalScored += OnGoalScored;
			GD.Print($"Arena: Gate2 connected (node: {gate2.Name})");
		}
		else
		{
			GD.PrintErr("Arena: Gate2 not found in scene tree!");
		}
	}

	public override void _Process(double delta)
	{
		// Reset score when 'R' key is pressed
		if (Input.IsActionJustPressed("ui_cancel") || Input.IsKeyPressed(Key.R))
		{
			ResetGame();
		}
	}

	private void ResetGame()
	{
		_player1Score = 0;
		_player2Score = 0;
		UpdateScoreDisplay();
		ResetBall();
		GD.Print("==========================================");
		GD.Print("🔄 Game Reset! Score: 0 - 0");
		GD.Print("==========================================");
	}

	private void OnGoalScored(string side)
	{
		GD.Print($"Arena: OnGoalScored called for side: {side}");
		_lastScoringSide = side;
		// Update score (if ball enters left gate, right player scores)
		if (side == "Left")
		{
			_player2Score++;
		}
		else
		{
			_player1Score++;
		}

		GD.Print("=".PadRight(50, '='));
		GD.Print($"⚽ GOAL! Player {(side == "Left" ? "2" : "1")} scores!");
		GD.Print($"📊 SCORE: Player 1: {_player1Score} | Player 2: {_player2Score}");
		GD.Print("=".PadRight(50, '='));

		// Update UI
		UpdateScoreDisplay();

		// Reset the ball after a short delay
		GD.Print("Arena: Creating timer for ball reset...");
		GetTree().CreateTimer(1.0).Timeout += ResetBall;
	}

	private void ResetBall()
	{
		GD.Print("Arena: ResetBall called");
		if (_ball == null)
		{
			GD.PrintErr("Arena: Cannot reset ball - ball reference is null");
			return;
		}

		// Reset ball position
		GD.Print($"Arena: Resetting ball from {_ball.GlobalPosition} to {_ballStartPosition}");
		_ball.GlobalPosition = _ballStartPosition;
		_ball.LinearVelocity = Vector3.Zero;
		_ball.AngularVelocity = Vector3.Zero;

		// Serve in a random direction towards the scoring player
		var rng = new RandomNumberGenerator();
		rng.Randomize();
		var randomX = rng.RandfRange(-0.5f, 0.5f);
		var randomY = rng.RandfRange(-0.5f, 0.5f);
		
		// Determine Z direction based on who scored
		// If Left gate (player 2 scores), serve towards player 2 (negative Z)
		// If Right gate (player 1 scores), serve towards player 1 (positive Z)
		float zDirection = (_lastScoringSide == "Right") ? 1f : -1f;
		
		var direction = new Vector3(randomX, randomY, zDirection).Normalized();
		
		GD.Print($"Arena: Serving ball towards Player {(_lastScoringSide == "Right" ? "1" : "2")} in direction {direction}");
		_ball.Serve(direction);
		GD.Print("Arena: Ball reset complete");
	}

	private void UpdateScoreDisplay()
	{
		if (_scoreLabel != null)
		{
			_scoreLabel.Text = $"Player  {_player1Score}  :  {_player2Score}  NPC";
		}
	}

	private T FindChildRecursive<T>(Node node) where T : Node
	{
		if (node is T result)
			return result;

		foreach (Node child in node.GetChildren())
		{
			var found = FindChildRecursive<T>(child);
			if (found != null)
				return found;
		}

		return null;
	}
}

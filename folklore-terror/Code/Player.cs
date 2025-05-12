using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed = 100.0f;
	[Export]
	public float JumpVelocity = -200.0f;
	[Export]
	public float Gravity = 600.0f;
	
	private AnimatedSprite2D _animations;
	private Vector2 _velocity = Vector2.Zero;
	
	public override void _Ready()
	{
		_animations = GetNode<AnimatedSprite2D>("Animations");
	}
	
	public override void _Process(double delta)
	{
		// Dodaj grawitację
		if (!IsOnFloor())
		{
			_velocity.Y += Gravity * (float)delta;
		}
		
		// Obsługa ruchu w lewo i prawo na podstawie wciśniętych klawiszy
		if (Input.IsKeyPressed(InputKeys.WALK_LEFT))
		{
			_velocity.X = -Speed;
			_animations.FlipH = true;
		}
		else if (Input.IsKeyPressed(InputKeys.WALK_RIGHT))
		{
			_velocity.X = Speed;
			_animations.FlipH = false;
		}
		else
		{
			_velocity.X = 0; 
		}

		// Obsługa skoku po wciśnięciu spacji, tylko gdy postać jest na podłodze
		if (IsOnFloor() && Input.IsKeyPressed(InputKeys.JUMP))
		{
			_velocity.Y = JumpVelocity;
		}

		// Przesuń ciało
		Velocity = _velocity;
		MoveAndSlide();
	}
}

using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed = 100.0f;
	[Export]
	public float JumpVelocity = -200.0f;
	[Export]
	public float Gravity = 600.0f;
	
	public bool LockMovement = false;
	
	public int numOfLives = 5;
	private AnimatedSprite2D[] _cells = new AnimatedSprite2D[5];
	private AnimatedSprite2D _statusIcons;
	private AnimatedSprite2D _effectAnimations;
	
	private AnimatedSprite2D _animations;
	private Sprite2D _healthBar;
	private Vector2 _velocity = Vector2.Zero;
	
	private int currentIndex = -1;
	private double currentIndexTimer = 10;

	public override void _Ready()
	{
		_animations = GetNode<AnimatedSprite2D>("Animations");
		_effectAnimations = GetNode<AnimatedSprite2D>("EffectAnimations");
		
		_healthBar = GetNode<Sprite2D>("HealthBar");
		_cells[0] = GetNode<AnimatedSprite2D>("HealthBar/HealthOrb 1");
		_cells[1] = GetNode<AnimatedSprite2D>("HealthBar/HealthOrb 2");
		_cells[2] = GetNode<AnimatedSprite2D>("HealthBar/HealthOrb 3");
		_cells[3] = GetNode<AnimatedSprite2D>("HealthBar/HealthOrb 4");
		_cells[4] = GetNode<AnimatedSprite2D>("HealthBar/HealthOrb 5");
		
		_statusIcons = GetNode<AnimatedSprite2D>("StatusIcons");

		// Zaczynamy od pierwszego elementu podniesionego
		currentIndex = 0;
		_cells[currentIndex].Position -= new Vector2(0, 1); // Podnosimy pierwszy element
		
		HideLives(true);
		ChangeStatus("none");
	}
	
	public override void _Process(double delta)
	{
		// Dodaj grawitację
		if (!IsOnFloor())
		{
			_velocity.Y += Gravity * (float)delta;
		}
		
		// Obsługa ruchu w lewo i prawo na podstawie wciśniętych klawiszy
		if (Input.IsKeyPressed(InputKeys.WALK_LEFT) && LockMovement == false)
		{
			_velocity.X = -Speed;
			_animations.FlipH = true;
			if (IsOnFloor())
				_animations.Play("run");
		}
		else if (Input.IsKeyPressed(InputKeys.WALK_RIGHT) && LockMovement == false)
		{
			_velocity.X = Speed;
			_animations.FlipH = false;
			if (IsOnFloor())
				_animations.Play("run");
		}
		else
		{
			_velocity.X = 0; 
			_animations.Play("idle");
		}

		// Obsługa skoku po wciśnięciu spacji, tylko gdy postać jest na podłodze
		if (IsOnFloor() && Input.IsKeyPressed(InputKeys.JUMP) && LockMovement == false)
		{
			_velocity.Y = JumpVelocity;
		}
		
		if (_velocity.Y < 0 && !IsOnFloor())
			_animations.Play("jump");
		if (_velocity.Y > 0 && !IsOnFloor())
			_animations.Play("fall");
			
		if (currentIndexTimer < 0) // "ui_accept" to domyślnie spacja lub Enter
		{
			// Opuszczamy poprzedni element (jeśli istnieje)
			if (currentIndex != -1)
			{
				_cells[currentIndex].Position += new Vector2(0, 1);
			}

			// Przechodzimy do następnego elementu
			currentIndex = (currentIndex + 1) % 5;

			// Podnosimy nowy, aktualny element
			_cells[currentIndex].Position -= new Vector2(0, 1);
			
			currentIndexTimer = 10;
		}
		
		currentIndexTimer-=delta*100;
		
	
		// Przesuń ciało
		Velocity = _velocity;
		MoveAndSlide();
	}
	
	public void PlayEffect(string effect)
	{
		switch(effect)
		{
			case "punch":
				_effectAnimations.Play("punch");
				break;
			case "slash":
				_effectAnimations.Play("slash");
				break;
			case "scream":
				_effectAnimations.Play("scream");
				break;
		}
	}
	
	public void HideLives(bool hideLives)
	{
		if (hideLives == false)
		{
			_healthBar.Visible = true;
			_statusIcons.Visible = true;
		}
		
		if (hideLives == true)
		{
			_healthBar.Visible = false;
			_statusIcons.Visible = false;
		}
	}
	
	public void ChangeStatus(string status)
	{
		switch(status)
		{
			case "none":
				_statusIcons.Visible = false;
				break;
			case "attack":
				_statusIcons.Visible = true;
				_statusIcons.Play("attack");
				break;
			case "attack_buff":
				_statusIcons.Visible = true;
				_statusIcons.Play("attack_buff");
				break;
			case "bulb":
				_statusIcons.Visible = true;
				_statusIcons.Play("bulb");
				break;
			case "confusion":
				_statusIcons.Visible = true;
				_statusIcons.Play("confusion");
				break;
			case "defense_buff":
				_statusIcons.Visible = true;
				_statusIcons.Play("defense_buff");
				break;
			case "dread":
				_statusIcons.Visible = true;
				_statusIcons.Play("dread");
				break;
			case "exclamation_mark":
				_statusIcons.Visible = true;
				_statusIcons.Play("exclamation_mark");
				break;
		}
		
	}
	
	public void CalculateLives(int changeNumOfLives)
	{
		numOfLives += changeNumOfLives;
		
		if(numOfLives < 0)
			numOfLives = 0;
		else if (numOfLives > 5)
			numOfLives = 5;
		
		for (int i = 0; i < 5; i++) 
		{
			if(i < numOfLives)
 				_cells[i].Play("full");
			else if(i >= numOfLives)
 				_cells[i].Play("empty");
		}
	}
	
}

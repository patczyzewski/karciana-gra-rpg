using Godot;
using System;

public partial class EnemyHealthBar : Node2D
{
	public int numOfLives = 10;
	
	private RichTextLabel _textLabel;
	private Sprite2D[] _cells = new Sprite2D[10];
	
	public override void _Ready()
	{
		_textLabel = GetNode<RichTextLabel>("RichTextLabel");
		
		_cells[0] = GetNode<Sprite2D>("LifeCell 1");
		_cells[1] = GetNode<Sprite2D>("LifeCell 2");
		_cells[2] = GetNode<Sprite2D>("LifeCell 3");
		_cells[3] = GetNode<Sprite2D>("LifeCell 4");
		_cells[4] = GetNode<Sprite2D>("LifeCell 5");
		_cells[5] = GetNode<Sprite2D>("LifeCell 6");
		_cells[6] = GetNode<Sprite2D>("LifeCell 7");
		_cells[7] = GetNode<Sprite2D>("LifeCell 8");
		_cells[8] = GetNode<Sprite2D>("LifeCell 9");
		_cells[9] = GetNode<Sprite2D>("LifeCell 10");
		
		CalculateLives(0);
		SetEnemyName("Skin Walker");
	}
	
	public void SetEnemyName(string enemyName)
	{
		_textLabel.AppendText(enemyName);
	}
	
	public void CalculateLives(int changeNumOfLives)
	{
		numOfLives += changeNumOfLives;
		
		if(numOfLives < 0)
			numOfLives = 0;
		else if (numOfLives > 10)
			numOfLives = 10;
		
		for (int i = 0; i < 10; i++) 
		{
			if(i < numOfLives)
 				_cells[i].Visible = true;
			else if(i >= numOfLives)
 				_cells[i].Visible = false;
		}
	}
}

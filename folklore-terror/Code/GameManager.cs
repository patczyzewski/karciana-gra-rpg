using Godot;
using System;

public partial class GameManager : Node2D
{
	// UI & Functionality
	[Export] public Node2D BattleTrigger;
	[Export] public DialogBox GameDialogBox;
	[Export] public CardHand GameCardHand;
	
	private CustomSignals _customSignal;
	
	public bool playerPlayedCard;
	public bool playerTurn = true;
	public bool enemyTurn;
	private bool _gameEnded;
	private bool _gameStarted;
	
	private bool _dialogActive;
	private bool _dialogScenarioActive;
	
	// Player
	[Export] public Player Player;
	
	public string playerStatus;
	public bool isPlayerConfused = false;
	
	public int playerDefenseBuff = 0;
	public int playerAttackBuff = 0;
	public int playerRegenerationBuff = 0;
	
	// Enemy
	[Export] public Enemy Enemy;
	[Export] public EnemyHealthBar EnemyHealth;
	
	public string enemyStatus;
	public bool isEnemyConfused = false;
	
	public int enemyDefenseBuff = 0;
	public int enemyAttackBuff = 0;
	public int enemyRegenerationBuff = 0;
	public int enemyConfusedTurns = -1;
	
	
	public override void _Ready()
	{
		_customSignal = GetNode<CustomSignals>("/root/CustomSignals");
		_customSignal.BattleStarted += PlayerEntered;
	}
	public override void _Process(double delta)
	{
		Battle();
	}
	
	public void Battle()
	{
		if (EnemyHealth.numOfLives <= 0 && _gameEnded == false)
		{
			DisplayText("win");
		}
		else if (Player.numOfLives <= 0 && _gameEnded == false)
		{
			DisplayText("lose");
		}

		if (GameDialogBox.Visible == false && EnemyHealth.numOfLives == 0 && _gameEnded == false)
		{
			_gameEnded = true;
			ChangeTurns();
		}
			
		
		
		
		if (enemyConfusedTurns == 0 && playerTurn == false)
		{
			
			DisplayText("enemyIsNotConfused");
		}
		else if (isEnemyConfused == true && playerTurn == false)
		{
			DisplayText("enemyIsConfused");
		}
		
		if (playerTurn == false)
		{
			DisplayText("enemy");
		}
	}
	
	public void PlayerTurn()
	{
		if (!_dialogActive) // Sprawdzamy też, czy flaga nie jest już ustawiona
		{
			DisplayText("card"); // Wywołaj metodę do wyświetlenia dialogu
		}
		
		if (GameCardHand.chosenCard.Damage < 0)
		{
			Health("enemy", GameCardHand.chosenCard.Damage);
			Enemy.PlayEffect("punch");
		}
		
		if (GameCardHand.chosenCard.Effect != null)
		{
			if (GameCardHand.chosenCard.Target == "Enemy")
				Status("enemy", GameCardHand.chosenCard.Effect);
			if (GameCardHand.chosenCard.Target == "Player")
				Status("player", GameCardHand.chosenCard.Effect);
			
			if (GameCardHand.chosenCard.Effect == "confusion")
			{
				enemyConfusedTurns = 2;
				Player.PlayEffect("scream");
			}
				
		}
	}
	
	public void EnemyTurn()
	{
		
	}
	
	public void ChangeTurns()
	{
		// Jeśli tekst jest w pełni wypisany, ukryj dialog
		GameDialogBox.HideDialog();
		_dialogActive = false; // Resetujemy flagę, bo dialog się zakończył
		
		if(_gameStarted == false)
		{
			_gameStarted = true;
			playerTurn = false;

			Player.HideLives(false);
			GameCardHand.Visible = true;
			EnemyHealth.Visible = true;
		}
		
		if (playerTurn == true && _gameEnded == false)
		{
			playerTurn = false;
		}
		else if (playerTurn == false && _gameEnded == false) // Zwróć graczowi karty na jego turę
		{
			GameCardHand.HideCards();
			playerTurn = true;
		}
		
		if (_gameEnded == true)
		{
			playerTurn = true;
			GameCardHand._areCardsHidden = true;
			GameCardHand.Visible = false;
			Player.HideLives(true);
			EnemyHealth.Visible = false;
			Enemy.Visible = false;
			Player.LockMovement = false;
			
		}
	}
	
	private void PlayerEntered(bool isGameStarted)
	{
		GD.Print("Gracz wszedł na Area2D...");
		if (EnemyHealth.numOfLives > 0)
		{
			Player.LockMovement = true;
			DisplayText("startBattle");
		}
		
		
	}
	
	private async void DisplayText(string type)
	{
		if (_dialogActive) 
		{
			return;
		}
		
		_dialogActive = true; // dialog się rozpoczął
		
		if (type == "card")
		{
			await GameDialogBox.ShowDialog(GameCardHand.cardData.Find(p => p.Name == GameCardHand.chosenCard.CardName).Description); 
			ChangeTurns();
		}
			
		if (type == "enemy")
		{
			Health("player", -1);
			Player.PlayEffect("slash");
			await GameDialogBox.ShowDialog(new string[]{"Przeciwnik ostrzy swoje pazury...", "Czujesz strach."}); 
			ChangeTurns();
		}
		if (type == "win")
		{
			await GameDialogBox.ShowDialog(new string[]{"Wygrales."}); 
		}
		if (type == "lose")
		{
			await GameDialogBox.ShowDialog(new string[]{"Przegrales."}); 
		}
		if (type == "startBattle")
		{
			await GameDialogBox.ShowDialog(new string[]{"Widzisz cos, co mrozi Ci krew w zylach...",
			"W ciemnosci wylania sie para krwawych oczu",
			"Nogi lamia ci sie pod ciezarem strachu...",
			"Nie dasz rady uciec...",
			"Musisz walczyc."});
			GameCardHand._areCardsHidden = false;
			ChangeTurns();
		}
		if (type == "enemyIsConfused")
		{
			playerTurn = false;
			enemyConfusedTurns--;
			await GameDialogBox.ShowDialog(new string[]{"Przeciwnik jest zdezorientowany!"});
			ChangeTurns();
		}
		if (type == "enemyIsNotConfused")
		{
			playerTurn = false;
			enemyConfusedTurns = -1;
			Status("enemy", "none");
			await GameDialogBox.ShowDialog(new string[]{"Przeciwnik nie jest juz dluzej zdezorientowany!"});
			
			ChangeTurns();
		}
	}
	
	public override void _Input(InputEvent @event)
	{
		// Sprawdzamy, czy wciśnięto akcję "ui_accept"
		if (@event.IsActionPressed("ui_accept"))
		{
			// Jeśli dialog jest obecnie aktywny (widoczny)
			if (GameDialogBox.Visible)
			{
				// Jeśli okno dialogowe jest w ogóle widoczne
				if (GameDialogBox.IsDialogActive) // Używamy nowej właściwości IsDialogActive
				{
					// Wywołujemy nową metodę GoToNextLine()
					GameDialogBox.GoToNextLine();
					
					// Jeśli po GoToNextLine dialog został ukryty, to scenariusz się zakończył
		  		  	if (!GameDialogBox.IsDialogActive)
					{
						_dialogScenarioActive = false;
					}
				}
			}
		}
	}
	
	public void Status(string target, string status)
	{
		if (target == "player")
		{
			if (status == "none")
			{
				Player.ChangeStatus("none");
			}
			else if (status == "defense_buff")
			{
				Player.ChangeStatus("defense_buff");
				playerAttackBuff = 0;
				playerDefenseBuff = 1;
			}
			else if (status == "attack_buff")
			{
				Player.ChangeStatus("attack_buff");
				playerDefenseBuff = 0;
				playerAttackBuff += -1; // Można stack'ować
			}
			else if (status == "exclamation_mark")
			{
				Player.ChangeStatus("none");
				playerDefenseBuff = 0;
				playerAttackBuff = 0;
				Health("player", 2);
			}
		}
		
		if (target == "enemy")
		{
			if (status == "none")
			{
				Enemy.ChangeStatus("none");
				isEnemyConfused = false;
			}
			else if (status == "confusion")
			{
				Enemy.ChangeStatus("confusion");
				isEnemyConfused = true;
				enemyConfusedTurns = 2;
			}
		}
	}
	
	public void Health(string target, int health)
	{
		if (target == "player" && health < 0)
		{
			Player.CalculateLives(health + enemyAttackBuff + playerDefenseBuff);
		}
		else if (target == "player" && health > 0)
		{
			Player.CalculateLives(health + playerRegenerationBuff);
		}
		
		if (target == "enemy" && health < 0)
		{
			EnemyHealth.CalculateLives(health + playerAttackBuff + enemyDefenseBuff);
		}
		else if (target == "enemy" && health > 0)
		{
			EnemyHealth.CalculateLives(health + enemyRegenerationBuff);
		}
	}
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardHand : Node2D
{
	[Export] public PackedScene CardScene { get; set; } 
	[Export] public Texture2D CardAtlasTexture { get; set; } 

	[Export] public float CardSpacing = 100; 
	[Export] public float HandWidth = 100; 
	[Export] public float BaseYPosition = 500; 
	
	public GameManager gameManager;
	
	private List<Card> _cardsInHand = new List<Card>();
	private Card _currentHoveredCard = null; 
	public Card chosenCard = null;

	private int _totalNumberOfCards = 0;
	private int _numberOfCards = 0;
	private int _hoverZIndex = 100; 
	private int _baseZIndex = 1; 
	private Random _random = new Random();

	public bool _areCardsHidden = true;
	[Export] public float HideOffset = 50; 

	[Export] public uint CardCollisionLayer = 1; 

	private bool _startHideAnimation = false; 
	private bool _isProcessingCardClick = false; // NOWA FLAGA: zapobiega wielokrotnemu klikaniu
	
	public List<CardData> cardData;

	public void HideCards()
	{
		GetTree().CreateTimer(0.2f).Timeout += OnHideTimerTimeout; 
	}
	
	public override void _Ready()
	{
		GD.Print("Inicjalizacja listy przedmiotów...");
		
		this.Visible = false;
		
		gameManager = GetNode<GameManager>("../..");
		
		cardData = new List<CardData>();

		cardData.Add(new CardData("Wola Walki", new Rect2(0, 0, 48, 64), new string[]{"Oddech staje sie spokojniejszy, umysl ostrzejszy.", "Atakujesz przeciwnika pod wplywem slepej furii."}, -2));
		cardData.Add(new CardData("Jasny Umysl", new Rect2(48, 0, 48, 64), new string[]{"Mysli staja sie klarowne. Widzisz to, czego inni nie dostrzegaja.", "Twoje ataki trafiaja mocniej."}, "attack_buff", "Player"));
		cardData.Add(new CardData("Krzyk", new Rect2(96, 0, 48, 64), new string[]{"Gluchy wrzask rozchodzi sie w twojej glowie..."}, "confusion", "Enemy", -1));
		cardData.Add(new CardData("Pamiec o Rodzinie", new Rect2(144, 0, 48, 64), new string[]{"Obraz ukochanej osoby dodaje ci sil. Nie jestes sam.", "Twoja obrona staje sie potezniejsza."}, "defense_buff", "Player"));
		cardData.Add(new CardData("Sanacja", new Rect2(192, 0, 48, 64), new string[]{"To tylko sen... Probujesz odnalezc logiczne wytlumaczenie.", "Przywracasz sobie poczytalnosc, jednak tracisz swoje wzmocnienia."}, "exclamation_mark", "Player"));
		
		if (CardAtlasTexture == null)
		{
			GD.PrintErr("Błąd: CardAtlasTexture nie został przypisany w Inspektorze CardHand!");
			return;
		}
	}
	
	public override void _Process(double delta) 
	{
		if (Input.IsActionJustPressed("debug") && _numberOfCards < 9)
		{
			if (_areCardsHidden) return; 
			int randomNumber = _random.Next(1, 6);
			switch(randomNumber)
			{
				case 1:
					AddCard("Wola Walki");
					break;
				case 2:
					AddCard("Jasny Umysl");
					break;
				case 3:
					AddCard("Krzyk");
					break;
				case 4:
					AddCard("Pamiec o Rodzinie");
					break;
				case 5:
					AddCard("Sanacja");
					break;
			}
			_numberOfCards++;
			_totalNumberOfCards++;
		}
		
		// Gracz zagrał kartę, tym samym kończy swoją turę.
		if (_startHideAnimation)
		{
			_startHideAnimation = false; 
			_areCardsHidden = true; 
			
			AnimateCardsVertical(true); 
			
			GetTree().CreateTimer(0.2f).Timeout += () => 
			{
				ArrangeCards(); 
				foreach(var card in _cardsInHand)
				{
					card.Position = new Vector2(card.Position.X, BaseYPosition + HideOffset);
				}
				
				gameManager.PlayerTurn();
			};
		}
	}

	public void AddCard(string cardName)
	{
		if (CardScene == null)
		{
			GD.PrintErr("Błąd: Scena Card (Card.tscn) nie została przypisana do CardHand w Inspektorze!");
			return;
		}
		
		var newCard = CardScene.Instantiate<Card>(); 
		
		if (newCard == null)
		{
			GD.PrintErr("Błąd: Instancjonowanie karty zwróciło NULL. Sprawdź plik Card.tscn i jego główne węzły.");
			return;
		}
		
		if (CardAtlasTexture == null)
		{
			GD.PrintErr("Błąd: CardAtlasTexture jest NULL w CardHand.AddCard. Nie można ustawić regionu dla karty.");
			newCard.QueueFree(); 
			return;
		}
		
		newCard.CardName = cardName; 
		newCard.CardId = _totalNumberOfCards;
		newCard.Damage = cardData.Find(p => p.Name == cardName).Damage;
		newCard.Effect = cardData.Find(p => p.Name == cardName).Effect;
		newCard.Target = cardData.Find(p => p.Name == cardName).Target;
		
		AddChild(newCard); 
		
		newCard.SetCardRegion(CardAtlasTexture, cardData.Find(p => p.Name == cardName).CardRegions);
		
		_cardsInHand.Add(newCard); 
		
		// Zawsze podłączaj sygnały, upewnij się, że nie są duplikowane
		newCard.CardMouseEntered += OnCardMouseEntered;
		newCard.CardMouseExited += OnCardMouseExited;
		newCard.CardClicked += OnCardClicked; 
		
		ArrangeCards(); 
	}

	public void RemoveCard(int cardId)
	{
		Card cardToRemove = null;
		cardToRemove = _cardsInHand.FirstOrDefault(card => card.CardId == cardId);
		
		if (cardToRemove != null)
		{
			// --- KLUCZOWA ZMIANA: Dodatkowe zabezpieczenie przed sygnałami ---
			// Upewnij się, że sygnały są odłączone, nawet jeśli były podłączone wielokrotnie.
			// Godot sam obsługuje wielokrotne odłączenia bez błędów.
			cardToRemove.CardMouseEntered -= OnCardMouseEntered;
			cardToRemove.CardMouseExited -= OnCardMouseExited;
			cardToRemove.CardClicked -= OnCardClicked;

			if (_currentHoveredCard == cardToRemove)
			{
				cardToRemove.AnimateHoverEffect(false); 
				_currentHoveredCard = null;
			}
			chosenCard = cardToRemove;
			_cardsInHand.Remove(cardToRemove);
			_numberOfCards--;
			
			
			
			GD.Print($"Karta '{cardId}' : '{cardToRemove.CardName}' została usunięta.");
			
			cardToRemove.QueueFree(); 
		}
		else
		{
			GD.PrintErr($"Karta o nazwie '{cardId}' nie znaleziona w ręce.");
		}
	}

	private void ArrangeCards()
	{
		int numCards = _cardsInHand.Count;
		if (numCards == 0) return;

		float cardWidth = 0;
		if (_cardsInHand.Count > 0)
		{
			cardWidth = _cardsInHand[0].GetCardWidth();
		}
		else if (CardScene != null && CardAtlasTexture != null && cardData.Count > 0)
		{
			var dummyCard = CardScene.Instantiate<Card>();
			dummyCard.SetCardRegion(CardAtlasTexture, cardData[0].CardRegions); 
			cardWidth = dummyCard.GetCardWidth();
			dummyCard.QueueFree(); 
		}

		if (cardWidth == 0)
		{
			GD.PrintErr("Błąd: Nie można uzyskać szerokości karty. Sprawdź, czy regiony są zdefiniowane i TextureRect w scenie Card jest prawidłowo skonfigurowany.");
			return;
		}

		float totalWidth = (numCards - 1) * CardSpacing + cardWidth; 

		if (totalWidth > HandWidth)
		{
			CardSpacing = (HandWidth - cardWidth) / Math.Max(1, numCards - 1);
			totalWidth = HandWidth; 
		}

		float startX = (HandWidth / 2) - (totalWidth / 2);

		for (int i = 0; i < numCards; i++)
		{
			Card card = _cardsInHand[i];
			float currentYPosition = BaseYPosition; 
			Vector2 targetPosition = new Vector2(startX + i * CardSpacing, currentYPosition);

			card.SetOriginalPosition(targetPosition); 
			card.ZIndex = _baseZIndex + i; 
		}
	}
	
	private void OnCardMouseEntered(Card card)
	{
		if (_areCardsHidden || _isProcessingCardClick) return; // Zablokuj, jeśli ukryte lub w trakcie klikania

		if (_currentHoveredCard == card)
		{
			return; 
		}

		if (_currentHoveredCard != null && _currentHoveredCard != card)
		{
			int originalIndex = _cardsInHand.IndexOf(_currentHoveredCard);
			if (originalIndex != -1)
			{
				_currentHoveredCard.ZIndex = _baseZIndex + originalIndex;
			}
			else
			{
				_currentHoveredCard.ZIndex = _baseZIndex; 
			}
			_currentHoveredCard.AnimateHoverEffect(false); 
		}

		_currentHoveredCard = card;
		_currentHoveredCard.ZIndex = _hoverZIndex; 
		_currentHoveredCard.AnimateHoverEffect(true); 
	}

	private void OnCardMouseExited(Card card)
	{
		if (_areCardsHidden || _isProcessingCardClick) return; // Zablokuj, jeśli ukryte lub w trakcie klikania

		if (_currentHoveredCard == card)
		{
			int originalIndex = _cardsInHand.IndexOf(card);
			if (originalIndex != -1)
			{
				card.ZIndex = _baseZIndex + originalIndex;
			}
			else
			{
				card.ZIndex = _baseZIndex; 
			}
			card.AnimateHoverEffect(false); 
			_currentHoveredCard = null; 
		}
	}
	
	private void OnCardClicked(Card card)
	{
		if (_areCardsHidden || _isProcessingCardClick) return; // Zablokuj, jeśli już przetwarzamy kliknięcie
		_isProcessingCardClick = true; // Ustaw flagę, że przetwarzamy kliknięcie

		GD.Print($"Karta '{card.CardName}' została kliknięta.");
		
		SetCardsInteractive(false); 
		
		// Natychmiastowe zresetowanie wszelkich efektów najechania
		if (_currentHoveredCard != null)
		{
			_currentHoveredCard.AnimateHoverEffect(false); 
			int originalIndex = _cardsInHand.IndexOf(_currentHoveredCard);
			if (originalIndex != -1)
			{
				_currentHoveredCard.ZIndex = _baseZIndex + originalIndex;
			}
			else
			{
				_currentHoveredCard.ZIndex = _baseZIndex;
			}
			_currentHoveredCard = null;
		}

		// Odłączenie sygnałów klikniętej karty przed usunięciem
		card.CardMouseEntered -= OnCardMouseEntered;
		card.CardMouseExited -= OnCardMouseExited;
		card.CardClicked -= OnCardClicked;

		RemoveCard(card.CardId); 

		_startHideAnimation = true; 
	}

	private void OnHideTimerTimeout()
	{
		GD.Print("Timer upłynął, przywracam karty.");
		_areCardsHidden = false; 
		AnimateCardsVertical(false); 
		
		GetTree().CreateTimer(0.36f).Timeout += () =>
		{
			SetCardsInteractive(true); 
			ResetHoverState(); 
			_isProcessingCardClick = false; 
		};
	}

	private void AnimateCardsVertical(bool hide)
	{
		foreach (var card in _cardsInHand)
		{
			Area2D cardArea = card.GetNode<Area2D>("Area2D"); 

			var tween = card.CreateTween();
			float targetY = BaseYPosition + (hide ? HideOffset : 0);
			
			tween.TweenProperty(card, "position:y", targetY, 0.35f) 
				 .SetTrans(Tween.TransitionType.Quad)
				 .SetEase(Tween.EaseType.Out);
		}
	}

	private void SetCardsInteractive(bool interactive)
	{
		// --- KLUCZOWA ZMIANA: Upewnij się, że stan hover jest resetowany globalnie ---
		if (!interactive && _currentHoveredCard != null)
		{
			_currentHoveredCard.AnimateHoverEffect(false);
			// Nie ruszaj ZIndex, bo i tak zaraz będą chowane lub interakcje wyłączone
			_currentHoveredCard = null; 
		}

		foreach (var card in _cardsInHand)
		{
			Area2D cardArea = card.GetNode<Area2D>("Area2D");
			if (cardArea != null)
			{
				cardArea.InputPickable = interactive; 
				
				if (interactive)
				{
					cardArea.CollisionLayer = CardCollisionLayer; 
				}
				else
				{
					cardArea.CollisionLayer = 0; 
				}
			}
		}
		// --- Dodatkowe zabezpieczenie: wymuś odświeżenie wejścia ---
		// To może być pomocne w bardzo rzadkich przypadkach.
		GetViewport().SetInputAsHandled(); 
	}

	private void ResetHoverState()
	{
		Vector2 mousePos = GetViewport().GetMousePosition();
		
		Card newHoveredCard = null;

		if (_areCardsHidden || _isProcessingCardClick) // Zablokuj, jeśli ukryte lub w trakcie klikania
		{
			if (_currentHoveredCard != null)
			{
				OnCardMouseExited(_currentHoveredCard);
			}
			return; 
		}

		foreach (var card in _cardsInHand.OrderByDescending(c => c.ZIndex)) 
		{
			Area2D cardArea = card.GetNode<Area2D>("Area2D");
			// Dodatkowy warunek: sprawdź InputPickable, aby nie resetować hovera na nieaktywnych kartach
			if (cardArea != null && cardArea.CollisionLayer != 0 && cardArea.InputPickable) 
			{
				CollisionShape2D collisionShape = cardArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D"); 
				
				if (collisionShape != null && collisionShape.Shape != null)
				{
					Shape2D shape = collisionShape.Shape;
					Vector2 localMousePos = cardArea.ToLocal(mousePos); 

					if (shape is RectangleShape2D rectShape)
					{
						Rect2 localRect = new Rect2( -rectShape.Size / 2.0f, rectShape.Size);
						
						if (localRect.HasPoint(localMousePos))
						{
							newHoveredCard = card;
							break; 
						}
					}
					else if (shape is CircleShape2D circleShape)
					{
						if (localMousePos.LengthSquared() <= circleShape.Radius * circleShape.Radius)
						{
							newHoveredCard = card;
							break;
						}
					}
					else
					{
						GD.PrintErr($"Nieobsługiwany typ kształtu kolizji dla karty '{card.CardName}': {shape.GetType().Name}. Proszę dodać obsługę w ResetHoverState.");
					}
				}
			}
		}

		if (newHoveredCard != null && _currentHoveredCard != newHoveredCard)
		{
			OnCardMouseEntered(newHoveredCard); 
		}
		else if (newHoveredCard == null && _currentHoveredCard != null)
		{
			OnCardMouseExited(_currentHoveredCard); 
		}
	}
}

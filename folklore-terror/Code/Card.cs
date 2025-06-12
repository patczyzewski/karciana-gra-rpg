using Godot;
using System;

public partial class Card : Node2D // Klasa dziedziczy po Node2D
{
	// Właściwości karty, które możesz ustawić w edytorze lub kodzie
	[Export] public string CardName { get; set; } // Nazwa karty (np. "Karta A", "Karta B")
	public int CardId { get; set; }
	
	public string Effect { get; set; }
	public string Target { get; set; }
	public int Damage { get; set; }
	
	// Zmienne prywatne do zarządzania stanem karty
	private Vector2 _originalPosition; // Oryginalna pozycja karty w ręce
	private int _originalZIndex;       // Oryginalna kolejność rysowania (głębokość)
	private bool _isHovered = false;   // Czy kursor myszy jest nad kartą

	// Parametry efektu "wyskakiwania" po najechaniu
	private Vector2 _hoverOffset = new Vector2(0, -30); // O ile pikseli karta ma się podnieść w górę
	private int _hoverZIndex = 100; // Z-Index dla podniesionej karty (musi być wysoki, by była na wierzchu)

	// Referencje do węzłów potomnych, by mieć do nich szybki dostęp
	private TextureRect _textureRect; // Węzeł wyświetlający obrazek karty
	private Area2D _area2D;           // Węzeł do wykrywania interakcji myszy
	private PointLight2D _cardLight;
	
	[Signal]
	public delegate void CardClickedEventHandler(Card card);
	[Signal]
	public delegate void CardMouseEnteredEventHandler(Card card);
	[Signal]
	public delegate void CardMouseExitedEventHandler(Card card);
	[Signal]
	public delegate void CardClickedIdEventHandler(int cardId);

	// Metoda _Ready() jest wywoływana, gdy węzeł i jego dzieci są gotowe do użycia
	public override void _Ready()
	{
		// Pobieramy referencje do węzłów potomnych
		_textureRect = GetNode<TextureRect>("TextureRect");
		_area2D = GetNode<Area2D>("Area2D");
		_cardLight = GetNode<PointLight2D>("PointLight2D");
		
		if (_cardLight != null)
		{
			_cardLight.Energy = 0.0f;
		}

		// Ustawiamy tekst etykiety na karcie (nazwa karty)
		GetNode<Label>("Label").Text = CardName; 

		// Podłączamy sygnały z Area2D do naszych metod
		// InputEvent wykrywa ogólne zdarzenia wejścia (np. kliknięcia)
		_area2D.InputEvent += OnAreaInputEvent;
		_area2D.MouseEntered += OnCardAreaMouseEntered; // Zmieniamy nazwę, żeby uniknąć kolizji z sygnałem CardHand
		_area2D.MouseExited += OnCardAreaMouseExited;   // Zmieniamy nazwę

		// Zapisujemy początkową pozycję i Z-Index karty.
		// Będą one używane do powrotu karty do stanu normalnego po najechaniu.
		_originalPosition = Position; 
		_originalZIndex = ZIndex;
	}

	// Metoda wywoływana przez CardHand do ustawiania początkowej pozycji karty
	public void SetOriginalPosition(Vector2 pos)
	{
		_originalPosition = pos;
		Position = pos; // Ustawia lokalną pozycję węzła Card
	}

	// --- WAŻNA NOWA METODA: Ustawianie fragmentu tekstury z atlasu ---
	// W Card.cs, w metodzie SetCardRegion

public void SetCardRegion(Texture2D atlasTexture, Rect2 regionRect)
{
	// Dodajemy komunikat, aby sprawdzić, co dzieje się z _textureRect.Texture
	if (_textureRect == null)
	{
		GD.PrintErr("Błąd w Card.SetCardRegion: _textureRect jest NULL!");
		return; // Przerywamy, bo nie ma sensu kontynuować
	}
	
	// Sprawdzamy typ aktualnie przypisanej tekstury
	GD.Print($"Debug: Typ tekstury w TextureRect: {_textureRect.Texture?.GetType().Name ?? "NULL"}"); // Pokaże, co jest w _textureRect.Texture

	if (_textureRect.Texture is AtlasTexture atlas)
	{
		GD.Print("Debug: TextureRect.Texture JEST AtlasTexture. Kontynuuję duplikowanie.");
		atlas = (AtlasTexture)atlas.Duplicate(true); 
		_textureRect.Texture = atlas; 

		atlas.Atlas = atlasTexture; 
		atlas.Region = regionRect;  
	}
	else
	{
		GD.PrintErr("Błąd: TextureRect nie używa AtlasTexture, mimo że powinien! Być może scena Card.tscn nie jest poprawnie skonfigurowana lub używasz złej instancji.");
		_textureRect.Texture = atlasTexture; 
	}

	_textureRect.Size = regionRect.Size; 
}

	// Metoda wywoływana, gdy kursor myszy wejdzie w obszar Area2D karty
	private void OnCardAreaMouseEntered()
	{
		if (!_isHovered) // Sprawdzamy, czy karta już nie jest w stanie "hover"
		{
			_isHovered = true;
			AnimateHoverEffect(true); // Rozpoczynamy animację "wyskakiwania"
			EmitSignal(SignalName.CardMouseEntered, this);
		}
	}

	// Metoda wywoływana, gdy kursor myszy opuści obszar Area2D karty
	private void OnCardAreaMouseExited()
	{
		if (_isHovered) // Sprawdzamy, czy karta była w stanie "hover"
		{
			_isHovered = false;
			AnimateHoverEffect(false); // Rozpoczynamy animację powrotu
			EmitSignal(SignalName.CardMouseExited, this);
		}
	}

	// >>> POPRAWIONA SYGNATURA METODY! Zmieniono 'int shapeIdx' na 'long shapeIdx'. <<<
	private void OnAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		// Sprawdzamy, czy zdarzenie to kliknięcie lewym przyciskiem myszy
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			// Sprawdzamy, czy przycisk został zwolniony (to zapobiega wielokrotnym kliknięciom przy przytrzymaniu)
			if (mouseButtonEvent.IsReleased() && mouseButtonEvent.ButtonIndex == MouseButton.Left)
			{
				GD.Print($"Kliknięto kartę: {CardName}");
				// Tutaj możesz dodać własną logikę po kliknięciu karty,
				// np. emitować sygnał do CardHand, by wiedział, że karta została wybrana.
				EmitSignal(SignalName.CardClicked, this); 
			}
		}
	}
	
	// Metoda do płynnej animacji pozycji karty (przesunięcie do przodu/z tyłu)
	public void AnimateHoverEffect(bool enter)
	{
		var tween = CreateTween(); // Tworzymy obiekt Tween do animacji
		if (enter) // Jeśli mysz weszła na kartę
		{
			tween.TweenProperty(this, "position", _originalPosition + _hoverOffset, 0.15f)
				 .SetTrans(Tween.TransitionType.Quad) // Używamy kwadratowego przejścia dla płynności
				 .SetEase(Tween.EaseType.Out);         // Animacja zwalnia pod koniec
			ZIndex = _hoverZIndex; // Zmieniamy Z-Index, by karta rysowała się na wierzchu
			
			tween.TweenProperty(_cardLight, "energy", 1.0f, 0.15f) // Zwiększ energy do 1.0
					 .SetTrans(Tween.TransitionType.Quad)
					 .SetEase(Tween.EaseType.Out);
		}
		else // Jeśli mysz opuściła kartę
		{
			tween.TweenProperty(this, "position", _originalPosition, 0.15f)
				 .SetTrans(Tween.TransitionType.Quad)
				 .SetEase(Tween.EaseType.Out);
			ZIndex = _originalZIndex; // Przywracamy oryginalny Z-Index
			
			tween.TweenProperty(_cardLight, "energy", 0.0f, 0.15f) // Zmniejsz energy do 0.0
					 .SetTrans(Tween.TransitionType.Quad)
					 .SetEase(Tween.EaseType.Out);
		}
	}

	// Metoda do pobierania szerokości karty, przydatna dla CardHand
	public float GetCardWidth()
	{
		// Zwracamy szerokość regionu z TextureRect, ponieważ to on definiuje wizualny rozmiar karty
		if (_textureRect != null && _textureRect.Texture is AtlasTexture atlas)
		{
			return atlas.Region.Size.X; 
		}
		// Awaryjnie, jeśli nie ma AtlasTexture, zwracamy szerokość samego TextureRect
		if (_textureRect != null) return _textureRect.Size.X;
		return 0; // Jeśli coś poszło nie tak
	}
}

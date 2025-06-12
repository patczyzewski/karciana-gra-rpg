using Godot;
using System;
using System.Threading.Tasks;

public partial class DialogBox : Node2D
{
	[Export] public RichTextLabel TextLabel;
	[Export] public NinePatchRect Background;
	[Export] public float CharactersPerSecond = 20f;
	[Export] public Color DefaultTextColor = Colors.White;

	private string[] _dialogLines; // Nowa tablica do przechowywania wielu linii dialogowych
	private int _currentLineIndex = 0; // Bieżący indeks linii
	private string _fullText = "";
	private bool _isTyping = false;
	private float _timeSinceLastChar = 0f;
	private int _currentCharIndex = 0;
	private TaskCompletionSource<bool> _dialogCompletionSource;
	private bool _waitingForInput = false; // Nowa flaga: czy czekamy na input gracza po zakończeniu linii

	private const string RichTextLabelColorOverrideName = "default_color";

	public override void _Ready()
	{
		if (TextLabel == null)
		{
			GD.PrintErr("TextLabel RichTextLabel nie jest przypisany w DialogBox!");
			SetProcess(false);
			return;
		}
		if (Background == null)
		{
			GD.PrintErr("Background NinePatchRect nie jest przypisany w DialogBox!");
			SetProcess(false);
			return;
		}

		Visible = false;
		TextLabel.AddThemeColorOverride(RichTextLabelColorOverrideName, DefaultTextColor);
	}

	public override void _Process(double delta)
	{
		if (_isTyping)
		{
			_timeSinceLastChar += (float)delta;
			float timePerChar = 1f / CharactersPerSecond;

			if (_timeSinceLastChar >= timePerChar)
			{
				int charsToAdd = (int)(_timeSinceLastChar / timePerChar);
				_timeSinceLastChar -= charsToAdd * timePerChar;

				for (int i = 0; i < charsToAdd; i++)
				{
					if (_currentCharIndex < _fullText.Length)
					{
						TextLabel.AppendText(_fullText[_currentCharIndex].ToString());
						_currentCharIndex++;
					}
					else
					{
						_isTyping = false;
						_waitingForInput = true; // Zakończono pisanie linii, czekaj na input
						GD.Print("Linia zakończona. Czekam na input gracza.");
						break;
					}
				}
			}
		}
	}

	/// <summary>
	/// Rozpoczyna wyświetlanie serii linii dialogowych.
	/// </summary>
	/// <param name="lines">Tablica stringów, każda reprezentuje jedną linię dialogu.</param>
	/// <param name="speed">Opcjonalna prędkość pisania.</param>
	/// <param name="color">Opcjonalny kolor tekstu.</param>
	/// <returns>Task, który zakończy się po przejściu przez wszystkie linie i zamknięciu dialogu.</returns>
	public async Task ShowDialog(string[] lines, float? speed = null, Color? color = null)
	{
		if (_isTyping || _waitingForInput || _dialogCompletionSource != null)
		{
			GD.PrintErr("Inny dialog jest już wyświetlany lub oczekuje na input. Nie można rozpocząć nowego.");
			return;
		}

		_dialogLines = lines;
		_currentLineIndex = 0;
		_dialogCompletionSource = new TaskCompletionSource<bool>(); // Resetuj TaskCompletionSource
		Visible = true; // Pokaż okno dialogowe

		// Ustaw kolor tekstu
		Color textColorToUse = color ?? DefaultTextColor;
		TextLabel.AddThemeColorOverride(RichTextLabelColorOverrideName, textColorToUse);

		// Ustaw prędkość
		float originalSpeed = CharactersPerSecond;
		if (speed.HasValue)
		{
			CharactersPerSecond = speed.Value;
		}

		// Rozpocznij wyświetlanie pierwszej linii
		DisplayNextLine();

		// Czekaj, aż TaskCompletionSource zostanie zakończony (czyli cały dialog zostanie zamknięty)
		await _dialogCompletionSource.Task;

		// Przywróć oryginalną prędkość i kolor
		if (speed.HasValue)
		{
			CharactersPerSecond = originalSpeed;
		}
		TextLabel.AddThemeColorOverride(RichTextLabelColorOverrideName, DefaultTextColor);

		GD.Print("Cały dialog zakończony.");
	}

	/// <summary>
	/// Wyświetla następną linię dialogu lub zamyka dialog, jeśli wszystkie linie zostały wyświetlone.
	/// Wywoływana po zakończeniu pisania bieżącej linii lub po kliknięciu "Dalej".
	/// </summary>
	public void GoToNextLine()
	{
		if (_isTyping)
		{
			// Jeśli tekst się pisze, pomiń pisanie
			SkipTyping();
			return;
		}
		
		// Jeśli czekamy na input po zakończeniu linii
		if (_waitingForInput)
		{
			_currentLineIndex++; // Przejdź do następnego indeksu
			if (_currentLineIndex < _dialogLines.Length)
			{
				// Mamy więcej linii do wyświetlenia
				_waitingForInput = false; // Już nie czekamy na input dla poprzedniej linii
				TextLabel.Clear();
				_fullText = _dialogLines[_currentLineIndex];
				_currentCharIndex = 0;
				_timeSinceLastChar = 0f;
				_isTyping = true; // Rozpocznij pisanie nowej linii
				GD.Print($"Wyświetlam linię {_currentLineIndex + 1}/{_dialogLines.Length}");
			}
			else
			{
				// Wszystkie linie zostały wyświetlone, zamknij dialog
				HideDialog();
			}
		}
	}

	private void DisplayNextLine()
	{
		if (_currentLineIndex < _dialogLines.Length)
		{
			_waitingForInput = false;
			TextLabel.Clear();
			_fullText = _dialogLines[_currentLineIndex];
			_currentCharIndex = 0;
			_timeSinceLastChar = 0f;
			_isTyping = true;
			GD.Print($"Rozpoczynam linię {_currentLineIndex + 1}/{_dialogLines.Length}");
		}
		else
		{
			HideDialog(); // Powinno być obsłużone przez GoToNextLine, ale to zabezpieczenie
		}
	}

	/// <summary>
	/// Natychmiast wyświetla cały tekst bieżącej linii.
	/// </summary>
	public void SkipTyping()
	{
		if (_isTyping)
		{
			TextLabel.Clear();
			TextLabel.AppendText(_fullText);
			_currentCharIndex = _fullText.Length;
			_isTyping = false;
			_waitingForInput = true; // Po pominięciu czekaj na input
			GD.Print("Pisanie pominięte. Czekam na input gracza.");
		}
	}

	/// <summary>
	/// Ukrywa okno dialogowe i resetuje stan.
	/// </summary>
	public void HideDialog()
	{
		Visible = false;
		_isTyping = false;
		_waitingForInput = false;
		TextLabel.Clear();
		_dialogCompletionSource?.TrySetResult(false); // Zakończ Task z wynikiem false (np. anulowano)
		_dialogCompletionSource = null; // Zresetuj TaskCompletionSource
		_dialogLines = null; // Wyczyść linie dialogowe
		_currentLineIndex = 0; // Resetuj indeks linii
		TextLabel.AddThemeColorOverride(RichTextLabelColorOverrideName, DefaultTextColor);
		GD.Print("Dialog został ukryty.");
	}

	// Publiczne właściwości do sprawdzania stanu dialogu
	public bool IsTyping => _isTyping;
	public bool IsWaitingForInput => _waitingForInput;
	public bool IsDialogActive => Visible; // Sprawdza czy DialogBox jest widoczny
}

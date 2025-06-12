using Godot;
using System;
using System.Collections.Generic;

public class CardData
{
	public string Name { get; set; }
	public Rect2 CardRegions { get; set; }
	public string[] Description { get; set; }
	public string Effect { get; set; }
	public string Target { get; set; }
	public int Damage { get; set; }
	
	public CardData(string name, Rect2 cardRegions, string[] description)
	{
		Name = name;
		CardRegions = cardRegions;
		Description = description;
	}
	
	public CardData(string name, Rect2 cardRegions, string[] description, string effect, string target)
	{
		Name = name;
		CardRegions = cardRegions;
		Description = description;
		Target = target;
		Effect = effect;
	}
	
	public CardData(string name, Rect2 cardRegions, string[] description, int damage)
	{
		Name = name;
		CardRegions = cardRegions;
		Description = description;
		Damage = damage;
	}
	
	public CardData(string name, Rect2 cardRegions, string[] description, string effect, string target, int damage)
	{
		Name = name;
		CardRegions = cardRegions;
		Description = description;
		Effect = effect;
		Target = target;
		Damage = damage;
	}
}

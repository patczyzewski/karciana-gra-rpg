using Godot;
using System;

public partial class BattleTrigger : Node2D
{
	private CustomSignals _customSignals;
	
	public override void _Ready()
	{
		_customSignals = GetNode<CustomSignals>("/root/CustomSignals");
	}
	
	private void OnArea2DBodyEntered(Player body)
	{
		if (body == null) { return; }
		_customSignals.EmitSignal(nameof(CustomSignals.BattleStarted), true);
	}
	
	private void OnArea2DBodyExited(Player body)
	{
		if (body == null) { return; }
		_customSignals.EmitSignal(nameof(CustomSignals.BattleStarted), true);
	}
	
	public override void _Process(double delta)
	{
		
	}
}

using Godot;
using System;

public partial class Enemy : Node2D
{
	private AnimatedSprite2D _statusIcons;
	private AnimatedSprite2D _effectAnimations;
	private PointLight2D _statusIconsLight;
	
	public override void _Ready()
	{
		_statusIcons = GetNode<AnimatedSprite2D>("StatusIcons");
		_statusIconsLight = GetNode<PointLight2D>("StatusIcons/PointLight2D");
		_effectAnimations = GetNode<AnimatedSprite2D>("EffectAnimations");
		
		ChangeStatus("none");
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
	
	public void ChangeStatus(string status)
	{
		switch(status)
		{
			case "none":
				_statusIcons.Visible = false;
				_statusIconsLight.Visible = false;
				break;
			case "attack":
				_statusIcons.Visible = true;
				_statusIconsLight.Visible = true;
				_statusIcons.Play("attack");
				break;
			case "attack_buff":
				_statusIcons.Visible = true;
				_statusIconsLight.Visible = true;
				_statusIcons.Play("attack_buff");
				break;
			case "bulb":
				_statusIcons.Visible = true;
				_statusIconsLight.Visible = true;
				_statusIcons.Play("bulb");
				break;
			case "confusion":
				_statusIcons.Visible = true;
				_statusIconsLight.Visible = true;
				_statusIcons.Play("confusion");
				break;
			case "defense_buff":
				_statusIcons.Visible = true;
				_statusIconsLight.Visible = true;
				_statusIcons.Play("defense_buff");
				break;
			case "dread":
				_statusIcons.Visible = true;
				_statusIconsLight.Visible = true;
				_statusIcons.Play("dread");
				break;
			case "exclamation_mark":
				_statusIcons.Visible = true;
				_statusIconsLight.Visible = true;
				_statusIcons.Play("exclamation_mark");
				break;
		}
		
	}
}

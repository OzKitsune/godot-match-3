using Godot;
using System;

public partial class MainMenu : Control
{
	[Export] private Button _playButton;
	[Export] private PackedScene _gameScene;

	public override void _Ready()
	{
		_playButton.Pressed += StartGame;
    }

	private void StartGame() 
	{
		GetTree().ChangeSceneToPacked(_gameScene);
	}
}

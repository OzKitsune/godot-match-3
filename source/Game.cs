using Godot;
using System;

public partial class Game : Node2D
{
    [Export] private Board _board;

    [Export] private Timer _timer;
	[Export] private double _gameTime = 60.0;

    [Export] private ScoreLabel _scoreLabel;
    [Export] private Control _gameOverScreen;

	public GameState State { get; private set; } = GameState.Input;

    private int _score = 0;

    public override void _Ready()
    {
        _board.AnimationStarted += OnBoardAnimationStarted;
        _board.AnimationFinished += OnBoardAnimationFinished;

        _timer.Timeout += End;

        Start();
    }

    private void Start()
    {
        _timer.Start(_gameTime);

        _board.Fill();
    }

    public void AddScore(int score) 
    {
        _score += score;
        _scoreLabel.SetScore(_score);
    }

    private void OnBoardAnimationStarted()
    {
        State = GameState.Busy;
    }

    private void OnBoardAnimationFinished()
    {
        State = GameState.Input;
    }

	private void End()
    {
        State = GameState.GameOver;

        GetNode<Profile>("/root/Profile").Highscore = _score;

        _board.ProcessMode = ProcessModeEnum.Disabled;

		_gameOverScreen.Show();
    }
}

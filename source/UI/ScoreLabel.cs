using Godot;
using System;

public partial class ScoreLabel : Label
{
    public override void _Ready()
    {
        SetScore(0);
    }

    public void SetScore(int score) 
    {
        Text = $"Score: {score}";
    }
}

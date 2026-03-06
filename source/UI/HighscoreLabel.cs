using Godot;
using System;
using static System.Formats.Asn1.AsnWriter;

public partial class HighscoreLabel : Label
{
	public override void _Ready()
	{
        var highscore = GetNode<Profile>("/root/Profile").Highscore;
        Text = $"Highscore: {highscore}";
    }
}

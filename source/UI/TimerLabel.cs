using Godot;
using System;

public partial class TimerLabel : Label
{
	[Export] private Timer _timer;

    public override void _Process(double delta)
	{
		Text = _timer.TimeLeft.ToString("F0");
	}
}

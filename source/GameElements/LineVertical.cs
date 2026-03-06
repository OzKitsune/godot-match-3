using Godot;
using System;

[Tool]
public partial class LineVertical : BonusGameElement
{
    private static readonly PackedScene _scene = GD.Load<PackedScene>("res://scenes/GameElements/LineVertical.tscn");

    public static LineVertical Create(AGameElement parentElement)
    {
        var line = _scene.Instantiate<LineVertical>();
        line.Type = parentElement.Type;
        line.Color = parentElement.Color;
        return line;
    }
}

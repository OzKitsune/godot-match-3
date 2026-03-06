using Godot;
using System;

[Tool]
public partial class LineVertical : BonusGameElement
{
    private static readonly PackedScene _scene = GD.Load<PackedScene>("res://scenes/GameElements/LineVertical.tscn");

    public static LineVertical Create(Type type, Color color)
    {
        var line = _scene.Instantiate<LineVertical>();
        line.Type = type;
        line.Color = color;
        return line;
    }
}

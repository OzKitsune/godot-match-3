using Godot;
using System;

[Tool]
public partial class LineHorizontal : BonusGameElement
{
    private static readonly PackedScene _scene = GD.Load<PackedScene>("res://scenes/GameElements/LineHorizontal.tscn");

    public static LineHorizontal Create(Type type, Color color)
    {
        var line = _scene.Instantiate<LineHorizontal>();
        line.Type = type;
        line.Color = color;
        return line;
    }
}

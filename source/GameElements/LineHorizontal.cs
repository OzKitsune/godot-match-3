using Godot;
using System;

[Tool]
public partial class LineHorizontal : BonusGameElement
{
    private static readonly PackedScene _scene = GD.Load<PackedScene>("res://scenes/GameElements/LineHorizontal.tscn");

    public static LineHorizontal Create(AGameElement parentElement)
    {
        var line = _scene.Instantiate<LineHorizontal>();
        line.Type = parentElement.Type;
        line.Color = parentElement.Color;
        return line;
    }
}

using Godot;
using System;

public partial class MatchLine : RefCounted
{
    public Type Type { get; }
    public Color Color { get; }
    public Direction Direction { get; }
    public Vector2I[] Points { get; }
    public int Length { get; }

    public MatchLine(Type type, Color color, Direction direction, params Vector2I[] points)
    {
        Type = type;
        Color = color;
        Direction = direction;
        Points = points;
        Length = points.Length;
    }
}

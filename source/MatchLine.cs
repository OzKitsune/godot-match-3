using Godot;
using System;

public partial class MatchLine : RefCounted
{
    public Type Type { get; }
    public Direction Direction { get; }
    public Vector2I[] Points { get; }
    public int Length { get; }

    public MatchLine(Type type, Direction direction, params Vector2I[] points)
    {
        Type = type;
        Direction = direction;
        Points = points;
        Length = points.Length;
    }
}

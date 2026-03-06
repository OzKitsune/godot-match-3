using Godot;
using System;

[Tool]
public partial class SimpleGameElement : AGameElement
{
    private static readonly PackedScene[] _scenes =
    [
        GD.Load<PackedScene>("res://scenes/GameElements/Circle.tscn"),
        GD.Load<PackedScene>("res://scenes/GameElements/Diamond.tscn"),
        GD.Load<PackedScene>("res://scenes/GameElements/Hexagon.tscn"),
        GD.Load<PackedScene>("res://scenes/GameElements/Rhomb.tscn"),
        GD.Load<PackedScene>("res://scenes/GameElements/Triangle.tscn"),
    ]; 

    public static SimpleGameElement CreateRandom()
    {
        var scene = _scenes[GD.RandRange(0, _scenes.Length - 1)];

        var element = scene.Instantiate<SimpleGameElement>();
        element.Type = element.GetType();
        return element;
    }
}

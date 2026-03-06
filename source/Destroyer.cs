using Godot;
using System;
using System.Xml.Linq;

public partial class Destroyer : Sprite2D
{
    [Signal] public delegate void DestroyElementEventHandler(AGameElement gameElement);
    [Signal] public delegate void DoneEventHandler();

    [Export] private float _speed = 512.0f;

    [Export] private Area2D _area;

    private Vector2 _direction;
    private bool _launched = false;

    private Vector2 _boardSize;

	public override void _Ready()
	{
        _area.AreaEntered += OnAreaEntered;
	}

    public override void _PhysicsProcess(double delta)
    {
        Position += _direction * _speed * (float)delta;

        if (Position.X < - 64 || Position.X > _boardSize.X + 64 || Position.Y < 0 - 64 || Position.Y > _boardSize.Y + 64)
        {
            EmitSignal(SignalName.Done);
            QueueFree();
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.GetParent() != null) 
        {
            if (area.GetParent() is AGameElement element) 
            {
                EmitSignal(SignalName.DestroyElement, element);
            }
        }
    }

    public void Launch(Vector2 start, Vector2 direction, Vector2 boardSize)
    {
        Position = start;
        _direction = direction;
        _boardSize = boardSize;

        if (direction == Vector2.Left)
        {
            RotationDegrees = 270;
        }
        else if (direction == Vector2.Right) 
        {
            RotationDegrees = 90;
        }
        else if (direction == Vector2.Down)
        {
            RotationDegrees = 180;
        }

        _launched = true;
    }
}

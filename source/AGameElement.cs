using Godot;
using System;

public abstract partial class AGameElement : Sprite2D
{
    [Signal] public delegate void ClickEventHandler(AGameElement gameElement);

    [Export] private Area2D _area;

    /// <summary>
    /// Цвет элемента.
    /// </summary>
    [Export] public Color Color 
    {
        get 
        {
            return Modulate;
        }
        protected set 
        {
            Modulate = value;
        }
    }

    [Export] public int Score { get; protected set; } = 1;

    private Color _hoverColor = new Color(0.75f, 0.75f, 0.75f);

    private Tween _scaleTweenLoop;

    /// <summary>
    /// Позиция на игровом поле.
    /// </summary>
    public Vector2I BoardPosition { get; set; }

    public Type Type { get; protected set; }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        //Type = GetType();

        _area.MouseEntered += OnAreaMouseEntered;
        _area.MouseExited += OnAreaMouseExited;
        _area.InputEvent += OnAreaInputEvent;

        _scaleTweenLoop = CreateTween().SetLoops();
        _scaleTweenLoop.TweenProperty(this, "scale", new Vector2(1.2f, 1.2f), 0.5).SetTrans(Tween.TransitionType.Bounce);
        _scaleTweenLoop.TweenProperty(this, "scale", Vector2.One, 0.5);
        _scaleTweenLoop.Stop();
    }

    private void OnAreaMouseEntered()
    {
        Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);
        SelfModulate = _hoverColor;
    }

    private void OnAreaMouseExited()
    {
        Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
        SelfModulate = Colors.White;
    }

    private void OnAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton eventMouseButton) 
        {
            if (eventMouseButton.IsReleased() && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                EmitSignal(SignalName.Click, this);
            }
        }
    }

    public virtual void Select()
    {
        _scaleTweenLoop.Play();
    }

    public virtual void Deselect()
    {
        _scaleTweenLoop.Stop();

        var tween = CreateTween();
        tween.TweenProperty(this, "scale", Vector2.One, 0.1);
    }

    public virtual async void Destroy()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "scale", new Vector2(1.3f, 1.3f), 0.2).SetTrans(Tween.TransitionType.Bounce);
        tween.Parallel().TweenProperty(this, "modulate", new Color(1, 1, 1, 0), 0.2);
        await ToSignal(tween, Tween.SignalName.Finished);
        QueueFree();
    }
}

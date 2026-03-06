using Godot;
using System;

[Tool]
public partial class Bomb : BonusGameElement
{
    private static readonly PackedScene _scene = GD.Load<PackedScene>("res://scenes/GameElements/Bomb.tscn");

    [Export] private Sprite2D _flashlightSprite;

    /// <summary>
    /// Область поражения 3х3 с центром в том месте, где взорвался бонус.
    /// </summary>
    public Vector2I[] ExplosionArea 
    {
        get 
        {
            return [
                BoardPosition + new Vector2I(-1, -1),
                BoardPosition + new Vector2I(0, -1),
                BoardPosition + new Vector2I(1, -1),
                BoardPosition + new Vector2I(-1, 0),
                BoardPosition + new Vector2I(0, 0),
                BoardPosition + new Vector2I(1, 0),
                BoardPosition + new Vector2I(-1, 1),
                BoardPosition + new Vector2I(0, 1),
                BoardPosition + new Vector2I(1, 1),
            ];
        }
    }

    public static Bomb Create(AGameElement parentElement)
    {
        var bomb = _scene.Instantiate<Bomb>();
        bomb.Type = parentElement.Type;
        bomb.Color = parentElement.Color;
        return bomb;
    }

    public override void Activate()
    {
        base.Activate();

        var tween = _flashlightSprite.CreateTween();
        tween.TweenProperty(_flashlightSprite, "modulate", new Color(1, 1, 1, 1), 0.15).SetTrans(Tween.TransitionType.Bounce);
    }
}

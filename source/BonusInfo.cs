using Godot;
using System;

public partial class BonusInfo : RefCounted
{
    public Type ParentType { get; }
    public Type BonusType { get; }
    public Color Color { get; }
    public Vector2I Position { get; }

    public BonusInfo(Type parentType, Type bonusType, Color color, Vector2I position) 
    {
        ParentType = parentType;
        BonusType = bonusType;
        Color = color;
        Position = position;
    }

    public BonusGameElement CreateBonus() 
    {
        if (BonusType == typeof(Bomb))
        {
            return Bomb.Create(ParentType, Color);
        }
        else if (BonusType == typeof(LineHorizontal)) 
        {
            return LineHorizontal.Create(ParentType, Color);
        }
        else
        {
            return LineVertical.Create(ParentType, Color);
        }
    }
}

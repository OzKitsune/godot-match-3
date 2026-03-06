using Godot;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;

public partial class BonusInfo : RefCounted
{
    public Type SourceType { get; }
    public Type BonusType { get; }
    public Color Color { get; }
    public Vector2I Position { get; }

    private static readonly Dictionary<Type, Func<Type, Color, BonusGameElement>> _factories = new()
    {
        [typeof(Bomb)] = (sourceType, color) => Bomb.Create(sourceType, color),
        [typeof(LineHorizontal)] = (sourceType, color) => LineHorizontal.Create(sourceType, color),
        [typeof(LineVertical)] = (sourceType, color) => LineVertical.Create(sourceType, color)
    };

    public BonusInfo(Type sourceType, Type bonusType, Color color, Vector2I position) 
    {
        SourceType = sourceType;
        BonusType = bonusType;
        Color = color;
        Position = position;
    }

    public BonusGameElement CreateBonus() 
    {
        if (_factories.ContainsKey(BonusType))
        {
            return _factories[BonusType](SourceType, Color);
        }

        throw new InvalidOperationException($"Незарегистрированный тип бонуса: {BonusType}");
    }
}

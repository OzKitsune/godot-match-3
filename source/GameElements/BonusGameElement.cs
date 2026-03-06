using Godot;
using System;

[Tool]
public partial class BonusGameElement : AGameElement
{
    public bool IsActivated { get; protected set; } = false;

    public virtual void Activate()
    {
        IsActivated = true;
    }
}

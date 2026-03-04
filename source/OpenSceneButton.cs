using Godot;
using System;

public partial class OpenSceneButton : Button
{
    [Export(PropertyHint.File, "*.tscn")] private string _sceneToLoad;

    public override void _Pressed()
    {
        GetTree().ChangeSceneToFile(_sceneToLoad);
    }
}

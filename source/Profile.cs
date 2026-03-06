using Godot;
using System;
using static Godot.TextServer;

public partial class Profile : Node
{
    const string PATH_TO_PROFILE = "user://profile";

    private int _highscore = 0;

    public int Highscore 
    {
        get 
        {
            return _highscore;
        }
        set 
        {
            if (_highscore > value) 
            {
                return;
            }

            _highscore = value;
        }
    }

    public override void _EnterTree()
    {
        LoadProfile();
    }

    public override void _ExitTree()
    {
        SaveProfile();
    }

    private void LoadProfile()
    {
        if (FileAccess.FileExists(PATH_TO_PROFILE))
        {
            var file = FileAccess.Open(PATH_TO_PROFILE, FileAccess.ModeFlags.Read);
            _highscore = (int)file.GetVar();
            file.Close();
        }
    }

    private void SaveProfile() 
    {
        var file = FileAccess.Open(PATH_TO_PROFILE, FileAccess.ModeFlags.Write);
        file.StoreVar(_highscore);
        file.Close();
    }
}

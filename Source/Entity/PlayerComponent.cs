using Godot;
using System;

namespace ZNet.Source.Entity;

[GlobalClass]
public partial class PlayerComponent : Node3D
{
    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);
        SetProcessShortcutInput(false);
        SetProcessUnhandledInput(false);
        SetProcessUnhandledKeyInput(false);
    }


}

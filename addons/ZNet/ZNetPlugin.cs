#if TOOLS
using Godot;
using System;

[Tool]
public partial class ZNetPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		AddAutoloadSingleton("ZNetSingleton", "res://addons/ZNet/Singletons/ZNetSingleton.tscn");
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton("ZNetSingleton");
	}
}
#endif

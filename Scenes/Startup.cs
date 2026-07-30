using Godot;

namespace ZNet.Scenes
{
    public partial class Startup : Node
    {
        public override void _Ready()
        {

        }

        public static void LoadOrReloadGame()
        {
            ZNetMultiplayer.Instance.GetTree().CallDeferred("change_scene_to_file", "res://Scenes/Startup.tscn");
        }
    }
}

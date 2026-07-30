using Godot;

namespace ZNet.Source.World
{
    public partial class ChunkLoader : Node3D
    {
        [Export] public int Distance = 24;

        protected Level Level = null;

        public override void _EnterTree()
        {
            Level = Level.FindAbove(this);
        }

        public override void _Ready()
        {
            ForceUpdate();
        }

        public void ForceUpdate()
        {
            if (Level == null)
                return;

        }
    }
}

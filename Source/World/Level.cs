using Godot;
using ZNet.Communication.Rpc;

namespace ZNet.Source.World
{
    public partial class Level : Node3D
    {

        private System.Collections.Generic.Dictionary<(float, float, float), Chunk> _chunks = new();

        private RpcSystem _rpc = new();

        public override void _Ready()
        {
            _rpc.RegisterByNode(this);
            _rpc.BindDelegates(this);

            SetProcess(false);
            SetPhysicsProcess(false);
            SetProcessInput(false);
            SetProcessShortcutInput(false);
            SetProcessUnhandledInput(false);
            SetProcessUnhandledKeyInput(false);

            GenerateWorld();
        }

        public void GenerateWorld()
        {
            var posTest = new Chunk.ChunkPos(Position);
        }

        public static Level FindAbove(Node node)
        {
            while (node != null)
            {
                if (node is Level level)
                    return level; 

                node = node.GetParent();
            }

            return null;
        }

    }
}

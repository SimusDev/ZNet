using Godot;
using Godot.Collections;
using System;

namespace ZNet.Prototyping.Components
{
    public partial class NetworkPlayerSpawner : NetworkSpawner
    {
        [Export] public PackedScene PlayerScene;
        [Export] public Array<Node> SpawnPoints = new();

        private Dictionary<int, Node> _players = new();

        public override void _Ready()
        {
            base._Ready();

            if (PlayerScene == null)
            {
                GD.PushError("Player scene is null!");
                return;
            }

            if (RootNode == null)
            {
                GD.PushError("RootNode is null!");
                return;
            }

            ZNetMultiplayer.ServerPeerConnected += OnServerPeerConnected;
            ZNetMultiplayer.ServerPeerDisconnected += OnServerPeerDisconnected;
        }

        private void OnServerPeerDisconnected(int id)
        {
            var playerNode = PlayerScene.Instantiate();
            playerNode.SetMultiplayerAuthority(id);
            RootNode.AddChild(playerNode);

            var pickedPoint = SpawnPoints.PickRandom();
            if (pickedPoint != null)
            {
                if (pickedPoint is Node2D point2d && playerNode is Node2D player2d)
                {
                    player2d.GlobalTransform = point2d.GlobalTransform;
                }

                if (pickedPoint is Node3D point3d && playerNode is Node3D player3d)
                {
                    player3d.GlobalTransform = point3d.GlobalTransform;
                }
            }

            _players[id] = playerNode;
        }

        private void OnServerPeerConnected(int id)
        {
            if (_players.TryGetValue(id, out var playerNode))
            {
                playerNode?.QueueFree();
                _players.Remove(id);
            }

        }
    }
}

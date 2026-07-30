using Godot;
using System;
using ZNet.Serialization;

namespace ZNet.Communication
{
    public partial class SystemBase : RefCounted
    {
        protected ZNetMultiplayer _api;

        public ZNetMultiplayer Multiplayer => _api;
        
        protected ulong _netId = 0;

        [ThreadStatic] private static BinaryWriter _writer = new();
        [ThreadStatic] private static BinaryReader _reader = new();

        public int Authority = ZNetMultiplayer.ServerId;

        public void ApplyHashIdAndRegister(ulong id)
        {
            if (_api == null)
                _api = ZNetMultiplayer.Instance;

            _api.CommunicatorRegistry.Remove(id);
            _netId = id;
            _api.CommunicatorRegistry.Add(id, new WeakReference<SystemBase>(this));
        }

        public SystemBase()
        {
            _api = ZNetMultiplayer.Instance;
        }

        public SystemBase(ZNetMultiplayer multiplayer)
        {
            _api = multiplayer;
        }

        public void SetMultiplayer(ZNetMultiplayer multiplayer)
        {
            _api = multiplayer;
        }

        public ulong GenerateId()
        {
            return _api.GenerateId();
        }

        public ulong GetHashedNetworkID()
        {
            return _netId;
        }

        protected virtual string GetHashStringSalt()
        {
            return "Communicator";
        }

        public void RegisterByName(string name)
        {
            string hashStr = $"{GetHashStringSalt()}_{name}";
            ulong hashId = ZNetMultiplayer.HashStringDetermenistic(hashStr);
            ApplyHashIdAndRegister(hashId);
        }

        public void RegisterById(ulong id)
        {
            string hashInput = $"HashById_{id.ToString()}";
            RegisterByName(hashInput);
        }

        public void RegisterByNode(Node node)
        {
            if (node.IsInsideTree())
                GenerateNodePathHash(node);
            else
                GD.PushWarning($"Node must be in SceneTree. {node}");

            node.TreeEntered += () => GenerateNodePathHash(node);
            node.Renamed += () => GenerateNodePathHash(node);
        }

        private void GenerateNodePathHash(Node node)
        {
            string hashInput = $"HashByNode_{node.GetPath().ToString()}";
            RegisterByName(hashInput);
        }

        public void RegisterByResource(Resource resource)
        {
            if (!resource.ResourcePath.IsValidFileName())
            {
                GD.PushError($"Invalid Resource Path! {resource}, {resource.ResourcePath}");
                return;
            }

            string hashInput = $"HashByResourcePath_{resource.ResourcePath}";
            RegisterByName(hashInput);
        }

        public override void _Notification(int what)
        {
            if (what == NotificationPredelete)
            {
                _api.CommunicatorRegistry.Remove(_netId);
            }
        }


    }


}

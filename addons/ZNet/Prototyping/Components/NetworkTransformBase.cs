using Godot;
using System;
using ZNet.Communication;
using ZNet.Communication.Rpc;
using ZNet.Serialization;

namespace ZNet.Prototyping.Components
{
    public partial class NetworkTransformBase : Node
    {
        [Export] public bool SyncScale = false;

        [Export] double TickRate = 20;
        [Export] public bool Interpolate = true;
        [Export] public float InterpolateScale = 20;

        private double _tickTime = 0;

        protected new RpcSystem Rpc = new();

        private static BinaryWriter _writer = new();
        private static BinaryReader _reader = new();

        public override void _Ready()
        {
            Rpc.RegisterByNode(this);
            Rpc.BindDelegates(this);
            Rpc.Authority = GetMultiplayerAuthority();
            NetworkInitialize();
        }

        public void SetAuthority(int  authority, bool recursive = true)
        {
            SetMultiplayerAuthority(authority, recursive);
            if (Rpc != null)
                Rpc.Authority = authority;
        }

        public override void _Process(double delta)
        {
            _tickTime += delta;

            if (Rpc.Authority != Rpc.Multiplayer.UniqueId && Interpolate)
                InterpolateProcess(delta);

            if (_tickTime >= (1.0 / TickRate))
            {
                if (Rpc.Multiplayer.IsServer)
                {
                    _writer.Reset();
                    Serialize(_writer);
                    Rpc.Invoke(ClientReceiveFromServer, _writer.ToArray());
                    
                }

                if (Rpc.Authority == Rpc.Multiplayer.UniqueId)
                {
                    _writer.Reset();

                    Serialize(_writer);
                    Rpc.Invoke(ServerReceiveFromAuth, _writer.ToArray());
                }

                _tickTime = 0;

            }
        }

        protected virtual void InterpolateProcess(double delta)
        {

        }

        public void SetMultiplayer(ZNetMultiplayer api)
        {
            if (Rpc != null)
                Rpc.SetMultiplayer(api);
        }

        public void SetObservers(NetObservers observers)
        {
            if (Rpc != null)
                Rpc.Observers = observers;
        }

        protected virtual void NetworkInitialize()
        {

        }

        [RemoteFunc(RpcType.AuthToServer, SendMode = SendMode.Unreliable)]
        protected void ServerReceiveFromAuth(byte[] data)
        {
            _reader.SetBuffer(data);
            _reader.Seek(0);
            Deserialize(_reader);
        }

        [RemoteFunc(RpcType.ToObserver, SendMode = SendMode.Unreliable)]
        protected void ClientReceiveFromServer(byte[] data)
        {
            if (Rpc.Multiplayer.UniqueId == GetMultiplayerAuthority())
                return;

            _reader.SetBuffer(data);
            _reader.Seek(0);
            Deserialize(_reader);
        }

        protected virtual void Serialize(BinaryWriter writer)
        {

        }

        protected virtual void Deserialize(BinaryReader reader)
        {

        }

    }
}

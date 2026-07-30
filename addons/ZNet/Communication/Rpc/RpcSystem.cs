using Godot;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ZNet.Serialization;

namespace ZNet.Communication.Rpc
{
    public partial class RpcSystem: SystemBase
    {
        [ThreadStatic] private static object[] _tempArgs = new object[4];

        [ThreadStatic] private static BinaryWriter _writerSend = new();

        [ThreadStatic] private static BinaryWriter _writerLocal = new();
        [ThreadStatic] private static BinaryReader _readerLocal = new();

        private Dictionary<ushort, Delegate> _idToDelegate = new();
        private Dictionary<Delegate, ushort> _delegateToId = new();

        private Dictionary<ushort, RpcConfig> _idToConfig = new();
        private Dictionary<Delegate, RpcConfig> _delegateToConfig = new();

        private ushort _nextMethodId = 0;

        public NetObservers Observers = null;

        private int _remoteSenderId = 0;
        public int RemoteSenderId => _remoteSenderId;

        public RpcSystem()
        {
            _api = ZNetMultiplayer.Instance;
        }

        public RpcSystem(ZNetMultiplayer multiplayer)
        {
            _api = multiplayer;
        }

        public void BindDelegates(object target)
        {
            var type = target.GetType();

            var methods = type.GetMethods(
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public
            );

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<RemoteFunc>();
                if (attr == null) continue;

                var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                var delegateType = paramTypes.Length switch
                {
                    0 => typeof(Action),
                    1 => typeof(Action<>).MakeGenericType(paramTypes),
                    2 => typeof(Action<,>).MakeGenericType(paramTypes),
                    3 => typeof(Action<,,>).MakeGenericType(paramTypes),
                    4 => typeof(Action<,,,>).MakeGenericType(paramTypes),
                    _ => typeof(Delegate)
                };

                Delegate del;

                if (delegateType == typeof(Delegate))
                {
                    del = Delegate.CreateDelegate(typeof(Delegate), target, method);
                }
                else
                {
                    del = Delegate.CreateDelegate(delegateType, target, method);
                }

                var config = new RpcConfig
                {
                    RpcType = attr.Type,
                    SendMode = attr.SendMode,
                    Channel = attr.Channel,
                    RunLocally = attr.RunLocally
                };

                BindDelegate(del, config);

            }
        }

        public bool TryGetDelegateConfig(Delegate @delegate, out RpcConfig config)
        {
            return _delegateToConfig.TryGetValue(@delegate, out config);
        }

        public void BindDelegate(Delegate @delegate, RpcConfig config)
        {
            _idToDelegate[_nextMethodId] = @delegate;
            _delegateToId[@delegate] = _nextMethodId;
            _idToConfig[_nextMethodId] = config;
            _delegateToConfig[@delegate] = config;

            _nextMethodId++;
        }

        public void BindDelegate(Delegate @delegate)
        {
            BindDelegate(@delegate, new());
        }

        protected override string GetHashStringSalt()
        {
            return "RpcSystem";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsContainsDelegate(Delegate method)
        {
            if (!_delegateToId.ContainsKey(method))
            {
                GD.PushError($"Delegate {method}, {method.Method.Name} was not found in bundle");
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Delegate GetDelegateByRpcId(ushort rpcId)
        {
            if (_idToDelegate.TryGetValue(rpcId, out var @delegate))
                return @delegate;

            GD.PushError($"Delegate by RpcId {rpcId} was not found in bundle");
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateRpc(RpcConfig config, int peer, int authority)
        {
            return config.RpcType switch
            {
                RpcType.AuthToServer => peer == authority,
                RpcType.ToObserver => peer == ZNetMultiplayer.ServerId,
                _ => true
            };
        }

        private bool ValidateRpcWithError(Delegate method, RpcConfig config, int peer, int authority)
        {
            bool validation = ValidateRpc(config, peer, authority);
            if (!validation)
                GD.PushError($"Rpc {method.Method.Name} Validation Failed for peer {peer}, {config.RpcType.ToString()}");
            return validation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvokeReceivedRpc(Delegate @delegate, object[] allocArgs, byte argsLength)
        {
            switch(argsLength)
            {
                case 0:
                    break;
                case 1:
                    @delegate.DynamicInvoke(allocArgs[0]);
                    break;
                case 2:
                    @delegate.DynamicInvoke(allocArgs[0], allocArgs[1]);
                    break;
                case 3:
                    @delegate.DynamicInvoke(allocArgs[0], allocArgs[1], allocArgs[2]);
                    break;
                case 4:
                    @delegate.DynamicInvoke(allocArgs[0], allocArgs[1], allocArgs[2], allocArgs[3]);
                    break;
                default:
                    object[] dynamicArgs = new object[argsLength];
                    for (byte i = 0; i < argsLength; i++)
                    {
                        dynamicArgs[i] = allocArgs[i];
                    }

                    @delegate.DynamicInvoke(dynamicArgs);

                    break;
            }
        }

        public void OnRawDataReceived(int peerId, BinaryReader reader)
        {
            ushort methodId = reader.ReadUShort();

            Delegate @delegate = GetDelegateByRpcId(methodId);
            if (@delegate == null)
                return;

            if (_api.IsServer)
            {
                RpcConfig config = _idToConfig[methodId];
                if (!ValidateRpcWithError(@delegate, config, peerId, Authority))
                    return;

            }

            byte argsLength = reader.ReadByte();

            object[] args = ArrayPool<object>.Shared.Rent(argsLength);

            try
            {
                for (byte i = 0; i < argsLength; i++)
                {
                    args[i] = ObjectSerializer.Read(reader);
                }

                InvokeReceivedRpc(@delegate, args, argsLength);

            }

            finally
            {
                ArrayPool<object>.Shared.Return(args, clearArray: true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasObservers()
        {
            if (Observers != null)
                return Observers.Count > 0;
            return true;
        }

        private bool HasObserversOrClient()
        {
            if (_api.IsServer)
                return HasObservers();
            return true;
        }

        private void InvokeLocally(Delegate method, object[] args, int argsLength)
        {
            _writerLocal.Reset();

            for (int i = 0; i < argsLength; i++)
            {
                ObjectSerializer.Write(_writerLocal, args[i]);
            }

            object[] newArgs = new object[argsLength];

            _readerLocal.SetBuffer(_writerLocal.GetBuffer());
            _readerLocal.Seek(0);

            for (int i = 0; i < argsLength; i++)
            {
                newArgs[i] = ObjectSerializer.Read(_readerLocal);
                //GD.Print(newArgs[i].GetType());
            }

            _remoteSenderId = _api.UniqueId;
            method.DynamicInvoke(newArgs);
            _remoteSenderId = 0;
        }


        private void InvokeInternal(Delegate method, object[] args, int argsLength)
        {
            if (!IsContainsDelegate(method))
                return;

            ushort rpcId = _delegateToId[method];
            RpcConfig config = _delegateToConfig[method];

            if (_api.IsServer)
            {
                if (config.RpcType == RpcType.ToServer && config.RunLocally)
                {
                    InvokeLocally(method, args, argsLength);
                    return;
                }

                _writerSend.Reset();
                SerializePacketTypeAndHashId(_writerSend);
                SerializeRpc(_writerSend, rpcId, args, argsLength);
                var data = _writerSend.GetSpan();

                if (Observers != null)
                {
                    foreach (int observer in Observers.GetObserversArray())
                    {
                        _api.SendTo(observer, data, config.Channel, (ZNetMultiplayer.SendMode)config.SendMode);
                    }
                }

                else
                {
                    var connections = _api.GetConnections();
                    if (config.RunLocally)
                    {
                        InvokeLocally(method, args, argsLength);
                    }

                    foreach (var pair in connections)
                    {
                        _api.SendTo(pair.Key, data, config.Channel, (ZNetMultiplayer.SendMode)config.SendMode);
                    }
                }

                return;
            }

            if (!ValidateRpcWithError(method, config, _api.UniqueId, Authority))
                return;

            _writerSend.Reset();
            SerializePacketTypeAndHashId(_writerSend);
            SerializeRpc(_writerSend, rpcId, args, argsLength);
            
            var clientData = _writerSend.GetSpan();
            _api.SendToServer(clientData, config.Channel, (ZNetMultiplayer.SendMode)config.SendMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SerializePacketTypeAndHashId(BinaryWriter writer)
        {
            writer.WriteByte((byte)ZNetMultiplayer.PacketType.RpcSystem);
            writer.WriteULong(_netId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SerializeRpc(BinaryWriter writer, ushort rpcId, object[] args, int argsLength)
        {
            writer.WriteUShort(rpcId);
            writer.WriteByte((byte)argsLength);

            switch (argsLength)
            {
                case 0:
                    break;
                case 1:
                    ObjectSerializer.Write(writer, args[0]);
                    break;
                case 2:
                    ObjectSerializer.Write(writer, args[0]);
                    ObjectSerializer.Write(writer, args[1]);
                    break;
                case 3:
                    ObjectSerializer.Write(writer, args[0]);
                    ObjectSerializer.Write(writer, args[1]);
                    ObjectSerializer.Write(writer, args[2]);
                    break;
                case 4:
                    ObjectSerializer.Write(writer, args[0]);
                    ObjectSerializer.Write(writer, args[1]);
                    ObjectSerializer.Write(writer, args[2]);
                    ObjectSerializer.Write(writer, args[3]);
                    break;
                default:
                    for (byte i = 0; i < argsLength; i++)
                    {
                        ObjectSerializer.Write(writer, args[i]);
                    }

                    break;
            }

        }

        public void Invoke(Delegate method)
        {
            if (!HasObserversOrClient())
                return;

            InvokeInternal(method, _tempArgs, 0);
        }

        public void Invoke(Delegate method, params object[] args)
        {
            if (!HasObserversOrClient())
                return;

            InvokeInternal(method, args, args.Length);
        }

        public void Invoke<A1>(Delegate method, A1 arg1)
        {
            if (!HasObserversOrClient())
                return;

            _tempArgs[0] = arg1;
            InvokeInternal(method, _tempArgs, 1);
        }

        public void Invoke<A1, A2>(Delegate method, A1 arg1, A2 arg2)
        {
            if (!HasObserversOrClient())
                return;

            _tempArgs[0] = arg1;
            _tempArgs[1] = arg2;
            InvokeInternal(method, _tempArgs, 2);
        }

        public void Invoke<A1, A2, A3>(Delegate method, A1 arg1, A2 arg2, A3 arg3)
        {
            if (!HasObserversOrClient())
                return;

            _tempArgs[0] = arg1;
            _tempArgs[1] = arg2;
            _tempArgs[2] = arg3;
            InvokeInternal(method, _tempArgs, 3);
        }

        public void Invoke<A1, A2, A3, A4>(Delegate method, A1 arg1, A2 arg2, A3 arg3, A4 arg4)
        {
            if (!HasObserversOrClient())
                return;

            _tempArgs[0] = arg1;
            _tempArgs[1] = arg2;
            _tempArgs[2] = arg3;
            _tempArgs[3] = arg4;
            InvokeInternal(method, _tempArgs, 4);
        }

        public void InvokeId(int peerId, Delegate method, params object[] args)
        {
            if (!CanInvokeById()) { return; }
            if (!IsContainsDelegate(method)) { return; }

            if (peerId == _api.UniqueId)
            {
                InvokeLocally(method, args, args.Length);
            }

            ushort rpcId = _delegateToId[method];
            RpcConfig config = _idToConfig[rpcId];
            _writerSend.Reset();
            SerializePacketTypeAndHashId(_writerSend);
            SerializeRpc(_writerSend, rpcId, args, args.Length);
            var data = _writerSend.GetSpan();
            _api.SendTo(peerId, data, config.Channel, (ZNetMultiplayer.SendMode)config.SendMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool CanInvokeById()
        {
            if (_api.IsServer)
                return true;

            GD.PushError("Only server can invoke by Id");
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvokeIdArrayInternal(int[] peers, Delegate method, object[] args, int argsLength)
        {
            ushort rpcId = _delegateToId[method];
            RpcConfig config = _idToConfig[rpcId];

            _writerSend.Reset();
            SerializePacketTypeAndHashId(_writerSend);
            SerializeRpc(_writerSend, rpcId, args, argsLength);
            var data = _writerSend.GetSpan();

            foreach (var peer in peers)
            {
                _api.SendTo(peer, data, config.Channel, (ZNetMultiplayer.SendMode)config.SendMode);
            }

        }

        public void InvokeIdArray(int[] peers, Delegate method)
        {
            if (!CanInvokeById()) { return; }
            if (!IsContainsDelegate(method)) { return; }
            InvokeIdArrayInternal(peers, method, _tempArgs, 0);
        }

        public void InvokeIdArray(int[] peers, Delegate method, params object[] args)
        {
            if (!CanInvokeById()) { return; }
            if (!IsContainsDelegate(method)) { return; }
            InvokeIdArrayInternal(peers, method, args, args.Length);
        }

        public void InvokeIdArray<A1>(int[] peers, Delegate method, A1 arg1)
        {
            if (!CanInvokeById()) { return; }
            if (!IsContainsDelegate(method)) { return; }

            _tempArgs[0] = arg1;
            InvokeIdArrayInternal(peers, method, _tempArgs, 1);
        }

        public void InvokeIdArray<A1, A2>(int[] peers, Delegate method, A1 arg1, A2 arg2)
        {
            if (!CanInvokeById()) { return; }
            if (!IsContainsDelegate(method)) { return; }

            _tempArgs[0] = arg1;
            _tempArgs[1] = arg2;
            InvokeIdArrayInternal(peers, method, _tempArgs, 2);
        }

        public void InvokeIdArray<A1, A2, A3>(int[] peers, Delegate method, A1 arg1, A2 arg2, A3 arg3)
        {
            if (!CanInvokeById()) { return; }
            if (!IsContainsDelegate(method)) { return; }

            _tempArgs[0] = arg1;
            _tempArgs[1] = arg2;
            _tempArgs[2] = arg3;
            InvokeIdArrayInternal(peers, method, _tempArgs, 3);
        }

        public void InvokeIdArray<A1, A2, A3, A4>(int[] peers, Delegate method, A1 arg1, A2 arg2, A3 arg3, A4 arg4)
        {
            if (!CanInvokeById()) { return; }
            if (!IsContainsDelegate(method)) { return; }

            _tempArgs[0] = arg1;
            _tempArgs[1] = arg2;
            _tempArgs[2] = arg3;
            _tempArgs[3] = arg4;
            InvokeIdArrayInternal(peers, method, _tempArgs, 4);
        }

    }

}

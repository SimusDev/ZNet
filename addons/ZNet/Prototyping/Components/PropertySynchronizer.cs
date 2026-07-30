using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ZNet.Communication.Rpc;
using ZNet.Serialization;

namespace ZNet.Prototyping.Components
{
	public partial class PropertySynchronizer: Node
	{
		private ZNetMultiplayer ZNetMultiplayer;

		[Export] public Node Target;
		[Export]
		string[] Properties = [];

		[Export] public float TickRate = 20;
		[Export] public bool Reliable = false;

		private double _tickTime = 0;

		private RpcSystem _rpcSystem = new RpcSystem();

		private Dictionary<string, Variant> _deltas = new();

		private BinaryWriter _writer = new();
		private BinaryReader _reader = new();

		private bool CheckPropertyDelta(string property)
		{
			if (_deltas.TryGetValue(property, out Variant data))
			{
				return !Get(property).Equals(data);
			}

			_deltas[property] = Get(property);
			return true;
		}

		public void SetAuthority(int auth, bool recursive)
		{
			SetMultiplayerAuthority(auth, recursive);
			
			if (_rpcSystem != null)
				_rpcSystem.Authority = auth;
		}

		public override void _Ready()
		{
			if (ZNetMultiplayer == null)
				ZNetMultiplayer = ZNetMultiplayer.Instance;

			_rpcSystem.SetMultiplayer(ZNetMultiplayer);
			_rpcSystem.RegisterByNode(this);
			_rpcSystem.BindDelegates(this);
			_rpcSystem.Authority = GetMultiplayerAuthority();

			ZNetMultiplayer.NetworkStatusChanged += OnNetworkStatusChanged;
			OnNetworkStatusChanged(ZNetMultiplayer.Status);

			SetPhysicsProcess(false);
			SetPhysicsProcessInternal(false);
			SetProcessInput(false);
			SetProcessShortcutInput(false);
			SetProcessUnhandledInput(false);
			SetProcessUnhandledKeyInput(false);
		}

		private void OnNetworkStatusChanged(ZNetMultiplayer.NetworkStatus status)
		{
			SetProcess(status == ZNetMultiplayer.NetworkStatus.Ready);
			if (status == ZNetMultiplayer.NetworkStatus.Ready)
			{
				_rpcSystem.Invoke(Send);
			}
		}

		[RemoteFunc(RpcType.ToServer, SendMode = SendMode.ReliableSequenced)]
		private void Send()
		{
			if (Properties.Length == 0)
				return;

			_writer.Reset();
			_writer.WriteVarInt(Properties.Length);
			for (int i = 0; i < Properties.Length; i++)
			{
				Variant value = Get(Properties[i]);
				_writer.WriteBytesDynamic(GD.VarToBytes(value));
			}

			_rpcSystem.InvokeId(_rpcSystem.RemoteSenderId, UnpackDetlasReliable, _writer.ToArray());
		}

		public override void _Process(double delta)
		{
			if (GetMultiplayerAuthority() == ZNetMultiplayer.UniqueId)
				_AuthorityProcess(delta);
		}

		private void _AuthorityProcess(double delta)
		{
			_tickTime += delta;
			if (_tickTime >= TickRate)
			{
				Synchronize();
				_tickTime = 0;
			}
		}

		public void Synchronize()
		{
			string[] changed = [];

			foreach (string property in Properties)
			{
				if (CheckPropertyDelta(property) || Reliable == false)
				{
					changed.Append(property);
				}
			}

			if (changed.Length < 1)
				return;

			_writer.WriteVarInt(changed.Length);

			foreach (string property in changed)
			{
				Variant value = Get(property);
				_writer.WriteBytesDynamic(GD.VarToBytes(value));
			}

			byte[] data = _writer.ToArray();

			Delegate rpcMethod;

			if (Reliable)
				rpcMethod = ReceiveFromClientRpcReliable;
			else
				rpcMethod = ReceiveFromClientRpcUnreliable;

			if (ZNetMultiplayer.IsServer)
			{
				if (Reliable)
					rpcMethod = UnpackDetlasReliable;
				else
					rpcMethod = UnpackDetlasUnreliable;
			}

			_rpcSystem.Invoke(rpcMethod, data);

		}

		[RemoteFunc(RpcType.AuthToServer, SendMode = SendMode.Unreliable)]
		private void ReceiveFromClientRpcUnreliable(byte[] data)
		{
			UnpackDeltas(data);
		}

		[RemoteFunc(RpcType.AuthToServer, SendMode = SendMode.ReliableSequenced)]
		private void ReceiveFromClientRpcReliable(byte[] data)
		{
			UnpackDeltas(data);
		}

		[RemoteFunc(RpcType.ToObserver, SendMode = SendMode.Unreliable)]
		private void UnpackDetlasUnreliable(byte[] data)
		{
			UnpackDeltas(data);
		}

		[RemoteFunc(RpcType.ToObserver, SendMode = SendMode.ReliableSequenced)]
		private void UnpackDetlasReliable(byte[] data)
		{
			UnpackDeltas(data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UnpackDeltas(byte[] data)
		{
			_reader.SetBuffer(data);
			_reader.Seek(0);

			int propertyCount = _reader.ReadVarInt();
			for (int i = 0; i < propertyCount; i++)
			{
				Variant value = GD.BytesToVar(_reader.ReadBytesDynamic());
				string property = Properties[i];
				Set(property, value);
			}
		}

	}
}
